using System;

namespace StarfallAfterlife.Bridge.Server
{
    public class PlayerStatusInfoEventArgs : EventArgs
    {
        public PlayerStatusInfo Info { get; }

        public PlayerStatusInfoEventArgs(PlayerStatusInfo info)
        {
            Info = info;
        }
    }
}