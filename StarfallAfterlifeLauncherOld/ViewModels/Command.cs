using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class Command : CommandBase
    {
        private readonly Action<object> _execute;

        private readonly Func<object, bool> _canExecute;

        public Command(Action execute, Func<object, bool> canExecute = null) : this(_ => execute(), canExecute) { }

        public Command(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        public override void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}
