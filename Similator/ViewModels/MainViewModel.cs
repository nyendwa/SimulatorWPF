using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Simulator.Models;
using Simulator.Services;

namespace Simulator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Two cassettes (load ports)
        public LoadPortViewModel LP1 { get; }
        public LoadPortViewModel LP2 { get; }

        // Robot arm
        public RobotArmViewModel Robot { get; }

        // Log entries
        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        // Commands
        public RelayCommand StartCommand { get; }
        public RelayCommand PauseCommand { get; }

        // Cancellation token for simulation loop
        private CancellationTokenSource? _cts;

        // Circular scan index for LP1
        private int _scanIndex = 0;

        // Transfer speed controlled by slider
        private int _transferSpeedMs = 6000;
        public int TransferSpeedMs
        {
            get => _transferSpeedMs;
            set
            {
                if (_transferSpeedMs != value)
                {
                    _transferSpeedMs = value;
                    OnPropertyChanged(nameof(TransferSpeedMs));
                }
            }
        }

        /// <summary>
        /// Initializes the simulator:
        /// - Creates LP1/LP2
        /// - Creates robot
        /// - Loads some wafers into LP1
        /// - Sets up Start & Pause commands
        /// </summary>
        public MainViewModel()
        {
            LP1 = new LoadPortViewModel("LP1");
            LP2 = new LoadPortViewModel("LP2");
            Robot = new RobotArmViewModel();

            // Pre-load LP1 with wafers
            for (int i = 0; i < 6; i++)
                LP1.PlaceWaferAt(i, new Wafer(i + 1));

            StartCommand = new RelayCommand(Start, () => _cts == null);
            PauseCommand = new RelayCommand(Pause, () => _cts != null);

            AddLog("Simulator ready.");
        }

        /// <summary>
        /// Starts simulation loop
        /// </summary>
        private void Start()
        {
            if (_cts != null) return; // Already running

            _cts = new CancellationTokenSource();
            StartCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();

            _ = Task.Run(() => RunLoopAsync(_cts.Token));

            AddLog("Simulation started.");
        }

        /// <summary>
        /// Pauses simulation
        /// </summary>
        private void Pause()
        {
            if (_cts == null) return;

            _cts.Cancel();
            _cts = null;

            StartCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();

            AddLog("Simulation paused.");
        }

        /// <summary>
        /// Main simulation loop
        /// </summary>
        private async Task RunLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Step 1: find next wafer
                    int idx = LP1.FindNextOccupiedIndex(_scanIndex);
                    if (idx == -1)
                    {
                        AddLog("No wafers left in LP1.");
                        break;
                    }

                    // Step 2: pick wafer
                    var wafer = LP1.RemoveWaferAt(idx);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Robot.WaferOnArm = wafer;
                        AddLog($"Picked wafer {wafer?.Id} from LP1 slot {idx}");
                    });

                    await Task.Delay(TransferSpeedMs, token);

                    // Step 3: move robot to B
                    Application.Current.Dispatcher.Invoke(() => Robot.Position = "B");
                    await Task.Delay(TransferSpeedMs, token);

                    // Step 4: place into LP2
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LP2.PlaceWaferAt(idx, wafer!);
                        Robot.WaferOnArm = null;
                        AddLog($"Placed wafer {wafer?.Id} into LP2 slot {idx}");
                    });

                    await Task.Delay(TransferSpeedMs, token);

                    // Step 5: return to A
                    Application.Current.Dispatcher.Invoke(() => Robot.Position = "A");

                    // Advance scan index
                    _scanIndex = (idx + 1) % LP1.Capacity;

                    await Task.Delay(TransferSpeedMs, token);
                }
            }
            catch (OperationCanceledException)
            {
                // normal
            }
            finally
            {
                // Reset robot
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Robot.WaferOnArm = null;
                    Robot.Position = "A";
                });

                _cts = null;

                StartCommand.RaiseCanExecuteChanged();
                PauseCommand.RaiseCanExecuteChanged();

                AddLog("Simulation ended.");
            }
        }

        /// <summary>
        /// Adds a timestamped log entry
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
