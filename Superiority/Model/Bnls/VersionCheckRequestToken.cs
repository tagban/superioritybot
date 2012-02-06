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
    public class VersionCheckRequestToken : IBnlsRequestToken
    {
        public int Version
        {
            get;
            set;
        }

        public int Checksum
        {
            get;
            set;
        }

        public byte[] Statstring
        {
            get;
            set;
        }

        public int CdKeySeed
        {
            get;
            set;
        }

        public int[] CdKeyData
        {
            get;
            set;
        }

        public long FileTime
        {
            get;
            set;
        }

        public int Product
        {
            get;
            set;
        }

        public string VersionName
        {
            get;
            set;
        }

        public byte[] ChecksumFormula
        {
            get;
            set;
        }

        public int ServerToken
        {
            get;
            set;
        }

        public string CdKey
        {
            get;
            set;
        }

        public int ClientToken
        {
            get;
            set;
        }

        public byte[] RequestBuffer
        {
            get;
            set;
        }

        public int RequestBufferCount
        {
            get;
            set;
        }

        public int VersionByte
        {
            get;
            set;
        }

        public VersionCheckRequestToken(int product, long fileTime, string versionName, byte[] checksumFormula, int serverToken, string cdKey)
        {
            Product = product;
            FileTime = fileTime;
            VersionName = versionName;
            ChecksumFormula = checksumFormula;
            ServerToken = serverToken;
            CdKey = cdKey;
        }
    }
}
