using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Globalization;
using System.Threading;


using Superiority.Model.Util;
using Superiority.Model.Networking;
using Superiority.ViewModels;
using Superiority.Model.Bnls;
using Superiority.Messages;

using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using System.Windows.Threading;
    
namespace Superiority.Model.Bnet
{
    public class BnetConnection : Connection
    {
        #region Bnetconnection Fields
        private BnetAccount botAccount;
        #region Request Tokens
            private LogonChallengeRequestToken logonChallengeRequestToken;
            private CreateAccountRequestToken createAccountRequestToken;
            private LogonProofRequestToken logonProofRequestToken;
            #endregion
        #endregion

        public BnetConnection(BnetAccount account, Guid token)
        {
            botAccount = account;

            var server = "useast.battle.net";

            if (botAccount.Server == BnetServer.UsWest)
                server = "uswest.battle.net";
            else if (botAccount.Server == BnetServer.Europe)
                server = "europe.battle.net";
            else if (botAccount.Server == BnetServer.Asia)
                server = "asia.battle.net";

            RemoteEndPoint = new DnsEndPoint(server, 6112);
            connectionToken = token;
            eventTimer = new Timer(eventTimer_Tick);
            eventTimer.Change(300, 0);
        }

        void eventTimer_Tick(object state)
        {
            lock (eventSync)
            {
                if (eventQueue.Count > 0)
                {
                    var toSend = eventQueue.ToArray();
                    eventQueue.Clear();
                    Messenger.Default.Send<BnetEvent[]>(toSend, connectionToken);
                }
            }

            eventTimer.Change(300, 0);
        }

        private void SendPacket(byte pktId, BnetPacket packet)
        {
            System.Diagnostics.Debug.WriteLine("BNET C->S {0:X} {1}", pktId, botAccount.AccountName);

            byte[] packetData = packet.GetBuffer(pktId);

            Send(packetData);
            packet.Dispose();
        }

        public void SendMessage(string message)
        {
            BnetPacket sendMessage = new BnetPacket();
            sendMessage.InsertString(message);
            SendPacket(0x0E, sendMessage); // SID_CHATCOMMAND
        }

        public void RequestProfile(string username, bool isWc3)
        {
            if (isWc3)
            {
                BnetPacket requestW3Profile = new BnetPacket();
                requestW3Profile.InsertInt32(0x00);
                requestW3Profile.InsertString(username);
                cachedW3RequestName = username;
                SendPacket(0x35, requestW3Profile);
            }
            else
            {
                BnetPacket requestProfile = new BnetPacket();
                requestProfile.InsertInt32(0x01);
                requestProfile.InsertInt32(0x02);
                requestProfile.InsertInt32(0x00);
                requestProfile.InsertString(username);
                requestProfile.InsertString("profile\\location");
                requestProfile.InsertString("profile\\description");

                SendPacket(0x26, requestProfile);
            }
        }

