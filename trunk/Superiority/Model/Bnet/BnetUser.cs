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
using System.ComponentModel;

namespace Superiority.Model.Bnet
{
    public class BnetUser : INotifyPropertyChanged
    {
        public string DisplayName
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public BnetProduct Product
        {
            get;
            set;
        }

        // Flag Update pkt requires INPC .. ffs.
        private int flags;
        public int Flags
        {
            get
            {
                return flags;
            }
            set
            {
                flags = value;
                OnPropertyChanged("Flags");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
