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

namespace Superiority.Model.Bnet
{
    public class BnetEvent
    {
        public int EventId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Flags { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
    }
}
