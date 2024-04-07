using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CodexItemPropertyViewModel : ViewModelBase
    {
        public string Key { get => _key; set => SetAndRaise(ref _key, value); }

        public string Name { get => _name; set => SetAndRaise(ref _name, value); }

        public object Value { get => _value; set => SetAndRaise(ref _value, value); }

        public bool IsMaxValue { get => _isMaxValue; set => SetAndRaise(ref _isMaxValue, value); }

        public CodexItemViewModel Item { get => _item; set => SetAndRaise(ref _item, value); }

        private string _key;
        private string _name;
        private object _value;
        private bool _isMaxValue;
        private CodexItemViewModel _item;
    }
}
