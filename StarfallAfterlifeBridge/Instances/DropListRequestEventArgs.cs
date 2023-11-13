using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class DropListRequestEventArgs : EventArgs
    {
        public string Auth { get; }
        public string DropName { get; }

        public DropListRequestEventArgs(string auth, string dropName)
        {
            Auth = auth;
            DropName = dropName;
        }
    }
}
