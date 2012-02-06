using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using GalaSoft.MvvmLight.Messaging;
using Superiority.Model.Util;
using Superiority.Model.Networking.Messages;

namespace Superiority.Model.Networking
{
    /// <summary>
    /// Base connection  class in Superiority. BnetConnection and BnlsService derive from it.
    /// </summary>
    public abstract class Connection : IDisposable
    {
        #region Connection Fields
        /// <summary>
        /// Used to talk to the Connection subscriber
        /// </summary>
        protected Guid connectionToken;

        /// <summary>
        /// This is used to ensure exclusivity to the connecting operation, useful for "service" scenarios like Bnls.
        /// </summary>
        protected AutoResetEvent connectingResetEvent;

        /// <summary>
        /// Hack used to make sure we're actually connected
        /// </summary>
        protected AutoResetEvent connectingVerifyResetEvent;

        /// <summary>
        /// This is used to ensure exclusivity to the receiving operation, useful for "service" scenarios like Bnls.
        /// </summary>
        protected AutoResetEvent receivingResetEvent;

        /// <summary>
        /// The underlying Socket object.
        /// </summary>
        protected Socket underlyingSocket;

        /// <summary>
        /// Pool of SocketAsyncEventArgs related to connections
        /// </summary>
        protected MemoryPool<SocketAsyncEventArgs> connectionPool;

        /// <summary>
        /// Pool of SocketAsyncEventArgs related to transmitting data
        /// </summary>
        protected MemoryPool<SocketAsyncEventArgs> transmitPool;
        #endregion

        #region Connection Properties
        /// <summary>
        /// Determines if the Connection has successfully connected to the RemoteEndPoint.
        /// </summary>
        public bool IsConnected
        {
            get;
            set;
        }

        /// <summary>
        ///  Remote End Point for the underlying Connection
        /// </summary>
        public DnsEndPoint RemoteEndPoint
        {
            get;
            set;
        }
        #endregion

        #region Connection Constructor
        /// <summary>
        /// Creates an instance of Connection
        /// </summary>
        /// <param name="remoteEp">The remote end point</param>
        /// <param name="messageToken">Token used to send replies back to the subscriber</param>
        public Connection(DnsEndPoint remoteEp, Guid messageToken)
        {
            if (remoteEp == null)
                throw new ArgumentNullException("remoteEp");

            if (messageToken == null)
                throw new ArgumentNullException("messageToken");

            connectionToken = messageToken;
            receivingResetEvent = new AutoResetEvent(true);
            
            // TODO: Test with multiple pending threads .. do we need to make this a ManualResetEvent .. ?
            connectingResetEvent = new AutoResetEvent(true);

            // Slight hack needed. Our first reset event blocks other threads, this one signals a connection was made
            connectingVerifyResetEvent = new AutoResetEvent(false);

            connectionPool = new MemoryPool<SocketAsyncEventArgs>();
            transmitPool = new MemoryPool<SocketAsyncEventArgs>();
           
            RemoteEndPoint = remoteEp;
        }

        public Connection()
        {
            receivingResetEvent = new AutoResetEvent(true);

            // TODO: Test with multiple pending threads .. do we need to make this a ManualResetEvent .. ?
            connectingResetEvent = new AutoResetEvent(true);

            // Slight hack needed. Our first reset event blocks other threads, this one signals a connection was made
            connectingVerifyResetEvent = new AutoResetEvent(false);

            connectionPool = new MemoryPool<SocketAsyncEventArgs>();
            transmitPool = new MemoryPool<SocketAsyncEventArgs>();
        }

        #endregion

