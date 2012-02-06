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
using GalaSoft.MvvmLight;

namespace Superiority.Model.Bnet
{
    public class BnetFriend : ViewModelBase
    {
        public string Username
        {
            get;
            set;
        }

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                VerifyPropertyName("Status");
                RaisePropertyChanged("Status");
            }
        }
    }
}
