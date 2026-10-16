# 🕵️‍♂️ LlamaNoir — Police Interrogation Room (1947)

[![Release](https://img.shields.io/badge/release-v0.1.0-blue.svg)](https://github.com/kelvynzucco/llama-noir/releases)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MonoGame](https://img.shields.io/badge/MonoGame-3.8.2-E11925?logo=monogame&logoColor=white)](https://monogame.net/)
[![Local AI](https://img.shields.io/badge/Local_AI-Qwen_2.5_3B_(Llamafile)-22c55e?logo=mozilla&logoColor=white)](https://github.com/Mozilla-Ocho/llamafile)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

**LlamaNoir** is a gritty, atmospheric criminal investigation and psychological interrogation game set in the golden era of *Film Noir* (late 1940s / 1950s), built in **C# (.NET 8)** using **MonoGame**.

You play as a homicide detective at Central Precinct, interrogating suspects whose personalities, alibis, bodily reactions, and dynamic dialogues are driven in real time by an embedded **local AI language model (Qwen 2.5 3B Instruct via Mozilla Llamafile)**.

**100% Private & Offline**: No data ever leaves your computer. The entire interrogation pipeline runs entirely on your local hardware.

---

## 🖥️ System Requirements

| Specification | Minimum | Recommended |
| :--- | :--- | :--- |
| **Operating System** | Windows 10 (64-bit) | Windows 11 (64-bit) |
| **Processor (CPU)** | Intel Core i3 / AMD Ryzen 3 (4 cores / 4 threads) | Intel Core i5 / AMD Ryzen 5 or higher (6+ cores) |
| **Memory (RAM)** | 8 GB RAM | 16 GB RAM |
| **Storage** | 4 GB free disk space | 4 GB free SSD space |
| **Graphics (GPU)** | Integrated graphics with Vulkan / DirectX 12 support | Dedicated GPU (NVIDIA GTX 1060 / AMD RX 580 or higher) with 4 GB+ VRAM |
| **Internet Connection** | Required **only** for the one-time initial AI model download (~2.1 GB) | Broadband connection for fast model download |

> **Performance Note**: If a compatible dedicated GPU is detected, LlamaNoir automatically offloads neural network layers to VRAM for near-instant responses. On machines with integrated graphics, the model runs smoothly via optimized multi-threaded CPU inference.

---

## ⚡ Quick Start: How to Download & Play (Pre-built Release)

No developer tools or manual server setups are required:

1. Go to the **[Releases](https://github.com/kelvynzucco/llama-noir/releases)** section.
2. Download **`LlamaNoir-v0.1.0-win-x64.zip`**.
3. Extract the `.zip` archive into any folder on your PC.
4. Double-click **`LlamaNoir.exe`**.
5. On the first launch, click **`[ 📥 DOWNLOAD AI AGENT AUTOMATICALLY ]`**. The game will stream the Mozilla Llamafile engine and the Qwen 2.5 3B model (~2.1 GB) directly into your local directory.
6. Once the progress bar reaches 100%, the police interrogation begins immediately!

---

## 🔍 Game Mechanics

### 🧩 Procedural Crime Cases & Suspects
* **Guilty Culprits vs. Framed Innocents**: Inquiries alternate dynamically between deceptive criminals (objective: expose alibi contradictions and extract a formal signed confession) and framed victims of an elaborate setup (objective: reveal the real mastermind and clear their name).
* **Pixel-Art Mugshots**: Procedurally assembled suspect portraits featuring 1940s fedora silhouettes, physical feature tags, and police height grids.
* **Biometric Polygraph Oscilloscope**: A real-time CRT polygraph monitor that traces heart rate (BPM), fluctuating with suspect stress, bluffs, and emotional breakdowns.

### 💼 Forensic Evidence Dossier (Single-Use Clues)
* **4 Pieces of Evidence per Case**: Ballistics reports, duplicate safe keys, forged bank slips, and forensic items gathered at the crime scene.
* **Secret Synergy Mechanics**: Pinning an evidence card to your dialogue choice permanently consumes it. Whether the clue is compelling or completely irrelevant is only revealed in the police ledger *after* the confrontation — irrelevant evidence will backfire, devastating trust and spiking suspect stress!

### 🎙️ Strategic Lines of Questioning
* `[EMPATHY]`: Build rapport, lower psychological stress, and foster cooperation.
* `[PRESSURE]`: Intimidate and corner the suspect (speeds up case progress, but risks emotional shutdown).
* `[CONFRONTATION]`: Scrutinize alibis and challenge timeline inconsistencies.
* `[REVELATION / FINAL]`: Deliver the decisive accusation to secure a confession or exonerate an innocent suspect.

### 📜 Interactive Timeline & Verbatim Archive
* **Historical Turn Rewind**: Click the turn pills (`[T1]`, `[T2]`, ...) on the timeline bar to inspect past questions and testimonies.
* **Forensic Tape Player**: Use the mouse scroll wheel or press `[F3]` to toggle between the current active turn and the full scrollable interrogation transcript.

---

## 🕹️ Controls & Shortcuts

| Key / Action | Description |
| :--- | :--- |
| **Mouse Click / Keys [1] to [4]** | Select and execute a detective inquiry option |
| **Click on Evidence Card** | Pin or unpin forensic evidence to confront the suspect |
| **Mouse Scroll Wheel** | Scroll up/down through the verbatim interrogation record |
| **[F3]** | Toggle between Current Turn View and Full Case Transcript |
| **[F5]** | Archive current case and open a brand-new procedural inquiry |
| **[Esc]** | Minimize the application window |

---

## 🛠️ Building from Source (For Developers)

To build and run LlamaNoir locally from source:

```bash
# Clone the repository
git clone https://github.com/kelvynzucco/llama-noir.git
cd llama-noir

# Build and run
dotnet run
```

---

## 🛡️ Visual Identity & Iconography
The game officially uses the vintage **Central Precinct Detective Badge** (`assets/Distintivo.ico` and `assets/Distintivo.png`) as the desktop icon, Windows taskbar icon, and in-game UI header.

---

## 📄 License
Distributed under the **MIT License**. Feel free to explore, customize, and build upon it!
