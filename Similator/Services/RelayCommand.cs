using System;
using System.Windows.Input;

namespace Simulator.Services
{
    /// <summary>
    /// RelayCommand is a core MVVM pattern component.
    /// It allows the View to trigger actions in the ViewModel
    /// without the View needing to know any implementation details.
    /// 
    /// The View binds a Button's Command to this class,
    /// and the ViewModel provides the logic through delegates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Delegate executed when the command runs.
        /// Represents the action the ViewModel wants to perform.
        /// </summary>
        private readonly Action? _execute;

        /// <summary>
        /// Delegate that determines whether the command is allowed to run.
        /// Enables or disables buttons in the View dynamically.
        /// </summary>
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Creates a command that runs ViewModel logic via delegates.
        /// This is part of the MVVM pattern where commands expose
        /// user actions to the View without using code-behind.
        /// </summary>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Logic for whether the command is currently allowed.
        /// Used by WPF to enable/disable UI elements that bind to this command.
        /// </summary>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        /// <summary>
        /// Calls the ViewModel's action when the View triggers the command.
        /// </summary>
        public void Execute(object? parameter) => _execute?.Invoke();

        /// <summary>
        /// Event WPF listens to in order to update button enabled states
        /// when the result of CanExecute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Notifies the View that CanExecute may have changed.
        /// This keeps the UI in sync with ViewModel logic.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
