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

namespace Superiority.Model.Bnls
{
    public class LogonProofRequestToken : IBnlsRequestToken
    {
        public int[] Salt
        {
            get;
            set;
        }

        public int[] ServerKey
        {
            get;
            set;
        }

        public int[] LogonProofData
        {
            get;
            set;
        }

        public byte[] RequestBuffer
        {
            get;
            set;
        }

        public int RequestBufferCount
        {
            get;
            set;
        }

        public LogonProofRequestToken(int[] salt, int[] serverKey)
        {
            Salt = salt;
            ServerKey = serverKey;
        }
    }
}
