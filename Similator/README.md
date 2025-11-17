# **Wafer Handling HMI — Architecture (one page)**

## **Project purpose**

A small WPF (.NET Framework 4.8) HMI that simulates wafer transfers between two load ports (LP1, LP2) using a robot arm. The UI visualizes slot occupancy (up to 25 slots per cassette), robot holding state, and animates the robot transferring wafers between stations A ↔ B. Users can Start and Pause the simulation.

---

## **High-level design**

* **Pattern:** MVVM (strict). Views contain only presentation; all business logic (simulation, timing, logging) resides in services and view models.
* **Language / Target:** C#, .NET Framework 4.8, WPF.

---

## **Components**

### **1. Models**

* `Slot` — simple model for slot index and occupancy (`HasWafer`).
* `Cassette` — collection of `Slot` (25 slots).
* `RobotState` — robot wafer state (`HasWafer`) and position (`A`, `B`, or `Moving`).
* `TransferRecord` — timestamp, source port, and destination port of each transfer.

---

### **2. ViewModels**

* `MainViewModel` — root VM exposing `LoadPort1`, `LoadPort2`, `Robot`, `Logs`, `StartCommand`, `PauseCommand`.
* `LoadPortViewModel` — exposes slot occupancy via `ObservableCollection<SlotViewModel>` and slot selection logic.
* `RobotViewModel` — exposes robot wafer state and animation-relevant properties.
* All ViewModels implement `INotifyPropertyChanged` (or derive from `ObservableObject`).

---

### **3. Views**

* `Views/MainWindow.xaml` — main layout, binds to `MainViewModel`.
* `Views/LoadPortControl.xaml` — UserControl representing a cassette and its 25 slots.
* `Views/RobotControl.xaml` — UserControl that visualizes the robot and position changes.
* `Views/TransferLogControl.xaml` — optional display for transfer history.

---

### **4. Services**

* `TransferService` — main simulation engine: picks wafers, updates model state, rotates robot (A ↔ B), places wafers. Runs asynchronously using `Task` + `CancellationToken`.
* `LoggingService` — stores `TransferRecord` entries and exposes them to the UI.
* **(Not implemented due to time)** `SettingsService` — would provide configurable simulation parameters (e.g., transfer speed).
* **(Not implemented due to time)** `PersistenceService` — would save/restore wafer state via JSON or XML.

  * These features are part of the **next story** for future development.

---

## **Data flow**

* UI binds directly to ViewModel properties.
* `StartCommand` calls `TransferService.Start()` which begins the transfer loop and updates models.
* Models notify ViewModels via change events; Views automatically update via binding.
* `Pause` cancels the running simulation task, preserving all current state.

---

## **Key assumptions**

* Each cassette always contains 25 slots.
* Transfer policy: pick the next filled slot from the source cassette and place into the next available empty slot in the target cassette.
* Pause stops future operations but does not reset ongoing robot movement.
* UI animations are visual only and derived from ViewModel state changes.

---

## **Non-functional requirements**

* All code & comments written in English.
* No business logic is allowed in `*.xaml.cs` (strict MVVM).
* Clean folder separation: **Models**, **ViewModels**, **Views**, **Services**, **Converters**.

---

## **Manual test checklist**

* Start: robot transfers wafers sequentially from LP1 → LP2 until none remain.
* Pause: stops additional transfers immediately while keeping current model state.
* Visual: slot occupancy updates correctly and robot movement reflects state.
* Log: transfer events appear in the log panel (if logging is active).
* **Settings / Persistence:** not implemented (planned for next story).

