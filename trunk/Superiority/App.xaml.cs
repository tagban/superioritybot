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

using Superiority.Views;
using Superiority.ViewModels;
using System.Net.NetworkInformation;

using GalaSoft.MvvmLight.Messaging;

namespace Superiority
{
    public partial class App : Application
    {
        public App()
        {
            this.Startup += this.Application_Startup;
            this.UnhandledException += this.Application_UnhandledException;
            this.Exit += new EventHandler(App_Exit);
            NetworkChange.NetworkAddressChanged+=new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);
        
            InitializeComponent();
        }

        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                
            }
        }

        void App_Exit(object sender, EventArgs e)
        {
            ViewModelLocator.Cleanup();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();

            if (this.IsRunningOutOfBrowser && this.HasElevatedPermissions)
                this.RootVisual = new MainPage();
            else
                this.RootVisual = new SetupPage();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }
    }
}
