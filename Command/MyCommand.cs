using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ColumnDimensions.Command
{
    public class MyCommand : ICommand
    {
        #region Event
        public event EventHandler CanExecuteChanged;

        #endregion

        #region Property
        public Action<object> Excute;
        public Predicate<object> CanExcute;
        #endregion

        #region Constructor
        public MyCommand(Action<object> excute, Predicate<object> canExcute)
        {
            this.Excute = excute;
            this.CanExcute = canExcute;
        }
        #endregion

        #region Methods
        public bool CanExecute(object parameter)
        {
            return CanExcute(parameter);
        }
        public void Execute(object parameter)
        {
            Excute(parameter);
        }
        #endregion

    }
}
