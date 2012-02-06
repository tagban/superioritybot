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
    public interface IBnlsRequestToken
    {
        byte[] RequestBuffer { get; set; }
        int RequestBufferCount { get; set; } // TODO: Buffer.Length makes this redundant
    }
}
