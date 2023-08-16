using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarfallAfterlife
{
    public class ConsoleWriter : TextWriter
    {
        public TextBox Output { get; }
        public override Encoding Encoding => Console.OutputEncoding;

        protected Task CurrentTask { get; set; } = null;
        protected string Buffer { get; set; } = string.Empty;
        protected object WriteLocker { get; } = new object();


        public ConsoleWriter(TextBox output)
        {
            lock(WriteLocker)
                Output = output;
        }

        public override void Write(char value)
        {
            WriteToBuffer(value.ToString());
        }

        public override void Write(char[] buffer, int index, int count)
        {
            WriteToBuffer(new string(buffer, index, count));
        }

        protected virtual void WriteToBuffer(string text)
        {
            lock (WriteLocker)
            {
                if (Buffer is null)
                    Buffer = string.Empty;

                Buffer += text;

                if (CurrentTask is not null)
                    return;

                CurrentTask = Task.Factory.StartNew(() =>
                {
                    while (string.IsNullOrEmpty(Buffer) == false)
                    {
                        ReleaseBuffer();
                        Thread.Sleep(100);
                    }

                    lock (WriteLocker)
                        CurrentTask = null;
                });
            }
        }

        protected virtual void ReleaseBuffer()
        {
            lock (WriteLocker)
            {
                string text = Buffer;
                Buffer = string.Empty;

                Output?.BeginInvoke(new Action(() =>
                {
                    Output.AppendText(text);
                }));
            }
        }
    }
}
