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
            set => SetAndRaise(ref _msg, value);
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
            set => SetAndRaise(ref _count, value);
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

    }
}
