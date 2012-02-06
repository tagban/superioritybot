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
using Superiority.Model.Bnet;

namespace Superiority.Model.Bnls
{
    public class LogonChallengeRequestToken : IBnlsRequestToken
    {
        public BnetProduct Product
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

        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public int[] ChallengeData
        {
            get;
            set;
        }

        public LogonChallengeRequestToken(string username, string password, BnetProduct prod)
        {
            Username = username;
            Password = password;
            Product = prod;
        }
    }
}
