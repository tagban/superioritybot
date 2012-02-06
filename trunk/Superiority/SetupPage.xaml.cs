using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Superiority.Views
{
    public partial class SetupPage : UserControl
    {
        public SetupPage()
        {
            InitializeComponent();
            Application.Current.InstallStateChanged += new EventHandler(Current_InstallStateChanged);
        }

        void Current_InstallStateChanged(object sender, EventArgs e)
        {
            if (Application.Current.InstallState == InstallState.Installed)
            {
            }
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Install();
        }
    }
}
