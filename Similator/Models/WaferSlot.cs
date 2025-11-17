// Simulator/Models/WaferSlot.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Simulator.Models
{
    public class WaferSlot : INotifyPropertyChanged
    {
        private Wafer? _occupant;

        /// <summary>
        /// 0-based index of the slot inside a cassette.
        /// </summary>
        public int SlotIndex { get; }

        /// <summary>
        /// The wafer currently in this slot (null = empty).
        /// </summary>
        public Wafer? Occupant
        {
            get => _occupant;
            set
            {
                if (_occupant != value)
                {
                    _occupant = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsOccupied));
                }
            }
        }

        public bool IsOccupied => Occupant != null;

        public WaferSlot(int index)
        {
            SlotIndex = index;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
