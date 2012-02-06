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
using System.Windows.Data;

using Superiority.Model.Bnet;
using System.Windows.Media.Imaging;

namespace Superiority.Views.SLGlue
{
    public class ProductToIconValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BnetProduct p = (BnetProduct)value;
            // TODO: Convert this to a brush, and cache/freeze it.

            
            switch (p)
            {
                case BnetProduct.Starcraft:
                    return new BitmapImage(new Uri("/Superiority;component/Assets/Icons/star.jpg", UriKind.RelativeOrAbsolute));
                case BnetProduct.BroodWar:
                    return new BitmapImage(new Uri("/Superiority;component/Assets/Icons/sexp.jpg", UriKind.RelativeOrAbsolute));
                case BnetProduct.Warcraft2:
                    return new BitmapImage(new Uri("/Superiority;component/Assets/Icons/w2bn.jpg", UriKind.RelativeOrAbsolute));
                case BnetProduct.Diablo2:
                    return new BitmapImage(new Uri("/Superiority;component/Assets/Icons/d2dv.jpg", UriKind.RelativeOrAbsolute));
                case BnetProduct.LordOfDest:
                    return new BitmapImage(new Uri("/Superiority;component/Assets/Icons/d2xp.jpg", UriKind.RelativeOrAbsolute));
                case BnetProduct.Warcraft3:
                    return new BitmapImage(new Uri("/Superiority;component/Assets/Icons/war3.jpg", UriKind.RelativeOrAbsolute));
                case BnetProduct.FrozenThrone:
                    return new BitmapImage(new Uri("/Superiority;component/Assets/Icons/w3xp.jpg", UriKind.RelativeOrAbsolute));
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
