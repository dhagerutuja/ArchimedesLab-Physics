# Archimedes Lab

> Interactive Physics learning module built in Unity to teach Archimedes' Principle through guided experiments, prediction, free experimentation, and challenges.

## Demo

**Demo Video:** [Watch the Archimedes Lab Interactive Physics Demo](https://drive.google.com/file/d/1DTpWetU3DmBkJ4k5Wnv0UWxRSgxBYNC4/view?usp=sharing)

## Screenshots

![Screenshot 1](Demo/Images/image1.png)

![Screenshot 2](Demo/Images/image2.png)

![Screenshot 3](Demo/Images/image3.png)

![Screenshot 4](Demo/Images/image4.png)

![Screenshot 5](Demo/Images/image5.png)

## Overview

Archimedes Lab transforms the teaching of buoyancy from static theory into an interactive learning experience.

Learners can observe floating and sinking, change materials and fluids, interact with the object, make predictions, and complete physics-based challenges.

## Learning Flow

```text
Introduction
    ↓
Mission
    ↓
Panel & Object Introduction
    ↓
Guided Experiments
    ↓
Archimedes' Principle
    ↓
Formula
    ↓
Prediction
    ↓
Result
    ↓
Free Experiment
    ↓
Challenge
```

## Key Features

- Guided Physics lesson
- Interactive 3D buoyancy simulation
- Multiple materials and fluids
- Real-time buoyancy calculations
- Interactive object dragging
- Physics values and force information
- Voice narration
- Dynamic UI and camera presentation
- Prediction → Experiment → Result learning loop
- Free Experiment mode
- Challenge and success/failure system
- Stable floating/sinking state evaluation

## Physics

The simulation is based on Archimedes' Principle:

```text
Buoyant Force = Fluid Density × Gravity × Displaced Volume

Weight = Mass × Gravity
```

Material and fluid properties are stored using Unity `ScriptableObject` data assets, allowing the experiment to be extended with additional materials and fluids.

## Technical Architecture

```text
Lesson Controller
       │
       ├── Narration
       ├── Camera
       └── UI
             │
             ▼
        Experiment
             │
     ┌───────┼────────┐
     ▼       ▼        ▼
 Materials Fluids  Interaction
     │       │        │
     └───────┼────────┘
             ▼
      Buoyancy Physics
             │
      ┌──────┼──────┐
      ▼      ▼      ▼
   Forces  Volume  State
                    Evaluation
                       │
                       ▼
                Challenge System
```

## Tech Stack

- Unity 6
- C#
- Unity Rigidbody Physics
- Unity UI
- TextMeshPro
- ScriptableObjects
- Unity Audio
- Git / GitHub

## AI-Assisted Development

AI tools were used for architecture exploration, implementation assistance, debugging, iteration, and documentation.

- ChatGPT
- Codex
- DeepSeek

AI-generated solutions were reviewed, integrated, modified, and tested within Unity.

## Documentation

- [Case Study](Documentation/CaseStudy.md)
- [Architecture](Documentation/Architecture.md)
- [Demo Videos](Demo/Videos/)
- [Screenshots](Demo/Images/)

## How to Run

1. Clone the repository.
2. Open the project in Unity 6.
3. Open the `MainLab` scene.
4. Press Play.

## Author

**Rutuja Dhage**

Generative AI Engineer | Python | RAG | Agentic AI | Backend Systems
