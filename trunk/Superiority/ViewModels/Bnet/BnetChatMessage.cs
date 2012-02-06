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

namespace Superiority.ViewModels.Bnet
{
    public class BnetChatMessage
    {
        public BnetProduct Product
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
