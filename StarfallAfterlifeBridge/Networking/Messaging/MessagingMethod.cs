using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public enum MessagingMethod : byte
    {
        Unknown = 0,
        Binary = 1,
        BinaryRequest = 2,
        Text = 3,
        TextRequest = 4,
        HTTP = 5
    }
}
