# 🕵️‍♂️ LlamaNoir: Local AI Police Interrogation Game

A gritty **Noir-style** criminal investigation game developed in **C# (.NET 8)** using **MonoGame**. The game simulates a 1940s/1950s police interrogation room featuring procedural pixel-art suspect portraits, CRT biometric oscilloscopes, forensic tape recordings, and branching dialogue dynamically driven by a local LLM via Mozilla Llamafile.

---

## 🎮 Game Mechanics

* **Tactical Dialogue Tree**: Detective inquiry options dynamically categorized into strategic lines of questioning:
  * `[CONFRONTATION]`: Scrutinize alibis, cross-examine testimony, and challenge contradictions.
  * `[EMPATHY]`: Build rapport, offer protection, and ease psychological pressure.
  * `[PRESSURE]`: Intimidate and corner the suspect with sharp rhetoric (high stress risk).
  * `[REVELATION / FINAL]`: Expose the mastermind, extract a formal confession, or exonerate an innocent person.

* **Forensic Evidence Dossier with Synergy**:
  * Attach collected evidence cards from the left panel to reinforce your dialogue (`★ COMPELLING EVIDENCE`).
  * Unrelated evidence is recognized as an amateur bluff (`⚠ IRRELEVANT CLUE`), devastating trust and spiking stress.

* **Guilty vs. Innocent Suspects**:
  * Cases dynamically alternate between **guilty culprits** (objective: obtain a formal confession and locate the incriminating evidence) and **framed innocents** (objective: expose the setup, exonerate the suspect, and identify the real mastermind).
  * Blindly pressuring an innocent suspect to 100% stress triggers a **Mistrial due to Coerced False Confession**, ending the investigation in failure.

* **Psychological & Forensic Meters**:
  * **Suspect Trust (0 to 100%)**: If trust drops to 0%, the suspect invokes their right to remain completely silent.
  * **Psychological Stress (0 to 100%)**: At 100%, the suspect suffers an emotional breakdown or clam up entirely.
  * **Inquiry Progress (0 to 100%)**: Reach 100% before the clock runs out to crack the case.

* **10-Turn Time Limit**: The clock is ticking — by Turn 10, the defense attorney arrives to take custody of the suspect and terminate the interrogation.

---

## ⌨️ Controls & Shortcuts

* **[Keys 1, 2, 3, 4]** or **Mouse Click**: Select and execute a detective dialogue option.
* **Left-Click on Clue Cards**: Attach or detach forensic evidence to reinforce the next inquiry.
* **[F3]**: Toggle Conversation View (`[F3] Current Turn Focus` vs. `[F3] Full Scrollable Case Archive`).
* **[F5]**: Archive current case and generate a brand-new investigative inquiry.
* **Mouse Scroll Wheel**: Scroll up/down through the dialogue and observation history.

---

## 🚀 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your machine.

### Running the Game
Open a terminal in the project directory and run:
```bash
dotnet run
```

---

## 🧠 Local LLM Integration (Mozilla Llamafile)

The game is designed to seamlessly integrate with local large language models using [Mozilla Llamafile](https://github.com/Mozilla-Ocho/llamafile):

1. Follow the setup guide in [`llamafile/README.md`](./llamafile/README.md).
2. Place the executable (`llamafile.exe`) and your preferred GGUF weights (such as `Qwen 2.5 3B Instruct`) in the `llamafile/` folder.
3. Start the local server on `http://127.0.0.1:8080`.
4. Launch the game — it will automatically detect the server and generate enriched noir crime cases, suspect backstories, and reactive testimonies.

---

## 📄 License
Distributed under the MIT License. Feel free to explore, customize, and build upon it!