        protected override void Connected()
        {
            base.Connected();

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    BnetEvent b = new BnetEvent();
                    b.EventId = 0x19;
                    b.Username = string.Empty;
                    b.Message = "Connected to Battle.net.";

                    Messenger.Default.Send<BnetEvent>(b, connectionToken);

                    // Build 0x50
                    BnetPacket bntPkt = new BnetPacket();
                    bntPkt.InsertInt32(0x00);
                    bntPkt.InsertInt32(0x49583836);

                    if (botAccount.Product == BnetProduct.Starcraft)
                        bntPkt.InsertBytes(System.Text.Encoding.UTF8.GetBytes("RATS"));
                    else if (botAccount.Product == BnetProduct.BroodWar)
                        bntPkt.InsertBytes(System.Text.Encoding.UTF8.GetBytes("PXES"));
                    else if (botAccount.Product == BnetProduct.Warcraft2)
                        bntPkt.InsertBytes(System.Text.Encoding.UTF8.GetBytes("NB2W"));
                    else if (botAccount.Product == BnetProduct.Diablo2)
                        bntPkt.InsertBytes(System.Text.Encoding.UTF8.GetBytes("VD2D"));
                    else if (botAccount.Product == BnetProduct.Warcraft3)
                        bntPkt.InsertBytes(System.Text.Encoding.UTF8.GetBytes("3RAW"));

                    if (ViewModelLocator.MainWindowViewModelStatic.AccountsModel.VersionBytes.ContainsKey(botAccount.Product))
                    {
                        bntPkt.InsertInt32(ViewModelLocator.MainWindowViewModelStatic.AccountsModel.VersionBytes[botAccount.Product]);
                    }
                    else
                    {
                        // Insert 0x00 just so that we don't have an invalid packet structure .. but we know we'll need to reconnect.
                        if (botAccount.Product == BnetProduct.Starcraft || botAccount.Product == BnetProduct.BroodWar)
                            bntPkt.InsertInt32(0xD3);
                        else if (botAccount.Product == BnetProduct.Diablo2 || botAccount.Product == BnetProduct.LordOfDest)
                            bntPkt.InsertInt32(0x0D);
                        else if (botAccount.Product == BnetProduct.Warcraft2)
                            bntPkt.InsertInt32(0x4F);
                        else if (botAccount.Product == BnetProduct.Warcraft3 || botAccount.Product == BnetProduct.FrozenThrone)
                            bntPkt.InsertInt32(0x13);
                    }

                    bntPkt.InsertInt32(0x00);
                    bntPkt.InsertInt32(0x00);
                    bntPkt.InsertInt32(0x00);
                    bntPkt.InsertInt32(0x00);
                    bntPkt.InsertInt32(0x00);
                    bntPkt.InsertString("USA");
                    bntPkt.InsertString("United States");

                    using (DataBuffer sendProtocolBuffer = new DataBuffer())
                    {
                        sendProtocolBuffer.InsertByte(0x01); // Protocol ID
                        sendProtocolBuffer.InsertBytes(bntPkt.GetBuffer(0x50));

                        Send(sendProtocolBuffer.GetBuffer());
                        bntPkt.Dispose();
                    }

                    Receive(4, null);
                });
        }

        protected override void ReceivedData(SocketAsyncEventArgs e)
        {
            byte[] receivedData = e.Buffer;
            int pktLen;

            if (e.UserToken == null)
            {
                e.UserToken = new DataBuffer();
                pktLen = BitConverter.ToInt16(receivedData, 2);
            }
            else
            {
                DataBuffer tmp = e.UserToken as DataBuffer;
                pktLen = BitConverter.ToInt16(tmp.GetBuffer(), 2);
            }

            DataBuffer recvBuffer = e.UserToken as DataBuffer;
            recvBuffer.InsertBytes(e.Buffer);

            if (recvBuffer.Length < pktLen)
            {
                int diffLen = pktLen - recvBuffer.Length;
                ReceiveNoLock(diffLen, e.UserToken);
            }
            else
            {
                ParseBnet(recvBuffer.GetBuffer());
                recvBuffer.Dispose();
                receivingResetEvent.Set();

                Receive(4, null);
            }
        }

        #region Bnls -> Bnet Messaging Handler
        public void ReceivedBnlsMessage(IBnlsRequestToken token)
        {
            if (token is VersionCheckRequestToken)
            {
                VersionCheckRequestToken vcrt = token as VersionCheckRequestToken;

                if (!ViewModelLocator.MainWindowViewModelStatic.AccountsModel.VersionBytes.ContainsKey(botAccount.Product))
                {
                    // Insert the Product, signal a reconnect
                    ViewModelLocator.MainWindowViewModelStatic.AccountsModel.VersionBytes.Add(botAccount.Product, (byte)vcrt.VersionByte);

                    lock (eventSync)
                    {
                        BnetEvent beEvt = new BnetEvent();
                        beEvt.EventId = 0x21;

                        eventQueue.Enqueue(beEvt);
                    }
                }
                else
                {
                    // Store the current version byte, if we get a "Invalid Version" we can try to reconnect
                    if ((int)ViewModelLocator.MainWindowViewModelStatic.AccountsModel.VersionBytes[botAccount.Product] < vcrt.VersionByte)
                    {
                        ViewModelLocator.MainWindowViewModelStatic.AccountsModel.VersionBytes[botAccount.Product] = (byte)vcrt.VersionByte;
                        // VersionByte sent from BNLS is greater than what we have stored   
                        BnetEvent beEvt = new BnetEvent();
                        beEvt.EventId = 0x21;

                        Messenger.Default.Send<BnetEvent>(beEvt, connectionToken);
                    }
                    else
                    {
                        BnetPacket bntPkt = new BnetPacket();

                        bntPkt.InsertInt32(clientToken);
                        bntPkt.InsertInt32(vcrt.Version);
                        bntPkt.InsertInt32(vcrt.Checksum);
                        bntPkt.InsertInt32(1);
                        bntPkt.InsertInt32(0);

                        bntPkt.InsertInt32(botAccount.CdKey.Length);
                        MBNCSUtil.CdKey key = null;

                        try
                        {
                            key = MBNCSUtil.CdKey.CreateDecoder(botAccount.CdKey);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // TODO: Post error.
                            return;
                        }

                        if (!key.IsValid)
                            return;

                        bntPkt.InsertInt32(key.Product);
                        bntPkt.InsertInt32(key.Value1);
                        bntPkt.InsertInt32(0);
                        bntPkt.InsertBytes(key.GetHash(clientToken, serverToken));

                        bntPkt.InsertBytes(vcrt.Statstring);
                        bntPkt.InsertByte(0);
                        bntPkt.InsertString(botAccount.Username);

                        SendPacket(0x51, bntPkt);
                    }
                }
            }
            else if (token is LogonChallengeRequestToken)
            {
                LogonChallengeRequestToken lcrt = token as LogonChallengeRequestToken;

                BnetPacket accountLogon = new BnetPacket();

                foreach (var logonData in lcrt.ChallengeData)
                {
                    accountLogon.InsertInt32(logonData);
                }

                accountLogon.InsertString(botAccount.Username);
                SendPacket(0x53, accountLogon);
            }
            else if (token is CreateAccountRequestToken)
            {
                CreateAccountRequestToken cart = token as CreateAccountRequestToken;

                BnetPacket createAccount = new BnetPacket();

                foreach (var createAccountData in cart.AccountCreateData)
                    createAccount.InsertInt32(createAccountData);

                createAccount.InsertString(botAccount.Username);
                SendPacket(0x52, createAccount);
            }
            else if (token is LogonProofRequestToken)
            {
                LogonProofRequestToken lprt = token as LogonProofRequestToken;

                BnetPacket logonProof = new BnetPacket();

                foreach (var logonProofData in lprt.LogonProofData)
                    logonProof.InsertInt32(logonProofData);

                SendPacket(0x54, logonProof);
            }
        }
        #endregion

        #region Packet Parsing
        private string cachedW3RequestName;
        private War3ProfileRequest cachedW3ProfileRequest;
        private int serverToken;
        private int clientToken;
        private Queue<BnetEvent> eventQueue = new Queue<BnetEvent>();
        private readonly object eventSync = new object();
        private Timer eventTimer;

        private void ParseBnet(byte[] incomingData)
        {
            DataReader dr = new DataReader(incomingData);
            BnetEvent cachedBeObj = new BnetEvent();

            dr.ReadByte();
            byte pktId = dr.ReadByte();
            dr.ReadInt16();

            switch (pktId)
            {
                case 0x00:
                    BnetPacket nullPkt = new BnetPacket();
                    SendPacket(0x00, nullPkt);

                    break;
                case 0x25:
                    BnetPacket pingPkt = new BnetPacket();
                    pingPkt.InsertInt32(dr.ReadInt32());

                    SendPacket(0x25, pingPkt);

                    break;
                case 0x26:
                    dr.ReadInt32(); // Account number
                    dr.ReadInt32(); // Key number
                    dr.ReadInt32(); // Request Id;

                    string location = dr.ReadString();
                    string description = dr.ReadString();

                    LegacyProfileRequest lpr = new LegacyProfileRequest();
                    lpr.Location = location;
                    lpr.Description = description;

                    Messenger.Default.Send<ProfileRequest>(lpr, connectionToken);

                    break;
                case 0x35:
                    dr.ReadInt32();
                    byte success = dr.ReadByte();

                    string w3Desc = dr.ReadString();
                    string w3Loc = dr.ReadString();

                    int clanTag = dr.ReadInt32();

                    if (cachedW3ProfileRequest == null)
                        cachedW3ProfileRequest = new War3ProfileRequest();

                    cachedW3ProfileRequest.Location = w3Loc;
                    cachedW3ProfileRequest.Description = w3Desc;

                    if (clanTag != 0)
                    {
                        // Request SID_CLANMEMBERINFORMATION
                        BnetPacket clanMemInfoPkt = new BnetPacket();
                        clanMemInfoPkt.InsertInt32(0);
                        clanMemInfoPkt.InsertInt32(clanTag);
                        clanMemInfoPkt.InsertString(cachedW3RequestName);

                //        SendPacket(0x82, clanMemInfoPkt);   

                        BnetPacket clanStatPkt = new BnetPacket();
                        clanStatPkt.InsertByte(0x08);
                        clanStatPkt.InsertInt32(0);
                        clanStatPkt.InsertInt32(clanTag);
                        clanStatPkt.InsertBytes(System.Text.Encoding.UTF8.GetBytes("3RAW"));

                        //SendPacket(0x44, clanStatPkt);
                    }

                    break;
         
                case 0x44:
                    break;
                case 0x82:
                    dr.ReadInt32();
                    dr.ReadByte();
                    System.Diagnostics.Debug.WriteLine("Clan Name {0}", dr.ReadString());
                    break;
                case 0x3D:
                    // Account creation
                    int legacyCreationResult = dr.ReadInt32();
                    switch (legacyCreationResult)
                    {
                        case 0x00:
                            BnetPacket logonResponse2 = new BnetPacket();
                             
                            logonResponse2.InsertInt32(clientToken);
                            logonResponse2.InsertInt32(serverToken);
                            logonResponse2.InsertBytes(MBNCSUtil.OldAuth.DoubleHashPassword(botAccount.Password.ToLower(), clientToken, serverToken));
                            logonResponse2.InsertString(botAccount.Username);

                            SendPacket(0x3A, logonResponse2); // SID_LOGONRESPONSE2

                            break;
                    }

                    break;
                case 0x50:
                    dr.ReadInt32(); // Logon Type
                    serverToken = dr.ReadInt32();
                    clientToken = Environment.TickCount;
                    dr.ReadInt32(); // UDPValue
                    long fileTime = dr.ReadInt64();
                    string versionName = dr.ReadString();
                    byte[] checksumForm = dr.ReadByteArrayNt();

                    var versionCheckRequestToken = new VersionCheckRequestToken((int)botAccount.Product, fileTime, versionName, checksumForm, serverToken, botAccount.CdKey);
                    Messenger.Default.Register<IBnlsRequestToken>(this, versionCheckRequestToken, true, ReceivedBnlsMessage);

                   BnlsService.Instance.BnlsVersionCheckRequest(versionCheckRequestToken);
               
                    break;
                case 0x51:
                    int result = dr.ReadInt32();
                    
                    switch(result)
                    {
                        case 0x000:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x19;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Version check passed.";

                                eventQueue.Enqueue(cachedBeObj);
                            }

                            if (botAccount.Product != BnetProduct.Warcraft3)
                            {
                                BnetPacket logonResponse2 = new BnetPacket();
                             
                                logonResponse2.InsertInt32(clientToken);
                                logonResponse2.InsertInt32(serverToken);
                                logonResponse2.InsertBytes(MBNCSUtil.OldAuth.DoubleHashPassword(botAccount.Password.ToLower(), clientToken, serverToken));
                                logonResponse2.InsertString(botAccount.Username);

                                SendPacket(0x3A, logonResponse2); // SID_LOGONRESPONSE2
                            }
                            else
                            {
                                logonChallengeRequestToken = new LogonChallengeRequestToken(botAccount.Username, botAccount.Password.ToLower(), botAccount.Product);
                                Messenger.Default.Register<IBnlsRequestToken>(this, logonChallengeRequestToken, true, ReceivedBnlsMessage);
                                BnlsService.Instance.BnlsLogonChallengeRequest(logonChallengeRequestToken);
                            }

                            break;
                        case 0x100:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Old game version!";
                                eventQueue.Enqueue(cachedBeObj);
                            }
                            
                            // Disconnect();
                            break;
                        case 0x101:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Invalid version!";

                                eventQueue.Enqueue(cachedBeObj);
                            }

                           // Disconnect();
                            break;
                        case 0x102:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Game version must be downgraded!";
                                eventQueue.Enqueue(cachedBeObj);
                            }

                           // Disconnect();

                            break;
                        case 0x200:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Invalid CD-Key!";

                                eventQueue.Enqueue(cachedBeObj);
                            }
                           // Disconnect();

                            break;
                        case 0x201:
                            lock (eventSync)
                            {
                                string user = dr.ReadString();
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = string.Format("CD-Key in use by: {0}", user);

                                eventQueue.Enqueue(cachedBeObj);
                            }
                           // Disconnect();

                            break;
                        case 0x202:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Banned CD-Key!";

                                eventQueue.Enqueue(cachedBeObj);
                            }
                           // Disconnect();

                            break;
                        case 0x203:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Wrong product!";

                                eventQueue.Enqueue(cachedBeObj);
                            }

                           // Disconnect();

                            break;
                        default:
                            lock (eventSync)
                            {
                                System.Diagnostics.Debug.WriteLine("UKWN Result {0:X}", result);
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Invalid version!";

                                eventQueue.Enqueue(cachedBeObj);
                            }
                           // Disconnect();

                            break;
                    }
                    

                    break;

                case 0x52:
                    int accountCreateResult = dr.ReadInt32();

                    switch (accountCreateResult)
                    {
                        case 0x00:
                            BnlsService.Instance.BnlsLogonChallengeRequest(logonChallengeRequestToken);

                            break;
                    }

                    break;

                case 0x53:
                    int accountLogonResult = dr.ReadInt32();

                    switch (accountLogonResult)
                    {
                        case 0x00:
                            int[] salt = dr.ReadInt32Array(8);
                            int[] serverKey = dr.ReadInt32Array(8);

                            logonProofRequestToken = new LogonProofRequestToken(salt, serverKey);
                            
                            Messenger.Default.Register<IBnlsRequestToken>(this, logonProofRequestToken, true, ReceivedBnlsMessage);
                            BnlsService.Instance.BnlsLogonProofRequest(logonProofRequestToken);

                            break;
                        case 0x01:
                            // Account doesn't exist. 
                            createAccountRequestToken = new CreateAccountRequestToken(botAccount.Username, botAccount.Password.ToLower());

                            Messenger.Default.Register<IBnlsRequestToken>(this, createAccountRequestToken, true, ReceivedBnlsMessage);
                            BnlsService.Instance.BnlsCreateAccountRequest(createAccountRequestToken);

                            break;
                        case 0x05:
                            // TODO:
                            break;
                    }

                    break;

                case 0x54:
                    int logonProofResult = dr.ReadInt32();

                    switch (logonProofResult)
                    {
                        case 0x00:
                            BnetPacket enterChat = new BnetPacket();
                            enterChat.InsertString(botAccount.Username);
                            enterChat.InsertString("");

                            SendPacket(0x0A, enterChat);

                            BnetPacket channelList = new BnetPacket();
                            channelList.InsertInt32(0);
    
                            SendPacket(0x0B, channelList);

                            BnetPacket joinChannel = new BnetPacket();

                            joinChannel.InsertInt32(0x02);
                            joinChannel.InsertString(botAccount.HomeChannel);

                            SendPacket(0x0C, joinChannel);

                            break;
                    
                        case 0x0E:
                            BnetPacket enterChat2 = new BnetPacket();
                            enterChat2.InsertString(botAccount.Username);
                            enterChat2.InsertString("");

                            SendPacket(0x0A, enterChat2);

                            BnetPacket channelList2 = new BnetPacket();
                            channelList2.InsertInt32(0);
    
                            SendPacket(0x0B, channelList2);

                            BnetPacket joinChannel2 = new BnetPacket();

                            joinChannel2.InsertInt32(0x02);
                            joinChannel2.InsertString(botAccount.HomeChannel);

                            SendPacket(0x0C, joinChannel2);

                            break;
                    }

                    break;
                case 0x3A:
                    int accountResult = dr.ReadInt32();

                    switch(accountResult)
                    {
                        case 0x00:
                            BnetPacket enterChat = new BnetPacket();
                            enterChat.InsertString(botAccount.Username);
                            enterChat.InsertString("");

                            SendPacket(0x0A, enterChat);

                            BnetPacket channelList = new BnetPacket();
                            channelList.InsertInt32(0);
    
                            SendPacket(0x0B, channelList);

                            BnetPacket joinChannel = new BnetPacket();

                            joinChannel.InsertInt32(0x02);
                            joinChannel.InsertString(botAccount.HomeChannel);

                            SendPacket(0x0C, joinChannel);

                            BnetPacket friendsList = new BnetPacket();
                            SendPacket(0x65, friendsList);
                            break;
                        case 0x01:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Account does not exist!";
                                eventQueue.Enqueue(cachedBeObj);
                            }

                            // Account creation for legacy clients
                            BnetPacket createAccount = new BnetPacket();

                            createAccount.InsertBytes(MBNCSUtil.OldAuth.HashPassword(botAccount.Password));                            
                            createAccount.InsertString(botAccount.Username);
                            SendPacket(0x3D, createAccount);

                            break;
                        case 0x02:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Invalid password!";

                                eventQueue.Enqueue(cachedBeObj);
                            }
                         //   Disconnect();

                            break;
                        case 0x06:
                            lock (eventSync)
                            {
                                cachedBeObj.EventId = 0x13;
                                cachedBeObj.Username = string.Empty;
                                cachedBeObj.Message = "Account closed!";

                                eventQueue.Enqueue(cachedBeObj);
                            }
                        //    Disconnect();

                            break;
                    }
                        
                    break;
                case 0x0B:
                    while (dr.PeekByte() != 0)
                    {
                        BnetEvent ber = new BnetEvent();
                        ber.EventId = 0x22; // EID_CHANNELLISTUSER
                        ber.Username = dr.ReadString();

                        Messenger.Default.Send<BnetEvent>(ber, connectionToken);
                    }

                    break;

                case 0x65:
                    int count = dr.ReadByte();
                    for (int i = 0; i < count; i++)
                    {
                        BnetFriend bf = new BnetFriend();
                        bf.Username = dr.ReadString();
                        byte status = dr.ReadByte();
                        byte friendLoc = dr.ReadByte();

                        string statusStr = string.Empty;
                        string locStr = string.Empty;

                        if (status == 0x00)
                            statusStr = "Friend";
                        else if (status == 0x01)
                            statusStr = "Friend (Mutual)";
                        else if (status == 0x02)
                            statusStr = "Friend (DND)";
                        else if (status == 0x04)
                            statusStr = "Friend (Away)";

                        if (friendLoc == 0x03)
                            locStr = ", in a public game.";
                        else if (friendLoc == 0x04 || friendLoc == 0x05)
                            locStr = ", in a private game.";
                        else
                            locStr = ", in a private channel.";

                        bf.Status = string.Format("{0}{1}", statusStr, locStr);
                        
                        dr.ReadInt32();
                        dr.ReadString();

                        Messenger.Default.Send<BnetFriend>(bf, connectionToken);
                    }

                    break;
                case 0x0A:
                    BnetPacket flPkt = new BnetPacket();
                    SendPacket(0x65, flPkt);

                    break;
                case 0x0F:
                    int eventId = dr.ReadInt32();
                    int flags = dr.ReadInt32();
                    int ping = dr.ReadInt32();
                   
                    dr.ReadInt32Array(3);

                    string username = dr.ReadString();
                    string message = dr.ReadString();

                    lock (eventSync)
                    {
                        BnetEvent cachedBeObj2 = new BnetEvent();
                        cachedBeObj2.EventId = eventId;
                        cachedBeObj2.Username = username;
                        cachedBeObj2.Message = message;
                        cachedBeObj2.Flags = flags;
                        cachedBeObj2.TimeStamp = DateTime.Now;

                        eventQueue.Enqueue(cachedBeObj2);
                    }

                    //Messenger.Default.Send<BnetEvent>(cachedBeObj2, connectionToken);
                    break;
                case 0x66:
                    // SID_FRIENDUPDATE
                    System.Diagnostics.Debug.WriteLine("Received Friend Update");
                    break;
            }
        }
        #endregion
    }
}
