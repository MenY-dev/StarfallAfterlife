using Avalonia.Data;
using Avalonia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetAndRaise<T>(ref T field, T newValue, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue) == true)
                return false;

            var oldValue = field;
            field = newValue;
            RaisePropertyChanged(oldValue, newValue, name);

            return true;
        }

        protected bool SetAndRaise<T>(T oldValue, T newValue, Action<T> setter, [CallerMemberName] string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(oldValue, newValue) == true)
                return false;

            setter?.Invoke(newValue);
            RaisePropertyChanged(oldValue, newValue, name);
            return true;
        }

        protected void RaisePropertyChanged(object value, [CallerMemberName] string name = null) =>
            RaisePropertyChanged(value, value, name);

        protected void RaisePropertyChanged(object oldValue, object newValue, [CallerMemberName] string name = null)
        {
            Trace.WriteLine($"PropertyChanged (Name={name}, Owner={GetType().Name}, OldValue={oldValue}, NewValue={newValue})");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            OnPropertyChanged(oldValue, newValue, name);
        }

        protected virtual void OnPropertyChanged(object oldValue, object newValue, string name)
        {

        }
    }
}
