using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class GenerationProgressEventArgs : EventArgs
    {
        public GenerationTask Task { get; }
        public GenerationStatus Status { get; }

        public GenerationProgressEventArgs(GenerationTask task, GenerationStatus status)
        {
            Task = task;
            Status = status;
        }
    }
}
