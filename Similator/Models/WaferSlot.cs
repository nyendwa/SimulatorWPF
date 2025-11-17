// Simulator/Models/WaferSlot.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Simulator.Models
{
    /// <summary>
    /// Represents a single slot inside a cassette or load port.
    /// A slot may contain a wafer or be empty.
    /// </summary>
    public class WaferSlot : INotifyPropertyChanged
    {
        /// Represents a slot that can hold a wafer.
        /// <summary>
        /// Backing field for the wafer currently inside this slot.
        /// Null means the slot is empty.
        /// </summary>
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
            // Get or set the wafer occupying this slot.
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
