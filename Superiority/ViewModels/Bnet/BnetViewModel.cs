using System;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Superiority.Model.Bnet;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using Superiority.Model.Util;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Windows.Controls;
using Superiority.Messages.Bnet;
using System.Threading;
using Superiority.Model.Intellisense;
using System.ComponentModel;
using System.Linq;

namespace Superiority.ViewModels.Bnet
{
    public class BnetSwitchAccountMessage
    {
        public AccountViewModel ViewModel
        {
            get;
            set;
        }
    }

    public class BnetViewModel : ViewModelBase
    {
        #region Battle.net related fields
        /// <summary>
        /// Connection we hold to Battle.net, on reconnect we reinstantiate this object.
        /// </summary>
        private BnetConnection bnetConnection;

        /// <summary>
        /// Battle.net account we store, only store it because of reinstantiation
        /// </summary>
        private BnetAccount bnetAccount;

        /// <summary>
        /// Our GUID for message passing purposes.
        /// </summary>
        private Guid token = Guid.NewGuid();
        #endregion

        #region Battle.net related properties
        public ObservableCollection<AutoCompletionEntry> AutoCompletionEntries
        {
            get;
            set;
        }

        private RelayCommand<KeyEventArgs> sendMessageCommand;
        public RelayCommand<KeyEventArgs> SendMessageCommand
        {
            get
            {
                if (sendMessageCommand == null)
                    sendMessageCommand = new RelayCommand<KeyEventArgs>((KeyEventArgs e) =>
                    {
                        var textBox = e.OriginalSource as TextBox;

                        if (e.Key == Key.Enter)
                        {
                            if (textBox.Text == string.Empty)
                                return;

                            SendMessage(textBox.Text);
                            textBox.Text = string.Empty;
                            e.Handled = true;

                            return;
                        }
                    });

                return sendMessageCommand;
            }
        }
        #endregion

        #region Battle.net data structures
        private string chattingStatus;
        public string ChattingStatus
        {
            get
            {
                return chattingStatus;
            }
            set
            {
                chattingStatus = value;
                VerifyPropertyName("ChattingStatus");
                RaisePropertyChanged("ChattingStatus");
            }
        }

        private ObservableCollection<BnetEvent> chatMessage;
        public ObservableCollection<BnetEvent> ChatMessages
        {
            get
            {
                return chatMessage;
            }
            set
            {
                chatMessage = value;
                VerifyPropertyName("ChatMessages");
                RaisePropertyChanged("ChatMessages");
            }
        }

        public ObservableCollection<BnetUser> ChannelUsers
        {
            get;
            set;
        }

        public ObservableCollection<BnetFriend> FriendsList
        {
            get;
            set;
        }

        public ObservableCollection<BnetUser> ChannelList
        {
            get;
            set;
        }

        private MemoryPool<BnetUser> userPool = new MemoryPool<BnetUser>();
        #endregion

        /// <summary>
        /// BnetViewModel's constructor
        /// </summary>
        /// <param name="account">BnetAccount passed by AccountViewModel</param>
        public BnetViewModel(BnetAccount account)
        {
            ChatMessages = new ObservableCollection<BnetEvent>();
            ChannelList = new ObservableCollection<BnetUser>();
            ChannelUsers = new ObservableCollection<BnetUser>();
            FriendsList = new ObservableCollection<BnetFriend>();
            AutoCompletionEntries = new ObservableCollection<AutoCompletionEntry>();
            AutoCompletionEntries.Add(new AutoCompletionEntry() { EntryType = AutoCompletionEntryType.Command, Title = "Reply Command", Description = "Replies to the last person who whispered you.", LiteralText = "/r" });
            AutoCompletionEntries.Add(new AutoCompletionEntry() { EntryType = AutoCompletionEntryType.Command, Title = "Reconnect Command", Description = "Reconnects the current account to Battle.net.", LiteralText = "/reconnect" });
            AutoCompletionEntries.Add(new AutoCompletionEntry() { EntryType = AutoCompletionEntryType.Command, Title = "Version Command", Description = "Emotes the current bot version.", LiteralText = "/version" });

            ChattingStatus = "disconnected";
            // Initialize Battle.net stuff
            bnetAccount = account;

            // Start listening for messages from BnetConnection
            Messenger.Default.Register<BnetEvent[]>(this, token, BnetEventReceived);
            Messenger.Default.Register<BnetFriend>(this, token, BnetFriendReceived);

            Connect();
        }

