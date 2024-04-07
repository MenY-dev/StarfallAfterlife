using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CodexEntryViewModel : ViewModelBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsVisible { get => _isVisible; set => SetAndRaise(ref _isVisible, value); }

        public bool IsExpanded { get => _isExpanded; set => SetAndRaise(ref _isExpanded, value); }

        private bool _isVisible = true;
        private bool _isExpanded = false;
    }

    public class CodexEntryGroupViewModel : CodexEntryViewModel
    {
        public int Count { get => _count; set => SetAndRaise(ref _count, value); }

        public ObservableCollection<CodexEntryViewModel> Items { get; set; }

        private int _count = 0;
    }
}
