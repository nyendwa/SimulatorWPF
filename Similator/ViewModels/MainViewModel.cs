// Simulator/ViewModels/MainViewModel.cs
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
        public LoadPortViewModel LP1 { get; }
        public LoadPortViewModel LP2 { get; }
        public RobotArmViewModel Robot { get; }

        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        public RelayCommand StartCommand { get; }
        public RelayCommand PauseCommand { get; }

        private CancellationTokenSource? _cts;
        private int _scanIndex = 0;
        private int _delayMs = 600;

        public MainViewModel()
        {
            LP1 = new LoadPortViewModel("LP1");
            LP2 = new LoadPortViewModel("LP2");
            Robot = new RobotArmViewModel();

            // demo data
            for (int i = 0; i < 6; i++)
                LP1.PlaceWaferAt(i, new Wafer(i + 1));

            StartCommand = new RelayCommand(Start, () => _cts == null);
            PauseCommand = new RelayCommand(Pause, () => _cts != null);

            AddLog("Simulator ready.");
        }

        private void Start()
        {
            if (_cts != null) return;

            _cts = new CancellationTokenSource();
            StartCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();

            _ = Task.Run(() => RunLoopAsync(_cts.Token));
            AddLog("Simulation started.");
        }

        private void Pause()
        {
            if (_cts == null) return;

            _cts.Cancel();
            _cts = null;

            StartCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();
            AddLog("Simulation paused.");
        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int idx = LP1.FindNextOccupiedIndex(_scanIndex);
                    if (idx == -1)
                    {
                        AddLog("No wafers left in LP1.");
                        break;
                    }

                    var wafer = LP1.RemoveWaferAt(idx);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Robot.WaferOnArm = wafer;
                        AddLog($"Picked wafer {wafer?.Id} from LP1 slot {idx}");
                    });

                    await Task.Delay(_delayMs, token);

                    Application.Current.Dispatcher.Invoke(() => Robot.Position = "B");

                    await Task.Delay(_delayMs, token);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LP2.PlaceWaferAt(idx, wafer!);
                        Robot.WaferOnArm = null;
                        AddLog($"Placed wafer {wafer?.Id} into LP2 slot {idx}");
                    });

                    await Task.Delay(_delayMs, token);

                    Application.Current.Dispatcher.Invoke(() => Robot.Position = "A");

                    _scanIndex = (idx + 1) % LP1.Capacity;

                    await Task.Delay(_delayMs, token);
                }
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            finally
            {
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

        private void AddLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Log.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            });
        }
    }
}
