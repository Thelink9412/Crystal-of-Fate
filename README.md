# Crystal of Fate

> A strategic 2D Tower Defense game built with Unity and C#, showcasing robust game logic, dynamic systems, and an in-depth economy, all reflecting core principles of modern software development.

<img width="908" height="200" alt="Title Screenshot" src="https://github.com/user-attachments/assets/fca21650-270e-42b5-83a0-844882d832cd" />

---

## 📋 Overview

"Crystal of Fate" is a compelling **room-based Tower Defense** experience. Players strategically place defensive characters within various rooms to protect a central crystal from relentless waves of randomly generated enemies. While my primary focus is on **Front-end Development**, this project was designed to hone crucial programming skills such as **state management, object-oriented design, dynamic data handling, and robust economic systems**, which are directly transferable to building scalable and interactive web applications.

---

## 🚀 Live Demo

* **Link to the build:** https://thelink9412.itch.io/crystal-of-fate

---

## ✨ Gameplay Features

* **Strategic Room Defense:** Position unique defensive characters in various rooms to create an impenetrable perimeter around the Crystal.
* **Dynamic Enemy Spawns:** Face randomized enemy waves across three distinct rarity categories, each offering escalating challenges and rewards.
* **Progressive Difficulty:** Conquer 4 challenging levels, each divided into 4 intense rounds, totaling 16 unique engagements.
* **Gold Economy & Upgrades:** Earn Gold by defeating enemies (rarer enemies drop more!). Invest your earnings between rounds to strengthen your characters and prepare for tougher challenges.

![hippo](https://media1.giphy.com/media/v1.Y2lkPTc5MGI3NjExNzc4MDB1MTdjMHExZndtYzcxNTY1bHNoOXZsNWRjZDJqcnI5bzJybCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/1BIo1PgmFD1Ul6Kubs/giphy.gif)

---

## 🧠 Technical Deep Dive: Key Learnings for a Front-end Developer

Developing *Crystal of Fate* presented several complex architectural challenges, providing hands-on experience with principles vital for building modern web applications:

### 1. Robust State Management & Economy System
* **Challenge:** Implementing a comprehensive Gold Economy, player stats, and character upgrades that persist across rounds and levels while maintaining data integrity.
* **Solution:** Designed a centralized `GameManager` (utilizing the Singleton pattern) responsible for tracking player resources, character attributes, and overall game state. This manager ensures consistent data flow and updates across all game elements.
* **Front-end Parallel:** Directly analogous to managing global state in web applications using patterns like **Redux, React Context API, or Vuex**, where a single source of truth governs the application's data.

### 2. Component-Based Architecture & Modular Design
* **Challenge:** Creating versatile defensive characters and enemy units that are easily configurable, reusable, and maintain their own distinct behaviors.
* **Solution:** Leveraged Unity's GameObject-Component model to build modular characters. Each character's abilities (e.g., targeting, attacking, movement) are encapsulated within separate C# scripts, promoting high cohesion and loose coupling.
* **Front-end Parallel:** This mirrors the **component-based paradigm** in modern UI frameworks (React, Vue, Angular), where complex interfaces are broken down into small, independent, and reusable components.

### 3. Dynamic Data Generation & Randomization
* **Challenge:** Implementing a flexible enemy spawning system that dynamically generates waves, incorporating weighted randomness based on enemy rarity tiers.
* **Solution:** Developed a custom Spawning System that procedurally determines enemy types and quantities per wave, ensuring varied gameplay. Rarity tiers influence drop rates for Gold.
* **Front-end Parallel:** Relevant to building dynamic content feeds, randomized user experiences (e.g., A/B testing variations, content recommendations), or data visualization dashboards that adapt to incoming data.

### 4. Event-Driven Communication
* **Challenge:** Ensuring seamless communication between disparate game systems (e.g., enemy defeated -> UI updates score -> character levels up) without tight coupling.
* **Solution:** Utilized C# Events and Delegates to establish an **event-driven architecture**. For instance, when an enemy is defeated, an event is broadcast, allowing the UI, economy system, and character progress trackers to react independently.
* **Front-end Parallel:** Directly applicable to event handling in the DOM, custom events in JavaScript, or using libraries like **Pub/Sub** for decoupled communication between different parts of a web application.

---

## 🛠️ Tech Stack

* **Game Engine:** Unity 2022.x
* **Programming Language:** C#
* **Core Patterns:** Singleton, Observer Pattern (via C# Events/Delegates), State Pattern (for game states).

---

## 📂 Project Structure

```text
├── Assets
│   ├── _Scripts            <-- All core C# game logic (GameManager, Enemy AI, Character Abilities, UI Logic)
│   ├── Audio               <-- Sound effects and music
│   ├── Prefabs             <-- Reusable GameObjects (Enemies, Characters, UI elements)
│   ├── Scenes              <-- Game levels and menus
│   ├── Sprites             <-- 2D graphics and textures
│   └── UI                  <-- User Interface assets and layouts
├── ProjectSettings         <-- Unity's project configuration files
├── Packages                <-- Unity Package Manager definitions
├── .gitignore              <-- Specifies intentionally untracked files to ignore
├── README.md               <-- Project documentation
```
---

## 🔧 How to Run Locally

To explore the source code or play the game directly within the engine, follow these steps:

1.  **Clone the Repository:**
    Open your terminal and run:
    ```bash
    git clone https://github.com/Thelink9412/CrystalOfFate.git
    ```

2.  **Open in Unity Hub:**
    * Launch **Unity Hub**.
    * Click the **"Add"** button (or "Open" -> "Add project from disk").
    * Navigate to the cloned `CrystalOfFate` folder and select it.
    * Ensure you are using **Unity 2022.x** (or a compatible LTS version) to avoid library conflicts.

3.  **Open and Play:**
    * Once the project loads in the Unity Editor, go to the `Project` window.
    * Navigate to: **`Assets` > `Scenes` > `MainScene.unity`**.
    * Double-click the scene to load it.
    * Press the **Play** button at the top of the editor to start the game!

---

## 📬 Contact & Connect

I am a developer who loves bridging the gap between game logic and modern web interfaces. I'm always open to discussing game development, front-end architecture, or any interesting technical challenges!

* **LinkedIn:** [Luca Baudo](https://www.linkedin.com/in/luca-baudo-2728a7358)
* **GitHub:** [@Thelink9412](https://github.com/Thelink9412)
* **Email:** [lucabaudo9412@gmail.com](mailto:lucabaudo9412@gmail.com)

---
