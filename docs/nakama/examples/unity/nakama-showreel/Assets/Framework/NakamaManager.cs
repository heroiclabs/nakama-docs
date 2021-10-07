/**
 * Copyright 2017 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

namespace Framework
{
    public class NakamaManager : Singleton<NakamaManager>
    {
        internal const string HostIp = "127.0.0.1";
        internal const uint Port = 7350;
        internal const bool UseSsl = false;
        internal const string ServerKey = "defaultkey";

        private const int MaxReconnectAttempts = 5;

        private readonly Queue<Action> _dispatchQueue = new Queue<Action>();
        private readonly INClient _client;
        private readonly Action<INSession> _sessionHandler;

        public static event EventHandler AfterConnected = (sender, evt) => { };
        public static event EventHandler AfterDisconnected = (sender, evt) => { };

        private static readonly Action<INError> ErrorHandler = err =>
        {
            if (err.Code == ErrorCode.GroupNameInuse)
            {
                // This is caused by the FakeDataLoader and is expected. We can ignore this safely in this case.
                return;
            }
            Logger.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
        };

        private INAuthenticateMessage _authenticateMessage;

        // Flag to tell us whether the socket was closed intentially or not and whether to attempt reconnect.
        private bool _doReconnect = true;

        private uint _reconnectCount;

        public INSession Session { get; private set; }

        private NakamaManager()
        {
            _client = new NClient.Builder(ServerKey).Host(HostIp).Port(Port).SSL(UseSsl).Build();
            _client.OnTopicMessage = message =>
            {
                var chatMessages = StateManager.Instance.ChatMessages;
                foreach (var topic in chatMessages.Keys)
                {
                    if (topic.Id.Equals(message.Topic.Id))
                    {
                        chatMessages[topic].Add(message.MessageId, message);
                    }
                }
            };

            _sessionHandler = session =>
            {
                Logger.LogFormat("Session: '{0}'.", session.Token);
                _client.Connect(session, done =>
                {
                    Session = session;
                    _reconnectCount = 0;
                    // cache session for quick reconnects
                    _dispatchQueue.Enqueue(() =>
                    {
                        PlayerPrefs.SetString("nk.session", session.Token);
                        AfterConnected(this, EventArgs.Empty);
                    });
                });
            };

            _client.OnDisconnect = evt =>
            {
                Logger.Log("Disconnected from server.");
                if (_doReconnect && _reconnectCount < MaxReconnectAttempts)
                {
                    _reconnectCount++;
                    _dispatchQueue.Enqueue(() => { Reconnect(); });
                }
                else
                {
                    _dispatchQueue.Clear();
                    _dispatchQueue.Enqueue(() => { AfterDisconnected(this, EventArgs.Empty); });
                }
            };
            _client.OnError = error => ErrorHandler(error);
        }

        private IEnumerator Reconnect()
        {
            // if it's the first time disconnected, then attempt to reconnect immediately
            // every other time, wait 10,20,30,40,50 seconds each time 
            var reconnectTime = ((_reconnectCount - 1) + 10) * 60;
            yield return new WaitForSeconds(reconnectTime);
            _sessionHandler(Session);
        }

        // Restore serialised session token from PlayerPrefs
        // If the token doesn't exist or is expired `null` is returned.
        private static INSession RestoreSession()
        {
            var cachedSession = PlayerPrefs.GetString("nk.session");
            if (string.IsNullOrEmpty(cachedSession))
            {
                Logger.Log("No Session in PlayerPrefs.");
                return null;
            }

            var session = NSession.Restore(cachedSession);
            if (!session.HasExpired(DateTime.UtcNow)) return session;
            Logger.Log("Session expired.");
            return null;
        }

        private void Update()
        {
            for (int i = 0, l = _dispatchQueue.Count; i < l; i++)
            {
                _dispatchQueue.Dequeue()();
            }
        }

        private void OnApplicationQuit()
        {
            _doReconnect = false;
            _client.Disconnect();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                _doReconnect = false;
                _client.Disconnect();
                return;
            }

            // let's re-authenticate (if neccessary) and reconnect to the server.
            if (_authenticateMessage != null)
            {
                _doReconnect = true;
                Connect(_authenticateMessage);
            }
        }

        // This method connects the client to the server and
        // if neccessary authenticates with the server
        public void Connect(INAuthenticateMessage request)
        {
            // Check to see if we have a valid session token we can restore
            var session = RestoreSession();
            if (session != null)
            {
                // Session is valid, let's connect.
                _sessionHandler(session);
                return;
            }

            // Cache message for later use for reconnecting purposes in case the session had expired
            _authenticateMessage = request;

            // Let's login to authenticate and get a valid token
            _client.Login(request, _sessionHandler, err =>
            {
                if (err.Code == ErrorCode.UserNotFound)
                {
                    _client.Register(request, _sessionHandler, ErrorHandler);
                }
                else
                {
                    ErrorHandler(err);
                }
            });
        }

        public void SelfFetch(NSelfFetchMessage message)
        {
            _client.Send(message, self => { StateManager.Instance.SelfInfo = self; }, ErrorHandler);
        }

        public void FriendAdd(NFriendAddMessage message, bool refreshList = true)
        {
            _client.Send(message, done =>
            {
                if (refreshList)
                {
                    FriendsList(NFriendsListMessage.Default());
                }
            }, ErrorHandler);
        }

        public void FriendRemove(NFriendRemoveMessage message)
        {
            _client.Send(message, done => { FriendsList(NFriendsListMessage.Default()); }, ErrorHandler);
        }

        public void FriendsList(NFriendsListMessage message)
        {
            _client.Send(message, friends =>
            {
                StateManager.Instance.Friends.Clear();
                StateManager.Instance.Friends.AddRange(friends.Results);
            }, ErrorHandler);
        }

        public void GroupsList(NGroupsListMessage.Builder message, bool appendList = false, uint maxGroups = 100)
        {
            _client.Send(message.Build(), groups =>
            {
                if (!appendList)
                {
                    StateManager.Instance.SearchedGroups.Clear();
                }

                foreach (var group in groups.Results)
                {
                    // check to see if SearchedGroups has 'maxGroups' groups.
                    if (StateManager.Instance.SearchedGroups.Count >= maxGroups)
                    {
                        return;
                    }

                    StateManager.Instance.SearchedGroups.Add(group);
                }

                // Recursively fetch the next set of groups and append
                if (groups.Cursor != null && groups.Cursor.Value != "")
                {
                    message.Cursor(groups.Cursor);
                    GroupsList(message, true);
                }
            }, ErrorHandler);
        }

        public void GroupJoin(NGroupJoinMessage message, bool refreshList = true)
        {
            _client.Send(message, done =>
            {
                if (refreshList)
                {
                    JoinedGroupsList(NGroupsSelfListMessage.Default());
                }
            }, ErrorHandler);
        }

        public void JoinedGroupsList(NGroupsSelfListMessage message)
        {
            _client.Send(message, groups =>
            {
                StateManager.Instance.JoinedGroups.Clear();
                StateManager.Instance.JoinedGroups.AddRange(groups.Results);
            }, ErrorHandler);
        }

        public void GroupCreate(NGroupCreateMessage message)
        {
            _client.Send(message, groups => { }, ErrorHandler);
        }

        public void TopicJoin(string userIdOrRoom, NTopicJoinMessage message)
        {
            _client.Send(message, topics =>
            {
                var topic = topics.Results[0];
                StateManager.Instance.Topics.Add(userIdOrRoom, topic.Topic);
                StateManager.Instance.ChatMessages.Add(topic.Topic, new Dictionary<string, INTopicMessage>());
            }, ErrorHandler);
        }

        public void TopicMessageList(INTopicId topic, NTopicMessagesListMessage.Builder message,
            bool appendList = false, uint maxMessages = 100)
        {
            _client.Send(message.Build(), messages =>
            {
                if (!appendList)
                {
                    StateManager.Instance.ChatMessages[topic].Clear();
                }

                foreach (var chatMessage in messages.Results)
                {
                    // check to see if ChatMessages has 'maxMessages' messages.
                    if (StateManager.Instance.ChatMessages[topic].Count >= maxMessages)
                    {
                        return;
                    }

                    StateManager.Instance.ChatMessages[topic].Add(chatMessage.MessageId, chatMessage);
                }

                // Recursively fetch the next set of groups and append
                if (messages.Cursor != null && messages.Cursor.Value != "")
                {
                    message.Cursor(messages.Cursor);
                    TopicMessageList(topic, message, true);
                }
            }, ErrorHandler);
        }

        public void TopicSendMessage(NTopicMessageSendMessage message)
        {
            _client.Send(message, acks => { }, ErrorHandler);
        }

        public void NotificationsList(NNotificationsListMessage.Builder message,
            bool appendList = false, uint maxNotifications = 100)
        {
            _client.Send(message.Build(), notifications =>
            {
                if (!appendList)
                {
                    StateManager.Instance.Notifications.Clear();
                }

                foreach (var notification in notifications.Results)
                {
                    // check to see if ChatMessages has 'maxMessages' messages.
                    if (StateManager.Instance.Notifications.Count >= maxNotifications)
                    {
                        return;
                    }

                    StateManager.Instance.Notifications.Add(notification);
                }

                // Recursively fetch the next set of groups and append
                if (notifications.Cursor != null && notifications.Cursor.Value != "")
                {
                    message.Cursor(notifications.Cursor.Value);
                    NotificationsList(message, true);
                }
            }, ErrorHandler);
        }
    }
}