using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using Superiority.Model.Bnet;
using Superiority.Model.Networking;
using Superiority.Model.Util;
using GalaSoft.MvvmLight.Messaging;
using System.Threading;

namespace Superiority.Model.Bnls
{
    public class BnlsService : Connection
    {
        #region Singleton
        static readonly BnlsService instance = new BnlsService();
        public static Guid BnlsToken = Guid.NewGuid();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static BnlsService()
        {
        }

        public static BnlsService Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion
        private Timer t;


        BnlsService()
        {
            RemoteEndPoint = new DnsEndPoint("bnls.net", 9367);
            connectionToken = BnlsToken;
            t = new Timer(TimerTick);
        }

        void TimerTick(object state)
        {
            if (IsConnected)
            {
                BnlsPacket p = new BnlsPacket();
                SendPacket(0x00, p);

                t.Change(30000, 0);
            }
        }

        private void SendPacket(byte pktId, BnlsPacket packet)
        {
            System.Diagnostics.Debug.WriteLine("BNLS C->S {0:X}", pktId);
            byte[] packetData = packet.GetBuffer(pktId);

            Send(packetData);
            packet.Dispose();
        }

        private void ParseBnls(byte[] incomingData, IBnlsRequestToken node)
        {
            DataReader dr = new DataReader(incomingData);

            dr.ReadInt16();
            byte pktId = dr.ReadByte();

            switch (pktId)
            {
                case 0x01:
                    VersionCheckRequestToken vcrt2 = node as VersionCheckRequestToken;

                    dr.ReadInt32();
                    int cdKeySeed = dr.ReadInt32();
                    vcrt2.ClientToken = cdKeySeed;
                   
                    int[] cdKeyData = dr.ReadInt32Array(9);

                    vcrt2.CdKeySeed = cdKeySeed;
                    vcrt2.CdKeyData = cdKeyData;

                    Messenger.Default.Send<IBnlsRequestToken>(node, node);

                    vcrt2.RequestBuffer = null;
                    vcrt2.RequestBufferCount = 0;
                    break;

                case 0x02:
                    LogonChallengeRequestToken lcrt = node as LogonChallengeRequestToken;

                    int[] challengeData = dr.ReadInt32Array(8);

                    lcrt.ChallengeData = challengeData;
                    Messenger.Default.Send<IBnlsRequestToken>(node, node);

                    lcrt.RequestBuffer = null;
                    lcrt.RequestBufferCount = 0;

                    break;

                case 0x03:
                    LogonProofRequestToken lprt = node as LogonProofRequestToken;

                    int[] logonProofData = dr.ReadInt32Array(5);
                    lprt.LogonProofData = logonProofData;

                    Messenger.Default.Send<IBnlsRequestToken>(node, node);

                    lprt.RequestBuffer = null;
                    lprt.RequestBufferCount = 0;

                    break;

                case 0x04:
                    CreateAccountRequestToken cart = node as CreateAccountRequestToken;

                    int[] accountCreateData = dr.ReadInt32Array(16);
                    cart.AccountCreateData = accountCreateData;

                    Messenger.Default.Send<IBnlsRequestToken>(node, node);

                    cart.RequestBuffer = null;
                    cart.RequestBufferCount = 0;

                    break;

                case 0x0D:
                    node.RequestBuffer = null;
                    node.RequestBufferCount = 0;

                    break;
                
                case 0x1A:
                    dr.ReadInt32();
                    VersionCheckRequestToken vcrt = node as VersionCheckRequestToken;

                    vcrt.Version = dr.ReadInt32();
                    vcrt.Checksum = dr.ReadInt32();
                    vcrt.Statstring = dr.ReadByteArrayNt();

                    dr.ReadInt32();

                    vcrt.VersionByte = dr.ReadInt32();

                    Messenger.Default.Send<IBnlsRequestToken>(node, node);

                    vcrt.RequestBufferCount = 0;
                    vcrt.RequestBuffer = null;

                    break;
            }
        }

