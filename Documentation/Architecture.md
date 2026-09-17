# Archimedes Lab — Architecture

## 1. High-Level Architecture

Archimedes Lab is organized into separate systems for lesson flow, presentation, interaction, physics, data, and challenge evaluation.

```text
                         Archimedes Lab
                               │
                    ┌──────────┴──────────┐
                    │   LabLessonController
                    │   Lesson State Flow
                    └──────────┬──────────┘
                               │
          ┌────────────────────┼────────────────────┐
          ▼                    ▼                    ▼
     Narration            Presentation              UI
     Audio Flow           Camera / Visuals      Dynamic Panels
          │                    │                    │
          └────────────────────┼────────────────────┘
                               ▼
                         Experiment Layer
                               │
             ┌─────────────────┼─────────────────┐
             ▼                 ▼                 ▼
       Material System    Fluid System      ObjectGrabber
             │                 │                 │
             └─────────────────┼─────────────────┘
                               ▼
                       BuoyancyPhysics
                               │
             ┌─────────────────┼─────────────────┐
             ▼                 ▼                 ▼
       Force / Weight    Displaced Volume   PhysicsStateEvaluator
                               │
                               ▼
                       ChallengeManager
```

---

## 2. Lesson Layer

### LabLessonController

The `LabLessonController` manages the educational sequence.

Main stages:

```text
Introduction
    ↓
DemonstrationWood
    ↓
DemonstrationSteel
    ↓
Explanation
    ↓
GuidedExperiment
    ↓
FreeExperiment
    ↓
Challenges
```

The lesson controller coordinates the educational progression without placing all physics calculations inside the lesson logic.

---

## 3. Data Layer

The physical properties of materials and fluids are stored separately from the behavior scripts.

### MaterialData

A Unity `ScriptableObject` containing:

- Material name
- Density

Current materials:

```text
Wood     600 kg/m³
Plastic  950 kg/m³
Steel    7850 kg/m³
Cork     240 kg/m³
```

### FluidData

A Unity `ScriptableObject` containing:

- Fluid name
- Density

Current fluids:

```text
Water     1000 kg/m³
Seawater  1025 kg/m³
Oil        850 kg/m³
Alcohol    789 kg/m³
```

This structure allows new materials and fluids to be added without rewriting the main physics system.

---

## 4. Physics Layer

### BuoyancyPhysics

`BuoyancyPhysics` provides the shared calculation used by the buoyancy system, UI, lesson logic, and challenge evaluation.

The calculation uses:

```text
Buoyant Force = Fluid Density × Gravity × Displaced Volume
```

The system calculates:

1. Object volume
2. Object weight
3. Whether the object is interacting with the fluid
4. Submerged ratio
5. Displaced volume
6. Buoyant force

A physics snapshot is produced so different systems can use the same calculated state.

---

## 5. Fluid Interaction

### WaterVolume

`WaterVolume` represents the fluid volume in the experiment.

It is responsible for:

- Current fluid selection
- Fluid density
- Fluid visual material
- Detecting objects inside the fluid
- Calculating the displaced volume
- Applying buoyant force
- Applying damping to the Rigidbody

Changing the selected fluid changes the density used by the buoyancy calculation.

---

## 6. Material Interaction

### MaterialController

`MaterialController` handles the selected object's material properties.

When a material is selected:

1. The current material is updated.
2. The displayed material information is updated.
3. Object volume is calculated.
4. Rigidbody mass is updated from:

```text
Mass = Material Density × Object Volume
```

The object can therefore retain the same geometry while its physical behavior changes according to material density.

---

## 7. Object Interaction

### ObjectGrabber

`ObjectGrabber` provides direct interaction with the test object.

The learner can:

1. Select the object.
2. Drag it through the scene.
3. Move it into the fluid.
4. Release it.
5. Observe the resulting physical behavior.

The object uses a Unity `Rigidbody` for physical simulation.

---

## 8. Physics State Evaluation

