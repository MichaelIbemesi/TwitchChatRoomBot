using System;
using System.Collections.Generic;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace ChatRoomBot
{
    public class TwitchBot
    {
        private ConnectionCredentials Credentials;
        private TwitchClient Client;
        private Dictionary<string, string> CommandResponses;

        public TwitchBot()
        {
            Credentials = new ConnectionCredentials(TwitchCredentials.ChannelName, TwitchCredentials.Token);
            Client = new TwitchClient();
            CommandResponses = new Dictionary<string, string>();
        }

        public void Connect(bool isLogging)
        {
            Client.Initialize(Credentials, TwitchCredentials.ChannelName);

            if (isLogging)
            {
                Client.OnLog += Client_OnLog;
                Client.OnMessageReceived += Client_OnMessageRecieved;
                Client.OnChatCommandReceived += Client_OnChatCommandRecieved;
                Client.OnError += Client_OnError;
                Client.OnNewSubscriber += Client_OnNewSubscriber;
            }

            Client.Connect();
            Client.OnConnected += Client_OnConnected;
        }

        public void Disconnect()
        {
            Client.Disconnect();
            Client.OnDisconnected += Client_OnDisconnected;
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine("[TwitchBot]: Connection Successful");
        }

        private void Client_OnChatCommandRecieved(object sender, OnChatCommandReceivedArgs e)
        {
            if (e.Command.ChatMessage.DisplayName == TwitchCredentials.ChannelName)
            {
                switch (e.Command.CommandText.ToLower())
                {
                    case "clear":
                        Client.ClearChat(TwitchCredentials.ChannelName);
                        break;
                    case "ban":
                        Client.BanUser(TwitchCredentials.ChannelName, e.Command.CommandText.Replace("ban","").ToLower().Trim());
                        break;
                    case "unban":
                        Client.UnbanUser(TwitchCredentials.ChannelName, e.Command.CommandText.Replace("unban", "").ToLower().Trim());
                        break;
                    case "timeout":
                        Client.TimeoutUser(TwitchCredentials.ChannelName, e.Command.CommandText.Replace("timeout", "").ToLower().Trim(), TimeSpan.FromMinutes(15));
                        break;

                    case "subon":
                        Client.SubscribersOnlyOn(TwitchCredentials.ChannelName);
                        break;
                    case "suboff":
                        Client.SubscribersOnlyOff(TwitchCredentials.ChannelName);
                        break;

                    case "slowon":
                        Client.SlowModeOn(TwitchCredentials.ChannelName, TimeSpan.FromMinutes(10));
                        break;
                    case "slowoff":
                        Client.SlowModeOff(TwitchCredentials.ChannelName);
                        break;

                    case "followon":
                        Client.FollowersOnlyOn(TwitchCredentials.ChannelName, TimeSpan.FromMinutes(10));
                        break;
                    case "followoff":
                        Client.FollowersOnlyOff(TwitchCredentials.ChannelName);
                        break;

                    case "emoteon":
                        Client.EmoteOnlyOn(TwitchCredentials.ChannelName);
                        break;
                    case "emoteoff":
                        Client.EmoteOnlyOff(TwitchCredentials.ChannelName);
                        break;

                    case "addcommand":
                        string[] commandKeyValue = e.Command.ChatMessage.Message.Replace("!addcommand", "").Split(":");
                        try
                        {
                            if (!commandKeyValue[0].Trim().StartsWith("!"))
                            {
                                Client.SendMessage(TwitchCredentials.ChannelName, "Invalid command.");
                            }
                            else if (CommandResponses.ContainsKey(commandKeyValue[0].Trim()))
                            {
                                Client.SendMessage(TwitchCredentials.ChannelName, "Command already exists.");
                            }
                            else
                            {
                                CommandResponses.Add(commandKeyValue[0].Trim(), commandKeyValue[1].Trim());
                            }
                            
                        }
                        catch (Exception)
                        {

                            Client.SendMessage(TwitchCredentials.ChannelName, "Invalid command.");
                        }
                        break;
                    case "dropcommand":
                        string commandToRemove = e.Command.ChatMessage.Message.Replace("!dropcommand", "").Trim();
                        if (CommandResponses.ContainsKey(commandToRemove))
                        {
                            CommandResponses.Remove(commandToRemove);
                        }
                        break;

                }
            }

            switch (e.Command.CommandText.ToLower())
            {
                case "help":
                    Client.SendMessage(TwitchCredentials.ChannelName, "No help can currently be provided.");
                    break;
                case "subage":
                    Client.SendMessage(TwitchCredentials.ChannelName, e.Command.ChatMessage.SubscribedMonthCount.ToString());
                    break;
            }

            if (CommandResponses.ContainsKey("!" + e.Command.CommandText.ToLower()))
            {
                Client.SendMessage(TwitchCredentials.ChannelName, CommandResponses.GetValueOrDefault("!" + e.Command.CommandText.ToLower()));
            }
        }

        private void Client_OnMessageRecieved(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.Contains("badword"))
            {
                Client.TimeoutUser(e.ChatMessage.Channel, e.ChatMessage.Username, TimeSpan.FromMinutes(15), $"Please Behave yourself in the chat {e.ChatMessage.Username}.");
            }

            Console.WriteLine($"[{e.ChatMessage.DisplayName}]: {e.ChatMessage.Message}");
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
            {
                Client.SendMessage(e.Channel, $"{e.Subscriber.DisplayName} has subscribed for the first time with Twitch Prime!");
            }
            else if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Tier2)
            {
                Client.SendMessage(e.Channel, $"{e.Subscriber.DisplayName} has subscribed for the first time! (Tier two).");
            }
            else if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Tier3)
            {
                Client.SendMessage(e.Channel, $"{e.Subscriber.DisplayName} has subscribed for the first time! (Tier three).");
            }
            else
            {
                Client.SendMessage(e.Channel, $"{e.Subscriber.DisplayName} has subscribed for the first time!");
            }
        }

        private void Client_OnError(object sender, OnErrorEventArgs e)
        {
            Console.WriteLine("Something has gone wrong...");
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void Client_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            Console.WriteLine("[TwitchBot]: Disconnection Successful");
        }
    }
}