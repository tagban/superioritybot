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

namespace Superiority.ViewModels
{
    public class ViewModelLocator
    {
        private static MainWindowViewModel mainWindowViewModel;
        public static MainWindowViewModel MainWindowViewModelStatic
        {
            get
            {
                if (mainWindowViewModel == null)
                    mainWindowViewModel = new MainWindowViewModel();

                return mainWindowViewModel;
            }
        }
        
        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                return MainWindowViewModelStatic;
            }
        }
        
        public static void Cleanup()
        {
        }
    }
}
