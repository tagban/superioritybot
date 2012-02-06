using System;

namespace Superiority.Model.Bnet
{
    public class LegacyProfileRequest : ProfileRequest
    {
        public string Wins
        {
            get;
            set;
        }

        public string Losses
        {
            get;
            set;
        }

        public string LastGame
        {
            get;
            set;
        }

        public string LastGameResult
        {
            get;
            set;
        }
    }
}
