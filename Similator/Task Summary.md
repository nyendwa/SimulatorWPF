

# **Project Summary — Wafer Handling HMI Simulator**

I developed a WPF-based HMI simulator that models a simplified wafer-handling system commonly found in semiconductor manufacturing. The system contains two load ports (LP1 and LP2), each holding up to 25 wafer slots, and a robot arm that transports wafers between the two cassettes. The goal of the project was to create a fully functional user interface that visualizes the state of the handling module and simulates wafer transfers using MVVM architecture.

To achieve this, I structured the application into Models, ViewModels, Views, and Services. I created modular UserControls for the load ports and the robot arm, implemented data binding to show wafer occupancy in each slot, and visualized whether the robot is currently holding a wafer. The robot’s movement between the two stations (A ↔ B) is represented through UI binding and simple animation logic.

The simulator performs a cyclic transfer process: when the user presses **Start**, the system picks the next available wafer from Load Port 1, assigns it to the robot, waits briefly to simulate motion, and then places the wafer into Load Port 2. The process repeats slot by slot until either the cassette is empty or the user presses **Pause**, which stops further transfers while keeping the current state intact.

Throughout development, I used the MVVM pattern strictly. All business logic — including transfer timing, wafer movement, and robot state updates — is implemented inside service classes and ViewModels, with no business logic in code-behind. The UI only handles presentation through XAML and bindings.