        #region Connection Methods
        /// <summary>
        /// Connects the underlying connection if it is not already connected.
        /// </summary>
        public void Connect()
        {
            // Ensure exclusivity to the resource, we only want one connection attempt.
            connectingResetEvent.WaitOne();

            if (!IsConnected)
            {
                // Make a new instance of Socket, in Silverlight there is no DisconnectAsync on Sockets.
                underlyingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Pull down a SocketAsyncEventArg to connect with.
                var connectAsyncEventArgs = connectionPool.Pull();
                connectAsyncEventArgs.RemoteEndPoint = RemoteEndPoint;

                if (connectAsyncEventArgs.LastOperation == SocketAsyncOperation.None)
                    connectAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AsyncEventCompleted);

                // Did the operation complete synchronously? 
                if (!underlyingSocket.ConnectAsync(connectAsyncEventArgs))
                    AsyncEventCompleted(this, connectAsyncEventArgs);

                connectingVerifyResetEvent.WaitOne();
            }
        }

        /// <summary>
        /// Sends data to the underlying connection if it is connected.
        /// NOTE: Will NOT connect the underlying socket if it is not connected already.
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        public void Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (IsConnected)
            {
                // Pull down a SocketAsyncEventArg to send with
                var sendAsyncEventArgs = transmitPool.Pull();
                sendAsyncEventArgs.RemoteEndPoint = RemoteEndPoint;

                if (sendAsyncEventArgs.LastOperation == SocketAsyncOperation.None)
                    sendAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AsyncEventCompleted);

                // Set our buffer
                sendAsyncEventArgs.SetBuffer(data, 0, data.Length);

