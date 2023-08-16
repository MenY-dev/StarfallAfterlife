using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking
{
    public interface IServer
    {
        event EventHandler<EventArgs> StateChanged;

        bool IsStarted { get; }

        void Start();

        void Stop();
    }
}
