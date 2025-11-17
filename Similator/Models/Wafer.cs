// Simulator/Models/Wafer.cs
namespace Simulator.Models
{
    // Represents a wafer in the simulation.
    public class Wafer
    {
        // Unique identifier for the wafer.
        public int Id { get; set; }

        // Default constructor.
        public Wafer() { }

        // Constructor with identifier.
        public Wafer(int id)
        {
            Id = id;
        }

        // Override ToString for better readability.
        public override string ToString() => $"Wafer {Id}";
    }
}