                // Did the operation complete synchroously? 
                if (!underlyingSocket.SendAsync(sendAsyncEventArgs))
                    AsyncEventCompleted(this, sendAsyncEventArgs);
            }
        }

        /// <summary>
        /// Receives data from the underlying connection if it is connected.
        /// NOTE: Will NOT connect the underlying socket if it is not connected already.
        /// </summary>
        /// <param name="length">Amount in bytes to receive</param>
        /// <param name="token">UserToken to use for multiple receives</param>
        public void Receive(int length, object token)
        {
            if (IsConnected)
            {
                // Ensure exclusivity to the resource. We only want one receive at a time. 
                receivingResetEvent.WaitOne();

                // Pull down a SocketAsyncEventArg to receive with.
                var receiveAsyncEventArgs = transmitPool.Pull();
                receiveAsyncEventArgs.RemoteEndPoint = RemoteEndPoint;

                if (receiveAsyncEventArgs.LastOperation == SocketAsyncOperation.None)
                    receiveAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AsyncEventCompleted);

                // Set our buffer and the data we've already received.
                receiveAsyncEventArgs.UserToken = token;
                receiveAsyncEventArgs.SetBuffer(new byte[length], 0, length);

                if (!underlyingSocket.ReceiveAsync(receiveAsyncEventArgs))
                    AsyncEventCompleted(this, receiveAsyncEventArgs);
            }
        }

        /// <summary>
        /// Special internal version of Receive which does not lock. Used by the accessee of the resource.
        /// TODO: Think of better way to do this, this is fine for now though.
        /// </summary>
        protected void ReceiveNoLock(int length, object token)
        {
            if (token == null)
                throw new ArgumentNullException("alreadyReceivedData");
     
            if (IsConnected)
            {
                // Pull down a SocketAsyncEventArg to receive with.
                var receiveAsyncEventArgs = transmitPool.Pull();
                receiveAsyncEventArgs.RemoteEndPoint = RemoteEndPoint;

                if (receiveAsyncEventArgs.LastOperation == SocketAsyncOperation.None)
                    receiveAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AsyncEventCompleted);

                // Set our buffer and the data we've already received.
                receiveAsyncEventArgs.UserToken = token;
                receiveAsyncEventArgs.SetBuffer(new byte[length], 0, length);

                if (!underlyingSocket.ReceiveAsync(receiveAsyncEventArgs))
                    AsyncEventCompleted(this, receiveAsyncEventArgs);
            }
        }

        /// <summary>
        /// User overridable method which signals the underlying connection has been established.
        /// </summary>
        protected virtual void Connected()
        {
        }

        /// <summary>
        /// User overridable method which signals that data has come in.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void ReceivedData(SocketAsyncEventArgs e)
        {
        }

        /// <summary>
        /// Hub for the completion of all I/O events on the underlying connection
        /// </summary>
        private void AsyncEventCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (isDisposed)
                return;

            switch (e.SocketError)
            {
                // Was the operation successful?
                case SocketError.Success:
                    switch (e.LastOperation)
                    {
                        case SocketAsyncOperation.Connect:
                        // Set IsConnected to true and dispatch a message to our subscriber
                        IsConnected = true;
                        Messenger.Default.Send<ConnectionMessage>(new ConnectionMessage() { MessageId = ConnectionMessageId.ConnectionConnected, ConnectionName = "Testing", AdditionalInformation = e.RemoteEndPoint.ToString() }, connectionToken);

                        Connected();
    
                        // Open the flood gates, we're connected!
                        connectingResetEvent.Set();
                        connectingVerifyResetEvent.Set();
                        break;

                        case SocketAsyncOperation.Receive:
                            if (e.BytesTransferred == 0)
                            {
                                if (e.LastOperation == SocketAsyncOperation.Connect)
                                    connectionPool.Push(e);
                                else
                                    transmitPool.Push(e);

                                // Terminate our connection.
                                IsConnected = false;
                                Messenger.Default.Send<ConnectionMessage>(new ConnectionMessage() { MessageId = ConnectionMessageId.ConnectionError, ConnectionName = "Testing", AdditionalInformation = e.SocketError.ToString() }, connectionToken);

                                // Send our Disconnection message, and try to reconnect
                                Messenger.Default.Send<ConnectionMessage>(new ConnectionMessage() { MessageId = ConnectionMessageId.ConnectionDisconnected, ConnectionName = "Testing", AdditionalInformation = string.Empty }, connectionToken);

                                return;
                            }
                            else
                            {
                                ReceivedData(e);
                            }
                        break;
                    }

                    // Put our items back on the pool.
                    if (e.LastOperation == SocketAsyncOperation.Connect)
                        connectionPool.Push(e);
                    else
                        transmitPool.Push(e);

                    break;
                // No? Send the error message off, reset the connection, and continue on :) 
                default:
                    // Put our items back on the pool.
                    // NOTE: This is done early here to ensure maximum efficiency when we reconnect. 
                    if (e.LastOperation == SocketAsyncOperation.Connect)
                        connectionPool.Push(e);
                    else
                        transmitPool.Push(e);

                    receivingResetEvent.Set();

                    // Terminate our connection.
                    IsConnected = false;
                    Messenger.Default.Send<ConnectionMessage>(new ConnectionMessage() { MessageId = ConnectionMessageId.ConnectionError, ConnectionName = "Testing", AdditionalInformation = e.SocketError.ToString() }, connectionToken);

                    // Send our Disconnection message, and try to reconnect
                    Messenger.Default.Send<ConnectionMessage>(new ConnectionMessage() { MessageId = ConnectionMessageId.ConnectionDisconnected, ConnectionName = "Testing", AdditionalInformation = string.Empty }, connectionToken);

                    break;
            }
        }
        #endregion

        #region IDisposable
        #region IDisposable Property
        /// <summary>
            /// Determines if the resources have been disposed of already.
            /// </summary>
            protected bool isDisposed;
            #endregion
        
        /// <summary>
        /// Dispose of the underlying connection, any pools, and clean up managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    underlyingSocket.Shutdown(SocketShutdown.Send);
                    underlyingSocket.Close();
                    underlyingSocket.Dispose();
                  
                    connectingResetEvent.Dispose();
                    connectingVerifyResetEvent.Dispose();
                    receivingResetEvent.Dispose();
                }

                underlyingSocket = null;
                connectingResetEvent = null;
                receivingResetEvent = null;

                isDisposed = true;
            }   
        }
        #endregion
    }
}
