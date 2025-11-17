


# Wafer Handling HMI — Architecture (one page)

## Project purpose
A small WPF (.NET Framework 4.8) HMI that simulates wafer transfers between two load ports (LP1, LP2) using a robot arm. The UI visualizes slot occupancy (up to 25 slots per cassette), robot holding state, and animates the robot transferring wafers between stations A ↔ B. Users can Start and Pause the simulation.

## High-level design
- **Pattern:** MVVM (strict). Views contain only presentation; all business logic (simulation, timing, persistence) resides in services and view models.
- **Language / Target:** C#, .NET Framework 4.8, WPF.

## Components
1. **Models**
   - `Slot` — simple model for slot index and occupancy (`HasWafer`).
   - `Cassette` — collection of `Slot` (25 slots).
   - `RobotState` — `HasWafer`, `Position` (A, B, Moving), optional `HoldingFromSlot`.
   - `TransferRecord` — timestamp, source slot, destination slot.

2. **ViewModels**
   - `MainViewModel` — root VM exposing `LoadPort1`, `LoadPort2`, `Robot`, `Logs`, `StartCommand`, `PauseCommand`.
   - `LoadPortViewModel` — exposes `ObservableCollection<SlotViewModel>` and helper methods (GetNextFilled, GetNextEmpty).
   - `RobotViewModel` — exposes robot properties for binding and animation triggers.
   - Each `*ViewModel` implements `INotifyPropertyChanged` (or derives from `ObservableObject`).

3. **Views**
   - `Views/MainWindow.xaml` — host layout, binds to `MainViewModel`.
   - `Views/LoadPortControl.xaml` — UserControl for a cassette (binds to a single LoadPortViewModel).
   - `Views/RobotControl.xaml` — UserControl that visually renders robot and exposes transform/animation bindings.
   - `Views/TransferLogControl.xaml` — optional ListView for logs.

4. **Services**
   - `ITransferService` / `TransferService` — simulation engine: pick wafer, simulate robot pick, rotate (180°), place; runs asynchronously using `Task` + `CancellationToken`.
   - `IPersistenceService` / `PersistenceService` — save/load JSON or XML of last state.
   - `ILoggerService` — appends `TransferRecord`s to an observable list.

5. **Helpers**
   - `RelayCommand` (ICommand implementation) or use CommunityToolkit MVVM.
   - `BoolToBrushConverter` for slot coloring.
   - Animation helpers or attached behaviors to trigger Storyboards from ViewModel changes.

## Data flow
- UI binds to ViewModel properties and commands.
- `StartCommand` triggers `TransferService.Start()` which runs the transfer loop and updates models.
- Models raise property change events; ViewModels forward/transform for UI; Views update automatically.
- `Pause` cancels the token to stop the loop; state remains saved in models.

## Key assumptions
- Each cassette has 25 slots (configurable).
- Transfer policy: pick the next filled slot (lowest index) and place into the next empty slot (lowest index) on the destination cassette.
- Pause cancels future transfers but preserves the current state; Start restarts a new simulation loop.
- UI animations are cosmetic and driven by ViewModel state (Robot position property).

## Non-functional requirements
- All code & comments in English.
- No business logic in XAML code-behind.
- Clean folder separation: Models, ViewModels, Views, Services, Converters.

## Manual test checklist
- Start: robot picks and moves wafers A→B until source empty.
- Pause: stops after current step and prevents further transfers.
- Visual: slots correctly reflect occupancy; robot visual shows wafer presence and movement.
- Persistence: saved state restores slot occupancy (if implemented).