        /// <summary>
        /// Used internally and by AccountViewModel to connect/reconnect to Battle.net
        /// </summary>
        public void Connect()
        {
            bool ret = false;
            if (bnetAccount["Username"] != null)
            {
                var chatMsg = new BnetEvent();
                chatMsg.EventId = 0x13;
                chatMsg.Message = string.Format("You have an error in your settings: {0}", bnetAccount["Username"]);
                chatMsg.TimeStamp = DateTime.Now;
                ChatMessages.Add(chatMsg);

                Messenger.Default.Send<ScrollDownMessage>(new ScrollDownMessage() { Source = this });
                ret = true;
            }

            if (bnetAccount["Password"] != null)
            {
                var chatMsg = new BnetEvent();
                chatMsg.EventId = 0x13;
                chatMsg.Message = string.Format("You have an error in your settings: {0}", bnetAccount["Password"]);
                chatMsg.TimeStamp = DateTime.Now;
                ChatMessages.Add(chatMsg);

                Messenger.Default.Send<ScrollDownMessage>(new ScrollDownMessage() { Source = this });
                ret = true;
            }

            if (bnetAccount["CdKey"] != null)
            {
                var chatMsg = new BnetEvent();
                chatMsg.EventId = 0x13;
                chatMsg.Message = string.Format("You have an error in your settings: {0}", bnetAccount["CdKey"]);
                chatMsg.TimeStamp = DateTime.Now;
                ChatMessages.Add(chatMsg);

                Messenger.Default.Send<ScrollDownMessage>(new ScrollDownMessage() { Source = this });
                ret = true;
            }

            if (bnetAccount["HomeChannel"] != null)
            {
                var chatMsg = new BnetEvent();
                chatMsg.EventId = 0x13;
                chatMsg.Message = string.Format("You have an error in your settings: {0}", bnetAccount["HomeChannel"]);
                chatMsg.TimeStamp = DateTime.Now;
                ChatMessages.Add(chatMsg);

                Messenger.Default.Send<ScrollDownMessage>(new ScrollDownMessage() { Source = this });
                ret = true;
            }

            if (ret)
            {
                var chatMsg = new BnetEvent();
                chatMsg.EventId = 0x13;
                chatMsg.Message = "Settings validation failed! Fix the errors listed above then type /reconnect to try again.";
                chatMsg.TimeStamp = DateTime.Now;
                ChatMessages.Add(chatMsg);

                Messenger.Default.Send<ScrollDownMessage>(new ScrollDownMessage() { Source = this });

                return;
            }

            if (bnetConnection != null)
                bnetConnection.Dispose();

            ChattingStatus = "connecting";

            ThreadPool.QueueUserWorkItem((object state) => 
            {
                bnetConnection = new BnetConnection(bnetAccount, token);
                bnetConnection.Connect();
            });
        }

