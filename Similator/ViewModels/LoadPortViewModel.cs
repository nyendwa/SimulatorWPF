// Simulator/ViewModels/LoadPortViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
using Simulator.Models;

namespace Simulator.ViewModels
{
    public class LoadPortViewModel
    {
        public string Name { get; }
        public ObservableCollection<WaferSlot> Slots { get; }

        public int Capacity => Slots.Count;

        public LoadPortViewModel(string name, int capacity = 25)
        {
            Name = name;
            Slots = new ObservableCollection<WaferSlot>();

            for (int i = 0; i < capacity; i++)
                Slots.Add(new WaferSlot(i));
        }

        public bool HasWaferAt(int index) =>
            index >= 0 && index < Capacity && Slots[index].IsOccupied;

        public Wafer? RemoveWaferAt(int index)
        {
            if (index < 0 || index >= Capacity) return null;
            var w = Slots[index].Occupant;
            Slots[index].Occupant = null;
            return w;
        }

        public bool PlaceWaferAt(int index, Wafer wafer)
        {
            if (index < 0 || index >= Capacity) return false;
            if (Slots[index].IsOccupied) return false;
            Slots[index].Occupant = wafer;
            return true;
        }

        public int FindNextOccupiedIndex(int startIndex)
        {
            int n = Capacity;
            for (int i = 0; i < n; i++)
            {
                int idx = (startIndex + i) % n;
                if (Slots[idx].IsOccupied) return idx;
            }
            return -1;
        }
    }
}
