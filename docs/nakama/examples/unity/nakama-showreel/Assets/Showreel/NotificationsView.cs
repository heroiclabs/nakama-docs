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

using Framework;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace Showreel
{
    public class NotificationsView : MonoBehaviour
    {
        private Text _notificationsLabel;

        // this is used to keep track of rendered messages for that user  
        private int _renderedNotifications = 0;

        private void Start()
        {
            _notificationsLabel = GameObject.Find("NotificationsLabel").GetComponent<Text>();

            NotificationsList();
        }

        private void Update()
        {
            // if the number of rendered notifications is the same as notifications we have, we don't need to re-render.
            if (_renderedNotifications == StateManager.Instance.Notifications.Count)
            {
                // nothing new to render
                return;
            }

            var allMessages = "";
            var notifications = StateManager.Instance.Notifications;
            foreach (var notification in notifications)
            {
                allMessages += string.Format(@"
Notification sent by {0}:
Subject: {1}
Content: {2}

				", notification.SenderId ?? "System", notification.Subject, notification.Content);
            }

            _notificationsLabel.text = allMessages;
        }

        private void NotificationsList()
        {
            var msg = new NNotificationsListMessage.Builder(10);
            NakamaManager.Instance.NotificationsList(msg);
        }
    }
}