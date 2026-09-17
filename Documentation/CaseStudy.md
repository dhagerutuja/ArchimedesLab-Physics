# Archimedes Lab — Case Study

## 1. Project Overview

**Archimedes Lab** is an interactive Physics learning module built in Unity to teach **Archimedes' Principle** through guided experimentation, visual explanations, prediction, free experimentation, and challenges.

The project adapts the learning-oriented structure of an interactive Biology AR/VR educational module into a Physics-focused experience.

## 2. Problem

Archimedes' Principle is often introduced through formulas and static diagrams. The goal of this project was to create a more interactive learning experience where a learner can:

- Observe floating and sinking behavior.
- Change the material of an object.
- Change the fluid.
- Interact directly with the experiment.
- Observe the forces and physics values involved.
- Make a prediction before performing an experiment.
- Explore the system freely.
- Complete a physics-based challenge.

## 3. Objective

Build a working Unity prototype that combines educational lesson progression, interactive 3D physics, dynamic UI, voice narration, camera presentation, prediction and verification, free experimentation, and challenge-based learning.

The focus was not only on simulating buoyancy, but on turning the simulation into a structured learning experience.

## 4. Learning Flow

**Introduction → Mission → Panel/Object Introduction → Guided Experiments → Principle → Formula → Prediction → Result → Free Experiment → Challenge**

### Introduction & Mission
The learner is introduced to the virtual laboratory and the learning objective.

### Guided Experiments
The learner observes a wooden object floating and a steel object sinking in water.

### Principle & Formula
The system explains buoyant force and introduces:

**Buoyant Force = Fluid Density × Gravity × Displaced Volume**

**Weight = Mass × Gravity**

### Prediction
The learner predicts the behavior of a plastic object in water before observing the actual result.

### Free Experiment
The learner can change materials and fluids and directly drag the object into the tank.

### Challenge
The learner must select a suitable material and fluid combination and successfully make the object float.

## 5. Interactive Physics

The simulation uses Unity Rigidbody physics to calculate buoyancy based on the physical properties of the object and fluid.

**Buoyant Force = ρ × g × Vᵈ**

Where:
- **ρ** = fluid density
- **g** = gravitational acceleration
- **Vᵈ** = displaced fluid volume

Object weight is calculated as:

**Weight = m × g**

The system evaluates the object's physical state rather than immediately treating temporary movement as a final result.

Possible evaluated states include:
- Floating
- Sinking
- Submerged
- Out of Fluid
- Settling

## 6. Materials & Fluids

The prototype uses data-driven material and fluid definitions.

### Materials

| Material | Density (kg/m³) |
|---|---:|
| Wood | 600 |
| Plastic | 950 |
| Steel | 7850 |
| Cork | 240 |

### Fluids

| Fluid | Density (kg/m³) |
|---|---:|
| Water | 1000 |
| Seawater | 1025 |
| Oil | 850 |
| Alcohol | 789 |

These values allow the learner to observe how changing density relationships affects floating and sinking.

## 7. System Architecture

```text
                    Lab Lesson
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
       Narration      Camera          UI
          │             │             │
          └─────────────┼─────────────┘
                        ▼
                    Experiment
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
      Materials       Fluids       Interaction
          │             │             │
          └─────────────┼─────────────┘
                        ▼
                Buoyancy Physics
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
     Force/Weight   Displacement   State Evaluation
                        │
                        ▼
                  Challenge System
```

## 8. Key Technical Components

### MaterialData
ScriptableObject-based data definitions for material name and density.

### FluidData
ScriptableObject-based data definitions for fluid name and density.

### MaterialController
Applies the selected material to the test object and updates its physical mass based on density and volume.

### WaterVolume
Handles fluid density and buoyancy interaction with the Rigidbody.

### BuoyancyPhysics
Provides the shared buoyancy calculation used by other systems.

### PhysicsStateEvaluator
Determines the stable physical state of the object before a result is presented.

### ObjectGrabber
Allows the learner to directly interact with and drag the test object.

### LabLessonController
Controls the educational lesson sequence and transitions between learning stages.

### LabCameraDirector
Controls camera presentation during experiment-focused stages.

### ChallengeManager
Handles challenge states, evaluation, success, failure, and retry behavior.

## 9. Design Decisions

### Prediction Before Experiment
The prediction stage was included so the learner has to reason about density before seeing the result.

### Stable-State Evaluation
The system does not immediately classify an object based on a single frame of movement. It waits for a stable physical state before evaluating floating or sinking behavior.

This prevents temporary movement from being incorrectly presented as the final result.

### Data-Driven Materials and Fluids
Materials and fluids are represented as separate data assets. This keeps physical properties independent from the core interaction logic and makes additional materials or fluids easier to add.

### Guided + Free Learning
The guided lesson establishes the concept first, while Free Experiment allows the learner to investigate the relationship independently.

## 10. Educational Interaction

The module combines:

- Visual 3D simulation
- Interactive object manipulation
- Dynamic UI
- Voice narration
- Camera transitions
- Prediction
- Immediate physical feedback
- Challenges

The intention is to connect the mathematical principle with an observable physical result.

## 11. AI-Assisted Development

AI tools were used throughout development for architecture exploration, implementation assistance, debugging, iteration, and documentation.

Tools used:
- ChatGPT
- Codex
- DeepSeek

AI-generated solutions were reviewed, integrated, modified, and tested within the Unity project.

## 12. Challenges Encountered

The development process involved issues around:

- Unity UI layout and dynamic positioning
- Runtime panel visibility
- World-space object targeting from UI
- Buoyancy and stable-state detection
- Material/fluid switching
- Physics interaction and object dragging
- Synchronizing narration with lesson stages

These were addressed through iterative implementation and testing within Unity.

## 13. Result

The final prototype provides a complete interactive learning flow covering:

- Guided introduction
- Physics demonstrations
- Archimedes' Principle explanation
- Formula explanation
- Prediction and verification
- Free experimentation
- Interactive material/fluid selection
- Real-time physics behavior
- Challenge-based evaluation
- Voice narration
- Dynamic visual presentation

## 14. Demo

See the `Demo/Videos/` directory for the project demonstration.

Screenshots are available in `Demo/Images/`.

## 15. Future Improvements

Possible future extensions include:

- AR/VR deployment
- Additional object shapes
- More advanced fluid behavior
- Real-time force visualization
- Additional lessons and challenges
- Student progress tracking
- More interactive measurement tools
- Expanded educational content
