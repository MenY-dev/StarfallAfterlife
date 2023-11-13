using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class DropListResponseEventArgs : EventArgs
    {
        public string DropName { get; }

        public string Auth { get; }

        public string Data { get; }

        public DropListResponseEventArgs(string dropName, string auth, string data)
        {
            DropName = dropName;
            Auth = auth;
            Data = data;
        }
    }
}
