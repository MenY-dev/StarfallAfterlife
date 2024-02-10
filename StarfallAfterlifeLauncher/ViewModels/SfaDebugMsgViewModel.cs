using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class SfaDebugMsgViewModel : ViewModelBase
    {

        public string Msg
        {
            get => _msg;
            set
            {
                var needCollapseOld = СontainsDuplicates;
                SetAndRaise(ref _msg, value);
                RaisePropertyChanged(needCollapseOld, NeedCollapse, nameof(NeedCollapse));
            }
        }

        public string Channel
        {
            get => _channel;
            set => SetAndRaise(ref _channel, value);
        }

        public DateTime Time
        {
            get => _time;
            set => SetAndRaise(ref _time, value);
        }

        public int Count
        {
            get => _count;
            set
            {
                var containsDuplicatesOld = СontainsDuplicates;

                SetAndRaise(ref _count, value);
                RaisePropertyChanged(
                    containsDuplicatesOld,
                    СontainsDuplicates,
                    nameof(СontainsDuplicates));
            }
        }

        public bool СontainsDuplicates => Count > 1;

        public bool NeedCollapse => Msg is not null and { Length: > 500 };

        public bool Expanded
        {
            get => _expanded;
            set => SetAndRaise(ref _expanded, value);
        }

        private string _msg;
        private string _channel;
        private DateTime _time;
        private int _count = 1;
        private bool _expanded;

        public bool IsStackable(SfaDebugMsgViewModel item)
        {
            if (item is null ||
                item.Channel != Channel ||
                item.Msg != Msg)
                return false;

            return true;
        }
        
        public void ToggleExpand()
        {
            Expanded = !Expanded;
        }

        public override string ToString() => ToString();

        public string ToString(int maxMsgLength = -1)
        {
            var sb = new StringBuilder();

            sb.Append($"[{Time:T}][{Channel ?? "Log"}]");

            if (Count > 1)
            {
                sb.Append('[');
                sb.Append(Count);
                sb.Append('}');
            }

            if (Msg is not null)
            {
                sb.Append(' ');

                if (maxMsgLength > -1)
                {
                    var length = Math.Min(maxMsgLength, Msg.Length);
                    sb.Append(Msg ?? "", 0, length);

                    if (length < Msg.Length)
                    {
                        sb.Append("...[");
                        sb.Append(Msg.Length - length);
                        sb.Append(" chars truncated]");
                    }
                }
                else
                {
                    sb.Append(Msg ?? "");
                }
            }

            return sb.ToString();
        }
    }
}
