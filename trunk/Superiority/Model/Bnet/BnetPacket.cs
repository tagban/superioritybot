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

using Superiority.Model.Util;
using System.IO;

namespace Superiority.Model.Bnet
{
    // TODO: Look into putting this on the stack, allows faster reclaiming of resources .. tho it excludes IDisposable .. hrm
    public class BnetPacket : DataBuffer
    {
        public byte[] GetBuffer(byte pktId)
        {
            byte[] data = base.GetBuffer();
            byte[] pktData; 

            using (DataBuffer tmp = new DataBuffer())
            {
                tmp.InsertByte(0xFF);
                tmp.InsertByte(pktId);
                tmp.InsertInt16((short)(Length + 4));
                tmp.InsertBytes(data);

                pktData = tmp.GetBuffer();
            }

            return pktData;
        }
    }
}
