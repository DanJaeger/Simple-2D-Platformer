# 2D Platformer Core Architecture - Unity

This project provides a professional, scalable, and decoupled architecture for 2D platformer games in Unity. It implements advanced design patterns to ensure that movement systems, player statistics, and audio management are easy to maintain and extend.

![Gameplay Preview](./GitVisuals/Game.gif)
![Game View](./GitVisuals/Game.png)

## 🚀 Key Features

* **HFSM (Hierarchical Finite State Machine):** Full player control via hierarchical states. Enables smooth transitions between Jumping, Dashing, Falling, and Coyote Time.
* **MVC Architecture (Model-View-Controller):** Strict separation of concerns for Health and Stamina statistics management.
* **Cinematics with Sequencer:** Integration with **Unity Timeline** to create cinematic sequences triggered by smart, decoupled world triggers.
* **Addressables-based Audio System:** Asynchronous loading of music and sound effects to optimize RAM usage and prevent performance spikes.
* **Observer Pattern:** Extensive use of `System.Action` and `UnityEvents` for system-to-system communication without rigid dependencies.
* **Generic Singletons:** Robust implementation of persistent managers like the `AudioManager`.

## 🎬 Cinematics with Sequencer
The project includes a **Cinematic Trigger** system specifically designed to work with Unity's `PlayableDirector`.

* **Smart Triggers:** Layer-based detection (LayerMask) and one-time activation (`oneTimeOnly`) to fire events.
* **Decoupling:** Through `UnityEvents`, triggers can pause player control, switch Cinemachine cameras, or start Timeline sequences without direct script dependencies.
* **Control Flow:** The `PlayerStateMachine` allows for locking player input during sequences to ensure a seamless narrative experience.



## ⚙️ Technical Configuration

### Requirements
* **Unity 6 (6000.0.27f2)** or higher.
* **Addressables Package** (installable via Package Manager).
* **Timeline Package**.

### Installation
1.  Clone the repository.
2.  Open the project in Unity.
3.  Open Scene folder and select 'Demo'
4.  Configure your AudioClips in the Addressables group with their respective keys.
5.  Fine-tune the `CharacterStatsSO` assets to modify the control feel (physics, stamina costs, etc.).

## 🔊 Usage Example: Audio Manager
Thanks to the Singleton and Addressables patterns, you can change background music from anywhere in the code (even from a Timeline signal) safely:

```csharp
// Load and play music without freezing the main thread
AudioManager.Instance.PlayBackgroundMusic("Forest_Level_Theme");
```

## License
This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
