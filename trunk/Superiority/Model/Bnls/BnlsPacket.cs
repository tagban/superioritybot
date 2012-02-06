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

namespace Superiority.Model.Bnls
{
    public class BnlsPacket : DataBuffer
    {
        public byte[] GetBuffer(byte pktId)
        {
            byte[] data = base.GetBuffer();
            byte[] pktData;

            using (DataBuffer tmp = new DataBuffer())
            {
                tmp.InsertInt16((short)(Length + 3));
                tmp.InsertByte(pktId);
                tmp.InsertBytes(data);

                pktData = tmp.GetBuffer();
            }

            return pktData;
        }
    }
}
