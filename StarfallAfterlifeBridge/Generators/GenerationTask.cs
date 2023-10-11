using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public abstract class GenerationTask
    {
        public List<GenerationTask> Subtasks { get; } = new();

        public GenerationStatus Status { get; protected set; }

        public event EventHandler<GenerationProgressEventArgs> ProgressUpdated;

        protected abstract bool Generate();

        public Task<GenerationTask> Run()
        {
            return Task.Factory.StartNew(
                () => Run(null),
                TaskCreationOptions.LongRunning);
        }

        protected GenerationTask Run(GenerationTask parentTask = null)
        {
            Status = GenerationStatus.Running;
            ProgressUpdated?.Invoke(this, new(this, Status));
            parentTask?.OnChildTaskPtogressUpdated(this, Status);

            if (Generate() == true)
            {
                Status = GenerationStatus.Success;
            }
            else
            {
                Status = GenerationStatus.Fail;
            }

            ProgressUpdated?.Invoke(this, new(this, Status));
            parentTask?.OnChildTaskPtogressUpdated(this, Status);

            return this;
        }

        private void OnChildTaskPtogressUpdated(GenerationTask task, GenerationStatus status) =>
            ProgressUpdated?.Invoke(this, new(task, status));

        protected GenerationTask RunChildTasks(params GenerationTask[] tasks)
        {
            GenerationTask lastTask = null;

            foreach (var task in tasks)
            {
                if (task is null)
                    continue;

                lastTask = task;

                if (task.Run(this).Status == GenerationStatus.Fail)
                    break;
            }

            return lastTask;
        }

        public static Task<GenerationTask> RunTasks(params GenerationTask[] tasks)
        {
            return Task.Factory.StartNew(() =>
            {
                GenerationTask lastTask = null;

                foreach (var task in tasks)
                {
                    if (task is null)
                        continue;

                    lastTask = task;

                    if (task.Run(null).Status == GenerationStatus.Fail)
                        break;
                }

                return lastTask;
            });
        }
    }
}