### PhysicsStateEvaluator

The system does not classify an object's state from a single moment of movement.

Instead, it evaluates whether the object has reached a stable condition.

Possible states include:

```text
Settling
Floating
Sinking
Submerged
OutOfFluid
```

The evaluator uses velocity and force relationships together with a stability period before presenting a final result.

This prevents temporary movement during the experiment from being immediately treated as a final floating or sinking result.

---

## 9. Presentation Layer

### LabCameraDirector

Controls camera movement during important learning moments.

The camera can transition between:

- Normal laboratory view
- Experiment-focused view

Camera movement is separated from the physics system so presentation changes do not directly control physical behavior.

### LabVisualPresentation

Provides runtime visual presentation for the laboratory environment, including:

- Tank visual dressing
- Fluid surface
- Lighting
- Fluid-specific visual appearance
- Reflection/presentation elements
- Animated fluid surface

These visual elements are presentation-focused and are separate from the core buoyancy calculation.

---

## 10. UI Layer

The main UI is hosted by the existing `PhysicsUI` Canvas.

Major UI areas include:

- Physics Panel
- Material / Fluid Panel
- Challenge Panel
- Current Material display
- Current Fluid display
- Lesson and result presentation

The UI changes according to the current lesson stage.

### World-Object Pointer

The manually created `ArrowRight.png` is reused as a UI pointer.

Because the Tank and TestObject exist in world space, their positions are projected into the UI/screen coordinate system so the same UI arrow can point toward the actual runtime objects.

The arrow can also be rotated 180° when the target requires a left-facing direction.

---

## 11. Challenge Layer

### ChallengeManager

The challenge system uses explicit states:

```text
NotStarted
Intro
Evaluating
Success
Failure
```

The challenge system evaluates the stable physical outcome rather than a temporary movement state.

Example challenge condition:

```text
StableFloating
```

A successful attempt occurs when the selected material/fluid combination produces a stable floating state.

A failed attempt occurs when the object reaches a stable sinking state.

The system also supports retry behavior.

---

## 12. Lesson-to-Physics Relationship

The lesson system does not need to manually animate the physical result.

Instead:

```text
Lesson Stage
     ↓
Configure Experiment
     ↓
Physics Simulation
     ↓
Stable State Evaluation
     ↓
Lesson Result
     ↓
Narration / Explanation
```

This keeps the educational presentation connected to the actual simulation result.

---

## 13. Example Runtime Flow

Example: Plastic + Water

```text
Select Plastic
      ↓
Material density = 950 kg/m³
      ↓
Select Water
      ↓
Fluid density = 1000 kg/m³
      ↓
Drag object into water
      ↓
Displaced volume changes with submersion
      ↓
Buoyant force is calculated
      ↓
Object reaches stable equilibrium
      ↓
State = Floating
```

The object can be mostly submerged while still floating because the stable condition depends on the balance between buoyant force and weight, not on a requirement that a specific fraction of the object remain above the surface.

---

## 14. Separation of Responsibilities

The architecture intentionally separates responsibilities:

| System | Responsibility |
|---|---|
| `LabLessonController` | Educational sequence |
| `MaterialData` | Material properties |
| `FluidData` | Fluid properties |
| `MaterialController` | Apply material physics |
| `WaterVolume` | Fluid interaction |
| `BuoyancyPhysics` | Shared buoyancy calculation |
| `PhysicsStateEvaluator` | Stable physical state |
| `ObjectGrabber` | User interaction |
| `LabCameraDirector` | Camera presentation |
| `LabVisualPresentation` | Visual presentation |
| `ChallengeManager` | Challenge evaluation |
| `PhysicsUI` | User interface |

This separation makes individual systems easier to modify without changing the complete application.

---

## 15. Design Principle

The architecture follows a simple principle:

**Physics determines the result; the lesson system explains the result.**

The educational layer presents and contextualizes what happens in the simulation rather than replacing the underlying physical behavior with scripted outcomes.
