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
using System.Windows.Navigation;

namespace Superiority.Views
{
    public partial class SettingsScreen : Page
    {
        public SettingsScreen()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModels.ViewModelLocator.MainWindowViewModelStatic.SaveSettings();
            SelectAccountButton.Click -= SelectAccountButton_Click;

            base.OnNavigatingFrom(e);
        }

        private void SelectAccountButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAccountDialog d = new SelectAccountDialog();
            d.DataContext = this.DataContext;
            d.Show();
        }
    }
}
