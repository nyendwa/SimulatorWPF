// Simulator/ViewModels/MainViewModel.cs
//
// MainViewModel serves as the orchestrator of the entire wafer-handling simulator.
// It owns the two Load Ports (LP1, LP2), the RobotArmViewModel, and the simulation loop.
// The simulation transfers wafers from LP1 → LP2 using an asynchronous task and supports
// Start/Pause operations through commands.
//
// All UI updates are marshalled onto the Dispatcher thread to keep WPF thread-safe.
// No UI logic is placed here (strict MVVM). This class only exposes state and commands
// for binding to MainWindow.xaml.
//

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Simulator.Models;
using Simulator.Services;

namespace Simulator.ViewModels
{
    public class MainViewModel
    {
        // Two cassettes (load ports), each holding up to 25 wafer slots.
        public LoadPortViewModel LP1 { get; }
        public LoadPortViewModel LP2 { get; }

        // Robot arm state: position (“A”, “B”) and wafer held on the arm.
        public RobotArmViewModel Robot { get; }

        // Log entries shown in the UI (ListView or ItemsControl).
        // Most recent entry appears at top.
        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        // Commands bound to Start/Pause buttons in UI.
        public RelayCommand StartCommand { get; }
        public RelayCommand PauseCommand { get; }

        // Cancellation token to stop the async simulation loop.
        private CancellationTokenSource? _cts;

        // Index for scanning LP1 slots in a circular sequence.
        private int _scanIndex = 0;

        // Delay used between robot actions (simulated movement time).
        private int _delayMs = 600;

        /// <summary>
        /// Initializes the full simulator:
        /// - Creates both load ports
        /// - Creates the robot view model
        /// - Places a few initial wafers into LP1 for demonstration
        /// - Configures Start/Pause commands
        /// </summary>
        public MainViewModel()
        {
            LP1 = new LoadPortViewModel("LP1");
            LP2 = new LoadPortViewModel("LP2");
            Robot = new RobotArmViewModel();

            // Pre-load LP1 with some wafers for testing (slots 0–5).
            for (int i = 0; i < 6; i++)
                LP1.PlaceWaferAt(i, new Wafer(i + 1));

            // Start is only allowed when no simulation is running.
            StartCommand = new RelayCommand(Start, () => _cts == null);

            // Pause is only allowed when a simulation is running.
            PauseCommand = new RelayCommand(Pause, () => _cts != null);

            AddLog("Simulator ready.");
        }

        /// <summary>
        /// Starts the simulation loop.  
        /// Creates a new CancellationTokenSource, updates button states,
        /// and launches RunLoopAsync() on a background task.
        /// </summary>
        private void Start()
        {
            if (_cts != null) return; // Already running

            _cts = new CancellationTokenSource();

            StartCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();

            // Fire-and-forget simulation loop
            _ = Task.Run(() => RunLoopAsync(_cts.Token));

            AddLog("Simulation started.");
        }

        /// <summary>
        /// Requests the running simulation to stop by canceling the token.
        /// </summary>
        private void Pause()
        {
            if (_cts == null) return;

            _cts.Cancel();  // Signal cancellation
            _cts = null;    // Allow immediate restart

            StartCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();

            AddLog("Simulation paused.");
        }

        /// <summary>
        /// Main simulation loop:
        /// 1. Find next wafer in LP1
        /// 2. Robot picks wafer
        /// 3. Robot moves to position B
        /// 4. Robot places wafer into LP2
        /// 5. Robot returns to A
        ///
        /// Runs until LP1 is empty or Pause() is invoked.
        /// </summary>
        private async Task RunLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Step 1: Find next wafer in LP1 (circular search)
                    int idx = LP1.FindNextOccupiedIndex(_scanIndex);
                    if (idx == -1)
                    {
                        AddLog("No wafers left in LP1.");
                        break;
                    }

                    // Step 2: Remove wafer from LP1
                    var wafer = LP1.RemoveWaferAt(idx);

                    // Update robot state on UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Robot.WaferOnArm = wafer;
                        AddLog($"Picked wafer {wafer?.Id} from LP1 slot {idx}");
                    });

                    await Task.Delay(_delayMs, token);

                    // Step 3: Move robot to Load Port B
                    Application.Current.Dispatcher.Invoke(() => Robot.Position = "B");
                    await Task.Delay(_delayMs, token);

                    // Step 4: Place wafer into LP2
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LP2.PlaceWaferAt(idx, wafer!);
                        Robot.WaferOnArm = null;
                        AddLog($"Placed wafer {wafer?.Id} into LP2 slot {idx}");
                    });

                    await Task.Delay(_delayMs, token);

                    // Step 5: Return robot to A
                    Application.Current.Dispatcher.Invoke(() => Robot.Position = "A");

                    // Next scan index (circular)
                    _scanIndex = (idx + 1) % LP1.Capacity;

                    await Task.Delay(_delayMs, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal exit from cancellation
            }
            finally
            {
                // Reset robot state after exit
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Robot.WaferOnArm = null;
                    Robot.Position = "A";
                });

                // Clean up cancellation token and update buttons
                _cts = null;
                StartCommand.RaiseCanExecuteChanged();
                PauseCommand.RaiseCanExecuteChanged();

                AddLog("Simulation ended.");
            }
        }

        /// <summary>
        /// Inserts a new entry into the log with timestamp.
        /// Always executed on UI thread to keep ObservableCollection stable.
        /// </summary>
        private void AddLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Log.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            });
        }
    }
}
