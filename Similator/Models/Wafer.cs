// Simulator/Models/Wafer.cs
namespace Simulator.Models
{
    public class Wafer
    {
        public int Id { get; set; }

        public Wafer() { }

        public Wafer(int id)
        {
            Id = id;
        }

        public override string ToString() => $"Wafer {Id}";
    }
}
