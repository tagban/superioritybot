using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Superiority.Model.Networking.Messages
{
    public enum ConnectionMessageId : byte
    {
        ConnectionConnected = 0,
        ConnectionDisconnected, 
        ConnectionError
    }

    public class ConnectionMessage
    {
        #region Connection Message Fields
        /// <summary>
        /// Message ID used to tell what kind of message was sent to the subscriber
        /// </summary>
        public ConnectionMessageId MessageId
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the connection sending the message.
        /// </summary>
        public string ConnectionName
        {
            get;
            set;
        }

        /// <summary>
        /// Additional information regarding the message.
        /// </summary>
        public string AdditionalInformation
        {
            get;
            set;
        }
        #endregion
    }
}
