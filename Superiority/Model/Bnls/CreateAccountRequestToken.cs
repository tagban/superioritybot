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
    public class CreateAccountRequestToken : IBnlsRequestToken
    {
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

        public int[] AccountCreateData
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

        public CreateAccountRequestToken(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