        public void SendMessage(string msg)
        {
            if (!msg.StartsWith("/"))
            {
                bnetConnection.SendMessage(msg);

                var chatMsg = new BnetEvent();
                chatMsg.EventId = 0x18;
                chatMsg.Message = msg;
                chatMsg.Username = bnetAccount.Username;
                chatMsg.TimeStamp = DateTime.Now;
                ChatMessages.Add(chatMsg);

                Messenger.Default.Send<ScrollDownMessage>(new ScrollDownMessage() { Source = this });
            }
            else
            {
                if (msg.ToLower() == "/reconnect")
                {
                    ChannelUsers.Clear();
                    ChannelList.Clear();
                    FriendsList.Clear();

                    ChattingStatus = "disconnected";
                    Connect();

                }
                else if (msg.ToLower() == "/version")
                {
                    bnetConnection.SendMessage("/me is using Superiority.NET Beta5 (www.BNET.cc).");
                }
                else
                {
                    bnetConnection.SendMessage(msg);
                }
            }
        }

        public void BnetFriendReceived(BnetFriend friend)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                FriendsList.Add(friend);
            });
        }

        /// <summary>
        /// BnetConnection dispatches a message to us on every Bnet Event.
        /// </summary>
        /// <param name="msg">BnetEvent sent to us by BnetConnection</param>
        public void BnetEventReceived(BnetEvent[] messages)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                foreach (var msg in messages)
                {
                    if (ChatMessages.Count > 120)
                    {
                        var cache = new ObservableCollection<BnetEvent>();
                        for (int i = 100; i < ChatMessages.Count; i++)
                            cache.Add(ChatMessages[i]);

                        ChatMessages = new ObservableCollection<BnetEvent>(cache);
                    }

                    if (msg.EventId != 0x01 || msg.EventId != 0x09 || msg.EventId != 0x21 || msg.EventId != 0x22)
                    {
                        ChatMessages.Add(msg);
                        Messenger.Default.Send<ScrollDownMessage>(new ScrollDownMessage() { Source = this });
                    }

                    if (msg.EventId == 0x21)
                    {
                        ChannelUsers.Clear();
                        ChannelList.Clear();
                        FriendsList.Clear();

                        ChattingStatus = "disconnected";
                        Connect();
                    }

                    if (msg.EventId == 0x22) // EID_CHANNELUSER
                    {
                        // We use a List<> because ChannelList is only requested once per connection .. 
                        ChannelList.Add(new BnetUser() { Username = msg.Username });
                    }

                    if (msg.EventId == 0x07)
                    {
                        ChattingStatus = string.Format("chatting in {0}", msg.Message.ToLower());

                        ChannelUsers.Clear();
                    }

                    if (msg.EventId == 0x03)
                    {
                        foreach (var user in ChannelUsers)
                            if (user.Username == msg.Username)
                            {
                                ChannelUsers.Remove(user);
                                break;
                            }
                    }

                    if (msg.EventId == 0x01 || msg.EventId == 0x02)
                    {
                        BnetProduct p;

                        if (msg.Message.Length == 0)
                            return;

                        switch (msg.Message.Substring(0, 4))
                        {
                            case "RATS":
                                p = BnetProduct.Starcraft;
                                break;
                            case "PXES":
                                p = BnetProduct.BroodWar;
                                break;
                            case "NB2W":
                                p = BnetProduct.Warcraft2;
                                break;
                            case "VD2D":
                                p = BnetProduct.Diablo2;
                                break;
                            case "PX2D":
                                p = BnetProduct.LordOfDest;
                                break;
                            case "3RAW":
                                p = BnetProduct.Warcraft3;
                                break;
                            case "PX3W":
                                p = BnetProduct.FrozenThrone;
                                break;
                            default:
                                p = BnetProduct.BroodWar;
                                break;
                        }

                        BnetUser usr = new BnetUser();
                        usr.Username = msg.Username;
                        usr.DisplayName = msg.Username.Split('@')[0];
                        usr.Product = p;
                        usr.Flags = msg.Flags;

                        ChannelUsers.Add(usr);
                    }

                    if (msg.EventId == 0x09)
                        // Process flag update
                        foreach (var user in ChannelUsers)
                            if (user.Username == msg.Username)
                            {
                                user.Flags = msg.Flags;
                                break;
                            }
                }
            });
        }
    }
}
