# Unity Slot Machine Game

## 1. Game Overview

A 2D slot machine game developed in Unity 6.6 using C#. The project uses the assets provided in the assignment and focuses on randomized reel outcomes, smooth animations, clear UI, and straightforward gameplay.

**Objective:** Spin the three reels and win when all three display the same symbol.

## 2. Features

- Three-reel slot machine gameplay.
- Randomized symbol selection using Unity's random number generator.
- Animated reel spinning and sequential stopping.
- Win detection when all three symbols match.
- Virtual credits, betting, and payout calculations.
- UI for spinning, betting, balance, and game results.
- Responsive layout for browser-based gameplay.

*Note: Verify that each feature is implemented and working before claiming it in your final submission.*

## 3. Technologies Used

- **Engine:** Unity 6.6
- **Language:** C#
- **Platform:** WebGL
- **Version Control:** Git and GitHub
- **UI:** Unity UI system
- **Randomization:** Unity Random API

## 4. How to Run the Game

### Option A: Run in Unity Editor

1. Clone or download this repository.
2. Open the project using Unity Hub with a compatible Unity Editor version.
3. Open the main slot machine scene from `Assets/Scenes/`.
4. Press the Play button.
5. Use the on-screen controls to play.

### Option B: Run the WebGL Build

1. Open the project in Unity 6.6.
2. Open **File → Build Profiles**.
3. Select the Web - Desktop - Release profile.
4. Ensure the main game scene is included in the Scene List.
5. Build the project into `Build/WebGL/`.
6. Serve the exported build using a local web server or upload it to a compatible static web hosting service.
7. Open the hosted game URL in a supported browser.

**Important:** Do not open `index.html` directly using a `file://` URL. WebGL builds should be served over HTTP or HTTPS.

## 5. Game Rules and Payouts

- The player starts with a virtual credit balance.
- Each spin deducts the selected bet.
- The reels select symbols randomly.
- A win occurs when all three final symbols match.
- Winning spins award credits according to the configured payout.
- Invalid bets and spins during an active animation should be prevented.

Update this section if the final implementation uses different starting credits, betting rules, or payout values.

## 6. Technical Approach

### Object-Oriented Design

The game is organized into separate components so that reel animation, game logic, and UI responsibilities remain manageable.

### Random Number Generation

Unity's random number generator selects reel symbols. The final displayed symbols determine whether a spin wins.

### Reel Animation

The reels animate independently and stop sequentially to create a smooth slot machine experience. The result is evaluated after all reels have stopped.

### Win Detection and Payouts

The game compares the final symbol of each reel. If all three symbols match, the configured payout is awarded and the balance is updated.

### Code Quality

The project aims to use meaningful class names, modular scripts, appropriate comments, and clear folder organization.

## 7. Project Structure

```text
SlotMachine/
├── Assets/
│   ├── Arts/
│   │   └── ProvidedAssets/
│   ├── Animations/
│   ├── Prefabs/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Sounds/
│   └── UI/
├── Build/
│   └── WebGL/
├── Packages/
├── ProjectSettings/
├── .gitignore
└── README.md
```

The actual folders and files may vary depending on the final project implementation.

## 8. Bonus Features

No additional bonus features are claimed in this version. Any optional features will be documented here after implementation and testing.

## 9. Development Approach

The project is developed incrementally:

1. Inspect and integrate the supplied assets.
2. Build the slot machine interface.
3. Implement reel behavior and random symbol selection.
4. Add win detection, betting, and payout logic.
5. Test gameplay and handle invalid inputs.
6. Build and test the WebGL version.
7. Document the project and publish the source code on GitHub.

The priority is correctness, readable code, smooth gameplay, and a reliable browser build before adding optional features.

## 10. Assets and Credits

The slot machine artwork was supplied as part of the assignment. The assets are used for the game's interface and symbols.

All third-party assets, if any, should be credited according to their applicable licenses.

## 11. Author

Developed as part of a Unity game development assignment.
