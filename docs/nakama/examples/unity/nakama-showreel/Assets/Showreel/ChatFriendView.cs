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

using System.Collections.Generic;
using Framework;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace Showreel
{
    public class ChatFriendView : MonoBehaviour
    {
        private Text _chatMessageLabel;
        private Button _sendMessageButton;
        private InputField _inputField;
        private Dropdown _friendSelectorDropdown;

        private string _chatMessages = "";

        // this is used to keep track of rendered messages for that user  
        private int _renderedMessages = 0;

        private void Start()
        {
            NakamaManager.Instance.FriendsList(NFriendsListMessage.Default());

            _chatMessageLabel = GameObject.Find("ChatMessageLabel").GetComponent<Text>();
            _sendMessageButton = GameObject.Find("SendMessageButton").GetComponent<Button>();
            _inputField = GameObject.Find("InputField").GetComponent<InputField>();
            _friendSelectorDropdown = GameObject.Find("FriendSelectorDropdown").GetComponent<Dropdown>();
            _friendSelectorDropdown.onValueChanged.AddListener(delegate { FriendSelectorDropdownOnChange(); });

            _chatMessages = "";
            _sendMessageButton.interactable = false;
            _friendSelectorDropdown.options.Clear();
            _friendSelectorDropdown.interactable = false;
        }

        private void Update()
        {
            // we've not initialized friend list yet, let's skip rendering anything yet.
            if (StateManager.Instance.Friends.Count == 0)
            {
                return;
            }

            // check if dropdown is already populated with all friends
            if (_friendSelectorDropdown.options.Count != StateManager.Instance.Friends.Count)
            {
                SubscribeToAllFriends();

                // lets add friend handles to the dropdown
                List<Dropdown.OptionData> friendList = new List<Dropdown.OptionData>();
                for (var i = 0; i < StateManager.Instance.Friends.Count; i++)
                {
                    friendList.Add(new Dropdown.OptionData(StateManager.Instance.Friends[i].Handle));
                }

                _friendSelectorDropdown.interactable = true;
                _friendSelectorDropdown.options.Clear();
                _friendSelectorDropdown.options.AddRange(friendList);
                _friendSelectorDropdown.value = 0;
                _friendSelectorDropdown.RefreshShownValue();
                FriendSelectorDropdownOnChange();
            }
            else
            {
                // if dropdown is already updated, proceed to update the text
                RenderMessages();
            }

            _chatMessageLabel.text = _chatMessages;
        }

        private void FriendSelectorDropdownOnChange()
        {
            _chatMessages = "";
            _renderedMessages = 0;
            if (_friendSelectorDropdown.options.Count == 0)
            {
                _friendSelectorDropdown.interactable = false;
                _sendMessageButton.interactable = false;
                return;
            }

            var user = StateManager.Instance.Friends[_friendSelectorDropdown.value];
            FetchHistoricMessages(user);

            _sendMessageButton.interactable = true;
        }

        private void SubscribeToAllFriends()
        {
            for (var i = 0; i < StateManager.Instance.Friends.Count; i++)
            {
                var userId = StateManager.Instance.Friends[i].Id;

                // join a chat topic for users that we haven't seen previously
                if (!StateManager.Instance.Topics.ContainsKey(userId))
                {
                    var msg = new NTopicJoinMessage.Builder().TopicDirectMessage(userId).Build();
                    NakamaManager.Instance.TopicJoin(userId, msg);
                }
            }
        }

        private void FetchHistoricMessages(INUser user)
        {
            var topic = StateManager.Instance.Topics[user.Id];
            var builder = new NTopicMessagesListMessage.Builder();
            builder.TopicDirectMessage(user.Id);
            NakamaManager.Instance.TopicMessageList(topic, builder);
        }

        private void RenderMessages()
        {
            var user = StateManager.Instance.Friends[_friendSelectorDropdown.value];

            INTopicId topic;
            var topicExists = StateManager.Instance.Topics.TryGetValue(user.Id, out topic);
            if (!topicExists)
            {
                return;
            }

            var friendMessages = StateManager.Instance.ChatMessages[topic];

            if (_renderedMessages == friendMessages.Count)
            {
                // nothing new to render
                return;
            }

            var chatMessages = new List<INTopicMessage>();
            chatMessages.AddRange(friendMessages.Values);
            chatMessages.Sort(new TopicMessageComparer());

            _chatMessages = "";
            foreach (var msg in chatMessages)
            {
                var chatMessage = JsonUtility.FromJson<ChatMessageContent>(msg.Data);
                _chatMessages += string.Format(@"
{0} said: {1}
				", msg.Handle, chatMessage.Body);
            }
        }

        public void SendDirectMessage()
        {
            var user = StateManager.Instance.Friends[_friendSelectorDropdown.value];
            var topic = StateManager.Instance.Topics[user.Id];

            var chatMessage = new ChatMessageContent {Body = _inputField.text};

            var msg = NTopicMessageSendMessage.Default(topic, JsonUtility.ToJson(chatMessage));
            NakamaManager.Instance.TopicSendMessage(msg);
        }
    }
}