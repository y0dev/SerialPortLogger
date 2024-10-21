using System;
using System.Windows.Input;

namespace SerialLogAnalyzer.Commands
{
	/// <summary>
	/// A simple implementation of the ICommand interface that allows you to define command logic in a 
	/// more straightforward way. It can be used for commands in the MVVM (Model-View-ViewModel) pattern
	/// to bind UI actions to ViewModel methods.
	/// </summary>
	public class RelayCommand : ICommand
	{
		private readonly Action<object> execute; // The method to call when the command is executed
		private readonly Predicate<object> canExecute; // The method to determine if the command can execute

		public event EventHandler CanExecuteChanged; // Event to signal that the canExecute state may have changed

		/// <summary>
		/// Initializes a new instance of the RelayCommand class.
		/// </summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		/// <param name="canExecute">The predicate to determine if the command can execute.</param>
		public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
			this.canExecute = canExecute;
		}

		/// <summary>
		/// Determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used to determine if the command can execute.</param>
		/// <returns>True if the command can execute; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			return canExecute == null || canExecute(parameter);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameter">Data used by the command.</param>
		public void Execute(object parameter)
		{
			execute(parameter);
		}

		/// <summary>
		/// Raises the CanExecuteChanged event to notify that the canExecute state has changed.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
