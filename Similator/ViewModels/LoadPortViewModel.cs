// Simulator/ViewModels/LoadPortViewModel.cs
//
// ViewModel representing a single Load Port (Cassette) in the wafer handling system.
// Holds a collection of wafer slots and provides helper methods for inserting,
// removing, and locating wafers. Logic here supports the simulation engine
// but contains no UI code (pure MVVM).  
//
// Each Load Port contains a fixed number of slots (default 25), each able to hold
// one wafer. The View will bind to `Slots` to visualize occupancy.

using System.Collections.ObjectModel;
using Simulator.Models;

namespace Simulator.ViewModels
{
    public class LoadPortViewModel
    {
        // Display name for the port (e.g., "LP1", "LP2")
        public string Name { get; }

        // Collection of slot models bound to the UI.  
        // ObservableCollection ensures UI updates automatically when values change.
        public ObservableCollection<WaferSlot> Slots { get; }

        // Maximum number of wafers the cassette can hold.
        public int Capacity => Slots.Count;

        /// <summary>
        /// Creates a Load Port with a given name and capacity.
        /// Each slot is initialized as empty.
        /// </summary>
        public LoadPortViewModel(string name, int capacity = 25)
        {
            Name = name;
            Slots = new ObservableCollection<WaferSlot>();

            // Initialize cassette with empty slots
            for (int i = 0; i < capacity; i++)
                Slots.Add(new WaferSlot(i));
        }

        /// <summary>
        /// Checks if a particular slot is occupied by a wafer.
        /// Returns false if index is out of range.
        /// </summary>
        public bool HasWaferAt(int index) =>
            index >= 0 && index < Capacity && Slots[index].IsOccupied;

        /// <summary>
        /// Removes and returns the wafer at the specified index.
        /// If the slot is invalid or empty, returns null.
        /// </summary>
        public Wafer? RemoveWaferAt(int index)
        {
            if (index < 0 || index >= Capacity)
                return null;

            var wafer = Slots[index].Occupant; // Save current wafer
            Slots[index].Occupant = null;      // Empty the slot
            return wafer;
        }

        /// <summary>
        /// Attempts to place a wafer into a specific slot.
        /// Returns true if successful, false if the slot is occupied or invalid.
        /// </summary>
        public bool PlaceWaferAt(int index, Wafer wafer)
        {
            if (index < 0 || index >= Capacity)
                return false;

            if (Slots[index].IsOccupied)
                return false;

            Slots[index].Occupant = wafer;
            return true;
        }

        /// <summary>
        /// Finds the next slot that contains a wafer.
        /// Starts searching at the specified index and wraps around the cassette.
        /// Returns the index of the occupied slot, or -1 if none are found.
        /// </summary>
        public int FindNextOccupiedIndex(int startIndex)
        {
            int n = Capacity;

            // Circular search: start at startIndex, wrap using modulo.
            for (int i = 0; i < n; i++)
            {
                int idx = (startIndex + i) % n;
                if (Slots[idx].IsOccupied)
                    return idx;
            }

            return -1; // No wafers found
        }
    }
}
