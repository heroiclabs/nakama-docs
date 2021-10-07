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
using Nakama;

namespace Framework
{
    public class StateManager : Singleton<StateManager>
    {
        public INSelf SelfInfo { get; internal set; }

        public readonly List<INFriend> Friends = new List<INFriend>();
        public readonly List<INGroup> SearchedGroups = new List<INGroup>();
        public readonly List<INGroupSelf> JoinedGroups = new List<INGroupSelf>();

        // Map of User ID/Room Name to <TopicId, List of messages> for Chat Message
        public readonly Dictionary<string, INTopicId> Topics = new Dictionary<string, INTopicId>();

        public readonly Dictionary<INTopicId, Dictionary<string, INTopicMessage>> ChatMessages =
            new Dictionary<INTopicId, Dictionary<string, INTopicMessage>>();

        public readonly List<INNotification> Notifications = new List<INNotification>();
    }

    public class TopicMessageComparer : IComparer<INTopicMessage>
    {
        public int Compare(INTopicMessage x, INTopicMessage y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return y.CreatedAt.CompareTo(x.CreatedAt) != 0 ? y.CreatedAt.CompareTo(x.CreatedAt) : 0;
        }
    }
}