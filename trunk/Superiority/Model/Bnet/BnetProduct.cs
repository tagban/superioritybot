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
    public enum BnetProduct : byte
    {
        Starcraft = 0x01,
        BroodWar = 0x02,
        Warcraft2 = 0x03,
        Diablo2 = 0x04,
        LordOfDest = 0x05,
        Warcraft3 = 0x07,
        FrozenThrone = 0x08
    }
}