        public void BnlsLogonProofRequest(LogonProofRequestToken requestToken)
        {
            if (!IsConnected)
            {
                Connect();
            }

            BnlsPacket logonProof = new BnlsPacket();

            foreach (var saltData in requestToken.Salt)
                logonProof.InsertInt32(saltData);

            foreach (var serverKeyData in requestToken.ServerKey)
                logonProof.InsertInt32(serverKeyData);

            SendPacket(0x03, logonProof);
            Receive(3, requestToken);
        }

        public void BnlsCreateAccountRequest(CreateAccountRequestToken requestToken)
        {
            if (!IsConnected)
            {
                Connect();
            }

            BnlsPacket createAccount = new BnlsPacket();
            createAccount.InsertString(requestToken.Username);
            createAccount.InsertString(requestToken.Password);

            SendPacket(0x04, createAccount);
            Receive(3, requestToken);
        }

        public void BnlsLogonChallengeRequest(LogonChallengeRequestToken requestToken)
        {
            if (!IsConnected)
            {
                Connect();
            }

            if (((BnetProduct)requestToken.Product) == BnetProduct.Warcraft3)
            {
                BnlsPacket chooseNlsRevision = new BnlsPacket();
                chooseNlsRevision.InsertInt32(0x02);

                SendPacket(0x0D, chooseNlsRevision);
                Receive(3, requestToken);
            }

            BnlsPacket logonChallenge = new BnlsPacket();
            logonChallenge.InsertString(requestToken.Username);
            logonChallenge.InsertString(requestToken.Password);

            SendPacket(0x02, logonChallenge);
            Receive(3, requestToken);
        }
        

        public void BnlsVersionCheckRequest(VersionCheckRequestToken requestToken)
        {
            if (!IsConnected)
            {
                Connect();
            }

            BnlsPacket versionCheckEx2 = new BnlsPacket();
            versionCheckEx2.InsertInt32(requestToken.Product); // Product
            versionCheckEx2.InsertInt32(0); // Flags
            versionCheckEx2.InsertInt32(Environment.TickCount); // Cookie - unused
            versionCheckEx2.InsertInt64(requestToken.FileTime); // Filetime
            versionCheckEx2.InsertString(requestToken.VersionName); // Version file name
            versionCheckEx2.InsertBytes(requestToken.ChecksumFormula); // Checksum Formula
            versionCheckEx2.InsertByte(0x00);

            System.Diagnostics.Debug.WriteLine("Checksum Form");
            DataFormatter.WriteToTrace(requestToken.ChecksumFormula);

            SendPacket(0x1A, versionCheckEx2); // BNLS_VERSIONCHECK 
            Receive(3, requestToken);
        }

        protected override void Connected()
        {
            connectingVerifyResetEvent.Set();
            t.Change(30000,0);
        }

        protected override void ReceivedData(SocketAsyncEventArgs e)
        {
            byte[] receivedData = e.Buffer;
            IBnlsRequestToken node = e.UserToken as IBnlsRequestToken;
            int pktLen;

            if (node.RequestBuffer == null)
            {
                pktLen = BitConverter.ToInt16(receivedData, 0);
                node.RequestBuffer = new byte[pktLen];
            }
            else
            {
                pktLen = BitConverter.ToInt16(node.RequestBuffer, 0);
            }

            Buffer.BlockCopy(receivedData, 0, node.RequestBuffer, node.RequestBufferCount, receivedData.Length);
            node.RequestBufferCount += receivedData.Length;

            if (node.RequestBufferCount < pktLen)
            {
                int diffLen = pktLen - node.RequestBufferCount;
                ReceiveNoLock(diffLen, e.UserToken);
            }
            else
            {
                receivingResetEvent.Set();
                ParseBnls(node.RequestBuffer, node);
            }
        }
    }
}
