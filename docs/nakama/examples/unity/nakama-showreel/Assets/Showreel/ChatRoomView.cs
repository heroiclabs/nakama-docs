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
    public class ChatRoomView : MonoBehaviour
    {
        private Text _chatMessageLabel;
        private Button _sendMessageButton;
        private InputField _inputField;
        private Text _topicLabel;

        private const string RoomName = "showreel-room";

        // this is used to keep track of rendered messages for that user  
        private int _renderedMessages = 0;

        private void Start()
        {
            _chatMessageLabel = GameObject.Find("ChatMessageLabel").GetComponent<Text>();
            _sendMessageButton = GameObject.Find("SendMessageButton").GetComponent<Button>();
            _inputField = GameObject.Find("InputField").GetComponent<InputField>();
            _topicLabel = GameObject.Find("TopicLabel").GetComponent<Text>();
            _topicLabel.text = RoomName;

            _sendMessageButton.interactable = false;

            JoinRoomTopic();
            FetchHistoricMessages();
        }

        private void Update()
        {
            if (!StateManager.Instance.Topics.ContainsKey(RoomName))
            {
                // we've not subscribed to the topic yet - let's ignore rendering anything.
                return;
            }

            // if the number of rendered messages is the same as messages we have, we don't need to re-render.
            var topic = StateManager.Instance.Topics[RoomName];
            var roomMessages = StateManager.Instance.ChatMessages[topic];
            if (_renderedMessages == roomMessages.Count)
            {
                // nothing new to render
                return;
            }

            var chatMessages = new List<INTopicMessage>();
            chatMessages.AddRange(roomMessages.Values);
            chatMessages.Sort(new TopicMessageComparer());

            var allMessages = "";
            foreach (var msg in chatMessages)
            {
                var chatMessage = JsonUtility.FromJson<ChatMessageContent>(msg.Data);
                allMessages += string.Format(@"
{0} said: {1}
				", msg.Handle, chatMessage.Body);
            }

            _chatMessageLabel.text = allMessages;
        }

        private void JoinRoomTopic()
        {
            var msg = new NTopicJoinMessage.Builder().TopicRoom(RoomName).Build();
            NakamaManager.Instance.TopicJoin(RoomName, msg);
            _sendMessageButton.interactable = true;
        }

        private void FetchHistoricMessages()
        {
            var topic = StateManager.Instance.Topics[RoomName];
            var builder = new NTopicMessagesListMessage.Builder();
            builder.TopicRoom(RoomName);
            NakamaManager.Instance.TopicMessageList(topic, builder);
        }

        public void SendRoomMessage()
        {
            var topic = StateManager.Instance.Topics[RoomName];

            var chatMessage = new ChatMessageContent {Body = _inputField.text};

            var msg = NTopicMessageSendMessage.Default(topic, JsonUtility.ToJson(chatMessage));
            NakamaManager.Instance.TopicSendMessage(msg);
        }
    }
}