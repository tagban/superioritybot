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
    public class DataBuffer : IDisposable
    {
        protected MemoryStream memoryStream;
        public int Length { get; set; }

        public DataBuffer()
        {
            memoryStream = new MemoryStream();
        }

        public void InsertByte(byte b)
        {
            memoryStream.WriteByte(b);
            Length++;
        }

        public void InsertInt32(int i)
        {
            memoryStream.Write(BitConverter.GetBytes(i), 0, 4);
            Length += 4;
        }

        public void InsertInt64(long l)
        {
            memoryStream.Write(BitConverter.GetBytes(l), 0, 8);
            Length += 8;
        }

        public void InsertInt16(short s)
        {
            memoryStream.Write(BitConverter.GetBytes(s), 0, 2);
            Length += 2;
        }

        public void InsertBytes(byte[] bs)
        {
            memoryStream.Write(bs, 0, bs.Length);
            Length += bs.Length;
        }

        public void InsertString(string s)
        {
            byte[] tmp = System.Text.Encoding.UTF8.GetBytes(s);
            memoryStream.Write(tmp, 0, tmp.Length);
            memoryStream.WriteByte(0x00);

            Length += tmp.Length + 1;
        }

        public virtual byte[] GetBuffer()
        {
            byte[] data = new byte[Length];
            Buffer.BlockCopy(memoryStream.GetBuffer(), 0, data, 0, Length);
            return data;
        }

        public void Clear()
        {
            Length = 0;
            memoryStream.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            memoryStream.Dispose();
        }
    }
}
