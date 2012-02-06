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
using System.IO;

namespace Superiority.Model.Util
{
    public class DataReader
    {
        protected byte[] readerData;
        protected int readerLength;

        public DataReader(byte[] incomingData)
        {
            readerData = incomingData;
            readerLength = 0;
        }

        public byte ReadByte()
        {
            return readerData[readerLength++];
        }
        
        public byte PeekByte()
        {
            return readerData[readerLength];
        }

        public int ReadInt32()
        {
            int i = BitConverter.ToInt32(readerData, readerLength);
            readerLength += 4;

            return i;
        }

        public int[] ReadInt32Array(int amount)
        {
            int[] i = new int[amount];
            Buffer.BlockCopy(readerData, readerLength, i, 0, amount * 4);

            readerLength += amount * 4;

            return i;
        }

        public long ReadInt64()
        {
            long l = BitConverter.ToInt64(readerData, readerLength);
            readerLength += 8;

            return l;
        }

        public short ReadInt16()
        {
            short s = BitConverter.ToInt16(readerData, readerLength);
            readerLength += 2;

            return s;
        }

        public byte[] ReadBytes(int amount)
        {
            byte[] tmp = new byte[amount];
            Buffer.BlockCopy(readerData, readerLength, tmp, 0, amount);

            readerLength += amount;

            return tmp;
        }

        public string ReadString()
        {
            int origLen = readerLength;

            while (readerData[origLen] != ((char)'\0' & 0xFF))
                origLen++;

            string s = System.Text.Encoding.UTF8.GetString(readerData, readerLength, origLen-readerLength);
            readerLength = ++origLen;

            return s;
        }

        public byte[] ReadByteArrayNt()
        {
            int origLen = readerLength;

            while (readerData[origLen] != 0)
                origLen++;

            byte[] tmp = new byte[origLen - readerLength];

            Buffer.BlockCopy(readerData, readerLength, tmp, 0, tmp.Length);
            readerLength = ++origLen;

            return tmp;
        }
    }
}
