using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Environment
{
    public class SfaProcessOutputEventArgs : EventArgs
    {
        public string Line { get; }

        public SfaProcessOutputEventArgs(string line)
        {
            Line = line;
        }
    }
}
