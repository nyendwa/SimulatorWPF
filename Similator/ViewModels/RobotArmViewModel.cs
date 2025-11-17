// Simulator/ViewModels/RobotArmViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Simulator.Models;

namespace Simulator.ViewModels
{
    public class RobotArmViewModel : INotifyPropertyChanged
    {
        private Wafer? _waferOnArm;
        private string _position = "A"; // A = LP1, B = LP2

        public Wafer? WaferOnArm
        {
            get => _waferOnArm;
            set
            {
                if (_waferOnArm != value)
                {
                    _waferOnArm = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsHolding));
                }
            }
        }

        public bool IsHolding => WaferOnArm != null;

        public string Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
