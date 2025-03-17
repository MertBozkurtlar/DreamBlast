### Use Case 1: Launch Game and Display Main Menu

**Actor:** Player  
**Preconditions:** The game is installed and launched on the device.  
**Trigger:** The player starts the game.  
**Main Flow:**
- The game launches and loads the MainScene.
- The MainScene displays a background image and a LevelButton.
- The LevelButton shows the current level (or “finished” if all levels are complete).
- The game retrieves the last played level from local persistence.

**Postconditions:** The player sees the current level status and can proceed by tapping the LevelButton.

---

### Use Case 2: Start Level Gameplay

**Actor:** Player  
**Preconditions:** The player is at the MainScene and a valid level exists.  
**Trigger:** The player taps the LevelButton.  
**Main Flow:**
- The system loads the LevelScene corresponding to the current level.
- The LevelLoader reads the level JSON file and initializes level data (grid dimensions, move count, grid items).
- The BoardManager creates and displays the grid with cubes, rockets, obstacles, etc.
- The InputManager is activated to handle player interactions during gameplay.

**Postconditions:** The LevelScene is fully loaded and the game is ready for gameplay actions.

---

### Use Case 3: Handle Player Tap on Grid

**Actor:** Player  
**Preconditions:** The LevelScene is active and the grid is initialized.  
**Trigger:** The player taps a cube or grid cell.  
**Main Flow:**
- The InputManager translates the tap into grid coordinates.
- The system determines if the tapped item is part of a valid group (adjacent cubes of the same color).
- If valid, the system processes the move:
  - Blasts the group of cubes.
  - Checks if the group qualifies for a Rocket (group size ≥ 4).
  - Updates the grid by removing blasted cubes and triggering falling mechanics for the grid.
- The move count is decremented.

**Alternate Flow:**  
- If the tapped cell does not form a valid group, ignore the tap or provide feedback (e.g., “Invalid move”).

**Postconditions:** The grid updates accordingly, and the system evaluates win/loss conditions after the move.

---

### Use Case 4: Create and Manage Rocket Behavior

**Actor:** System  
**Preconditions:** A valid cube group that qualifies for a Rocket has been blasted.  
**Trigger:** The system identifies a blast with 4 or more cubes.  
**Main Flow:**
- The BoardManager initiates the Rocket creation process at the tapped cell.
- The system randomly assigns a direction (horizontal or vertical) to the Rocket.
- A Rocket object is instantiated and animated to form from moving cubes.
- The Rocket is incorporated into the grid and is set up to react to future taps or collisions.
- When a Rocket is tapped or interacts with another Rocket, it triggers its explosion sequence.
- Rockets can combine with adjacent Rockets to form a combo explosion (e.g., a 3x3 explosion area).

**Postconditions:** Rocket is created, and its subsequent interactions (explosions, damage propagation) are managed by the Rocket behavior logic.

---

### Use Case 5: Obstacle Interaction and Damage Handling

**Actor:** System  
**Preconditions:** Obstacles (box, stone, vase) are present in the level grid.  
**Trigger:** A blast or Rocket passes adjacent to or directly over an obstacle.  
**Main Flow:**
- The system determines which obstacles are affected based on proximity to the blast or Rocket path.
- Damage is applied to the obstacle:
  - **Box:** Takes one damage from any adjacent blast or Rocket pass.
  - **Stone:** Takes one damage only when a Rocket goes over it.
  - **Vase:** Takes one damage per blast (max one per group) and one per Rocket.
- Obstacles update their state (e.g., remaining hits) and, if cleared, are removed from the grid.
- For obstacles like vases that can fall, the falling mechanic is triggered after damage.

**Postconditions:** Obstacle states are updated, potentially clearing obstacles from the grid.

---

### Use Case 6: Win Level

**Actor:** System, Player  
**Preconditions:** The player has made moves and cleared obstacles as required by the level.
**Trigger:** All obstacles are cleared within the allowed move count.
**Main Flow:**
- The system detects that all level objectives have been met.
- A win sequence is triggered:
  - Celebration animations and particle effects are displayed.
  - A win popup or transition message is shown.
- The game updates the persisted level status (incrementing the level number or marking all levels as finished).

**Postconditions:** The win state is achieved, and the player is returned to the MainScene with updated level information.

---

### Use Case 7: Lose Level

**Actor:** System, Player  
**Preconditions:** The player has exhausted the allowed move count without clearing all obstacles.
**Trigger:** Move count reaches zero and level objectives are unmet.
**Main Flow:**
- The system identifies that the level has been lost.
- A fail popup is displayed with options:
  - **Try Again:** Restart the current level.
  - **Return to Main Menu:** Go back to the MainScene.
- The player's choice is processed accordingly:
  - Restarting reloads the current level.
  - Returning to MainScene displays the main menu.

**Postconditions:** The player is either given another attempt at the level or returned to the MainScene.

---

### Use Case 8: Persist Level Progress

**Actor:** System  
**Preconditions:** The game has been played, and progress has been made.  
**Trigger:** Level completion or explicit action from an editor tool.  
**Main Flow:**
- After completing a level, the system updates the last played level number.
- The PersistenceManager saves this data using local storage (PlayerPrefs or file-based storage).
- On game launch, the system reads the persisted data to set the current level in the MainScene.
- A Unity Editor menu item is available to manually update the last played level (for testing/debugging).

**Postconditions:** Player progress is stored and restored across game sessions.

---

### Use Case 9: Editor Tools for Level Management

**Actor:** Developer  
**Preconditions:** The project is opened in the Unity Editor.  
**Trigger:** The developer selects the custom menu item to set the last played level.
**Main Flow:**
- The editor script provides an interface (input field, button) to specify the desired last played level.
- The PersistenceManager is called to update the stored level value.
- The change is immediately reflected in the MainScene when the game is next launched or during testing.

**Postconditions:** Developers can easily adjust level progress for testing purposes.