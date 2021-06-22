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
using Framework;
using Nakama;

namespace Showreel
{
    // This is only needed for Demo purposes.
    // You can ignore this class entirely.
    public static class FakeData
    {
        private static bool _initializedFakeData = false;

        private static readonly Action<INError> ErrorHandler = err =>
        {
            Logger.LogErrorFormat("Error: code '{0}' with '{1}'.", err.Code, err.Message);
        };

        private static readonly NClient Client = new NClient.Builder(NakamaManager.ServerKey)
            .Host(NakamaManager.HostIp)
            .Port(NakamaManager.Port)
            .SSL(NakamaManager.UseSsl)
            .Build();

        private static INSession _user1Session, _user2Session, _user3Session;

        // This initializes the server with fake data so that views are not empty when loaded.
        // This assumes that the current user is connected to the system.
        public static void Init()
        {
            if (_initializedFakeData)
            {
                return;
            }

            Client.Register(BuildAuthenticationMessage(), session =>
            {
                _user1Session = session;
                Client.Connect(_user1Session, c =>
                {
                    SetupUser1();
                    Client.Disconnect();

                    Client.Register(BuildAuthenticationMessage(), session2 =>
                    {
                        _user2Session = session2;
                        Client.Connect(_user2Session, c2 =>
                        {
                            SetupUser2();
                            Client.Disconnect();

                            Client.Register(BuildAuthenticationMessage(), session3 =>
                            {
                                _user3Session = session3;
                                Client.Connect(_user3Session, c3 =>
                                {
                                    SetupUser3();
                                    Client.Disconnect();

                                    SetupMainUser();
                                    _initializedFakeData = true;
                                });
                            }, ErrorHandler);
                        });
                    }, ErrorHandler);
                });
            }, ErrorHandler);
        }

        private static void SetupUser1()
        {
            Client.Send(NFriendAddMessage.ById(NakamaManager.Instance.Session.Id), b => { }, ErrorHandler);

            var builder = new NGroupCreateMessage.Builder("Thanksgiving");
            builder.Description("Turkey eating at my house!");
            builder.Lang("en");
            Client.Send(builder.Build(), b => { }, ErrorHandler);
        }

        private static void SetupUser2()
        {
            var builder = new NGroupCreateMessage.Builder("House Party");
            builder.Description("House warming party in a few days");
            builder.Lang("en");
            Client.Send(builder.Build(), b => { }, ErrorHandler); // don't care about the response.
        }

        private static void SetupUser3()
        {
            Client.Send(NFriendAddMessage.ById(NakamaManager.Instance.Session.Id), b => { }, ErrorHandler);
        }

        private static void SetupMainUser()
        {
            // let's add two users as friends
            NakamaManager.Instance.FriendAdd(NFriendAddMessage.ById(_user1Session.Id), false);
            NakamaManager.Instance.FriendAdd(NFriendAddMessage.ById(_user2Session.Id), false);

            var builder = new NGroupCreateMessage.Builder("School friends");
            builder.Description("Weekend getaway");
            builder.Lang("en");
            NakamaManager.Instance.GroupCreate(builder.Build());

            builder = new NGroupCreateMessage.Builder("Iranian friends");
            builder.Description("Loving groups of Farsi speaking friends!");
            builder.Lang("fa");
            NakamaManager.Instance.GroupCreate(builder.Build());
        }

        private static INAuthenticateMessage BuildAuthenticationMessage()
        {
            return NAuthenticateMessage.Custom(Guid.NewGuid().ToString());
        }
    }
}