using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CommandBase<TViewModel> : CommandBase
    {
        public TViewModel ViewModel { get; }

        public CommandBase(TViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }

    public class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public virtual void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {

        }
    }
}
