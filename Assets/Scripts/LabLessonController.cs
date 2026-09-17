using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum LabLessonState { Introduction, DemonstrationWood, DemonstrationSteel, Explanation, GuidedExperiment, FreeExperiment, Challenges }

/// <summary>Small, reusable lesson state machine for MainLab's first learning flow.</summary>
public class LabLessonController : MonoBehaviour
{
    public LabLessonState State { get; private set; }

    private WaterVolume water;
    private MaterialController materials;
    private Rigidbody body;
    private Collider sampleCollider;
    private ObjectGrabber grabber;
    private ChallengeManager challenges;
    private PhysicsStateEvaluator evaluator;
    private GameObject materialPanel;
    private CanvasGroup overlayGroup;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private Button continueButton;
    private Button skipButton;
    private Button challengeButton;
    private bool advanceRequested;
    private bool skipRequested;
    private bool experimentReachedExpectedState;
    private LabCameraDirector cameraDirector;
    private FreeExperimentObjectPresenter freeExperimentPresenter;
    private AudioManager audioManager;
    private GameObject physicsPanel;
    private WorkspaceHighlight workspaceHighlight;
    private ForceVisualizer forceVisualizer;
    private RectTransform lessonPanelRect;
    private Image lessonPanelImage;
    private Image lessonHeaderImage;
    private TMP_Text dragHintText;
    private Outline lessonPanelOutline;
    private RectTransform physicsPanelRect;
    private RectTransform materialPanelRect;
    private RectTransform introductionArrowRect;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForMainLab()
    {
        if (SceneManager.GetActiveScene().name == "MainLab" && FindAnyObjectByType<LabLessonController>() == null)
            new GameObject("LabLessonController").AddComponent<LabLessonController>();
    }

    private void Awake()
    {
        water = FindAnyObjectByType<WaterVolume>();
        materials = FindAnyObjectByType<MaterialController>();
        grabber = FindAnyObjectByType<ObjectGrabber>();
        challenges = FindAnyObjectByType<ChallengeManager>();
        audioManager = FindAnyObjectByType<AudioManager>();
        physicsPanel = GameObject.Find("PhysicsPanel");
        physicsPanelRect = physicsPanel != null ? physicsPanel.GetComponent<RectTransform>() : null;
        forceVisualizer = FindAnyObjectByType<ForceVisualizer>();
        if (materials != null)
        {
            body = materials.rb;
            sampleCollider = materials.objectCollider;
        }
        materialPanel = GameObject.Find("MaterialPanel");
        materialPanelRect = materialPanel != null ? materialPanel.GetComponent<RectTransform>() : null;
        GameObject physicsUI = GameObject.Find("PhysicsUI");
        introductionArrowRect = physicsUI != null ? physicsUI.transform.Find("ArrowRight") as RectTransform : null;
        if (introductionArrowRect != null)
        {
            Image arrowImage = introductionArrowRect.GetComponent<Image>();
            if (arrowImage != null) arrowImage.raycastTarget = false;
            introductionArrowRect.gameObject.SetActive(false);
        }
        evaluator = gameObject.AddComponent<PhysicsStateEvaluator>();
        cameraDirector = gameObject.AddComponent<LabCameraDirector>();
        freeExperimentPresenter = body != null ? body.GetComponent<FreeExperimentObjectPresenter>() : null;
        if (freeExperimentPresenter == null && body != null) freeExperimentPresenter = body.gameObject.AddComponent<FreeExperimentObjectPresenter>();
        HoldSampleAtExperimentSpawn();
        BuildOverlay();
        workspaceHighlight = gameObject.AddComponent<WorkspaceHighlight>();
        SetPhysicsPanel(false);
        SetOpeningWorkspaceVisible(false);
        cameraDirector.FocusExplanation();
        if (challenges != null) challenges.ChallengeCompleted += PlayChallengeResultNarration;
    }

    private void OnDestroy()
    {
        if (challenges != null) challenges.ChallengeCompleted -= PlayChallengeResultNarration;
    }

    private IEnumerator Start()
    {
        yield return null; // let existing Start methods cache their inspector references.
        SetExperimentControls(false);
        yield return RunLesson();
    }

    private void FixedUpdate()
    {
        if (State == LabLessonState.DemonstrationWood || State == LabLessonState.DemonstrationSteel || State == LabLessonState.GuidedExperiment)
            evaluator.Evaluate(water.GetPhysicsSnapshot(body, sampleCollider), body);
    }

    private IEnumerator RunLesson()
    {
        State = LabLessonState.Introduction;
        yield return ShowFor("ARCHIMEDES LAB", "EXPLORE ARCHIMEDES'\nPRINCIPLE THROUGH\nINTERACTIVE BUOYANCY\nEXPERIMENTS.", 8f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }
        yield return ShowFor("YOUR MISSION", "INVESTIGATE WHY OBJECTS\nFLOAT OR SINK.\nCHANGE MATERIALS AND FLUIDS\nTO EXPLORE WHY.", 9f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        yield return ShowWorkspaceIntroduction();
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        State = LabLessonState.DemonstrationWood;
        PreparePrediction(materials.wood, water.water);
        yield return ShowFor("EXPERIMENT 01", "Experiment 1: Wooden Block\nObserve how it moves and settles in water.", 8f);
        yield return RunExperiment(materials.wood, water.water, PhysicalState.Floating, "Wood");
        cameraDirector.ReturnToLab(); SetPhysicsPanel(false); yield return new WaitForSeconds(1.2f);
        yield return ShowFor(
            experimentReachedExpectedState ? "WOOD FLOATS" : "EXPERIMENT NEEDS ATTENTION",
            experimentReachedExpectedState ? "Result: The wooden block floats.\nBuoyant force balances its weight." : "Wood did not reach a stable floating state within the safety timeout. The issue has been logged for debugging; no result is being claimed.", 9f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        State = LabLessonState.DemonstrationSteel;
        PreparePrediction(materials.steel, water.water);
        yield return ShowFor("EXPERIMENT 02", "Experiment 2: Steel Block\nObserve what happens when it enters water.", 8f);
        yield return RunExperiment(materials.steel, water.water, PhysicalState.Sinking, "Steel");
        cameraDirector.ReturnToLab(); SetPhysicsPanel(false); yield return new WaitForSeconds(1.2f);
        yield return ShowFor(
            experimentReachedExpectedState ? "STEEL SINKS" : "EXPERIMENT NEEDS ATTENTION",
            experimentReachedExpectedState ? "Result: The steel block sinks.\nIts weight exceeds the available buoyant force." : "Steel did not reach a stable sinking state within the safety timeout. The issue has been logged for debugging; no result is being claimed.", 9f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        State = LabLessonState.Explanation;
        yield return ShowFor("ARCHIMEDES' PRINCIPLE", "When an object is placed in a fluid, the fluid pushes upward on the object. This upward push is called buoyant force. The amount of buoyant force depends on how much fluid the object displaces and how dense that fluid is. This is the basic idea behind Archimedes' Principle.", 11f);
        yield return ShowFor("THE FORMULA", "DISPLACED FLUID  ↓  BUOYANT FORCE ↑\n\nBuoyant Force = Fluid Density × Gravity × Displaced Volume\nWeight = Mass × Gravity", 8f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        State = LabLessonState.GuidedExperiment;
        PreparePrediction(materials.plastic, water.water);
        yield return ShowFor("YOUR PREDICTION", "Plastic density is 950 kg/m³.\nWater density is 1000 kg/m³.\n\nWill plastic float or sink?\n\nContinue to test your prediction.", 11f);
        yield return RunExperiment(materials.plastic, water.water, PhysicalState.Floating, "Plastic");
        cameraDirector.ReturnToLab(); SetPhysicsPanel(false); yield return new WaitForSeconds(1.2f);
        yield return ShowFor(
            experimentReachedExpectedState ? "ANSWER: PLASTIC FLOATS" : "EXPERIMENT NEEDS ATTENTION",
            experimentReachedExpectedState ? ValuesText("It is mostly submerged, yet it floats when buoyant force balances weight. Floating does not mean half-submerged.") : "Plastic did not reach a stable floating state within the safety timeout. The issue has been logged for debugging; no result is being claimed.", 9f);
        EnterFreeExperiment();
    }

    private IEnumerator ShowFor(string title, string message, float minimumSeconds = 8f)
    {
        bool openingInstruction = title == "ARCHIMEDES LAB" || title == "YOUR MISSION";
        ConfigureLessonPanel(openingInstruction);
        if (openingInstruction) SetOpeningWorkspaceVisible(false);
        ShowOverlay(title, message, true, true, false);
        advanceRequested = false;
        // Reading and narration never gate progress.  This is deliberately true for every
        // instruction/result state so testers can advance immediately.
        continueButton.interactable = true;
        audioManager?.PlayNarration(GetNarrationCue(title, message));
        while (!skipRequested && !advanceRequested)
        {
            yield return null;
        }
    }

    private IEnumerator RunExperiment(MaterialData material, FluidData fluid, PhysicalState expected, string experimentName)
    {
        HideOverlay();
        SetPhysicsPanel(true);
        SetForceVisualization(true);
        cameraDirector.FocusExperiment();
        yield return new WaitForSeconds(1.15f);
        SetExperiment(material, fluid);
        yield return WaitForExpectedState(expected, experimentName, 12f);
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator ShowWorkspaceIntroduction()
    {
        cameraDirector.RestoreLabImmediately();
        SetOpeningWorkspaceVisible(true);
        HideOverlay();

        yield return ShowWorkspaceCue(AudioManager.NarrationCue.PhysicsPanelIntroduction, () => workspaceHighlight.ShowUI(physicsPanel), physicsPanelRect);
        yield return ShowWorkspaceCue(AudioManager.NarrationCue.MaterialFluidPanelIntroduction, () => workspaceHighlight.ShowUI(materialPanel), materialPanelRect);
        Renderer tankRenderer = water != null ? water.GetComponent<Renderer>() : null;
        Renderer testObjectRenderer = body != null ? body.GetComponent<Renderer>() : null;
        yield return ShowWorkspaceCue(AudioManager.NarrationCue.TankIntroduction, () => workspaceHighlight.ShowWorld(tankRenderer, WorkspaceHighlight.WorldFocus.Tank), null, tankRenderer);
        yield return ShowWorkspaceCue(AudioManager.NarrationCue.TestObjectIntroduction, () => workspaceHighlight.ShowWorld(testObjectRenderer, WorkspaceHighlight.WorldFocus.TestObject), null, testObjectRenderer);

        workspaceHighlight.Clear();
        SetOpeningWorkspaceVisible(false);
        cameraDirector.FocusExplanation();
    }

    private IEnumerator ShowWorkspaceCue(AudioManager.NarrationCue cue, System.Action showHighlight, RectTransform panelArrowTarget = null, Renderer worldArrowTarget = null)
    {
        workspaceHighlight.Clear();
        showHighlight?.Invoke();
        if (panelArrowTarget != null) ShowIntroductionArrow(panelArrowTarget);
        else ShowIntroductionArrow(worldArrowTarget);
        bool narrationStarted = audioManager != null && audioManager.PlayNarration(cue);
        while (!skipRequested && narrationStarted && audioManager.IsNarrationPlaying)
        {
            if (panelArrowTarget != null) PositionIntroductionArrow(panelArrowTarget);
            else PositionIntroductionArrow(worldArrowTarget);
            yield return null;
        }
        HideIntroductionArrow();
        workspaceHighlight.Clear();
    }

    private void ShowIntroductionArrow(RectTransform target)
    {
        if (introductionArrowRect == null || target == null) return;
        introductionArrowRect.SetAsLastSibling();
        introductionArrowRect.localRotation = Quaternion.Euler(0f, 0f, 180f);
        introductionArrowRect.gameObject.SetActive(true);
        PositionIntroductionArrow(target);
    }

    private void PositionIntroductionArrow(RectTransform target)
    {
        if (introductionArrowRect == null || target == null || !introductionArrowRect.gameObject.activeSelf) return;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 panelRightCenter = (corners[2] + corners[3]) * .5f;
        float arrowHalfWidth = introductionArrowRect.rect.width * introductionArrowRect.lossyScale.x * .5f;
        float gap = 28f * introductionArrowRect.lossyScale.x;
        introductionArrowRect.position = panelRightCenter + Vector3.right * (arrowHalfWidth + gap);
    }

    private void ShowIntroductionArrow(Renderer target)
    {
        if (introductionArrowRect == null || target == null) return;
        introductionArrowRect.SetAsLastSibling();
        introductionArrowRect.gameObject.SetActive(true);
        PositionIntroductionArrow(target);
    }

    private void PositionIntroductionArrow(Renderer target)
    {
        if (introductionArrowRect == null || target == null || !introductionArrowRect.gameObject.activeSelf) return;
        Camera mainCamera = Camera.main;
        RectTransform canvasRect = introductionArrowRect.parent as RectTransform;
        if (mainCamera == null || canvasRect == null) return;

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(target.bounds.center);
        if (screenPoint.z <= 0f) { introductionArrowRect.gameObject.SetActive(false); return; }
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 targetPosition)) return;

        Vector2 canvasCenter = canvasRect.rect.center;
        Vector2 awayFromCenter = (targetPosition - canvasCenter).normalized;
        if (awayFromCenter.sqrMagnitude < .001f) awayFromCenter = new Vector2(.7f, .7f).normalized;
        float gap = 28f;
        Vector2 arrowPosition = targetPosition + awayFromCenter * (introductionArrowRect.rect.width * .5f + gap);
        introductionArrowRect.anchoredPosition = arrowPosition;

        Vector2 directionToTarget = targetPosition - introductionArrowRect.anchoredPosition;
        introductionArrowRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg);
    }

    private void HideIntroductionArrow()
    {
        if (introductionArrowRect != null)
            introductionArrowRect.gameObject.SetActive(false);
    }

    private IEnumerator WaitForExpectedState(PhysicalState expectedState, string experimentName, float timeout)
    {
        experimentReachedExpectedState = false;
        float elapsed = 0f;
        while (elapsed < timeout && !skipRequested)
        {
            if (evaluator.IsStable && evaluator.CurrentState == expectedState)
            {
                experimentReachedExpectedState = true;
                yield break;
            }
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (!skipRequested)
            Debug.LogError($"{experimentName} demonstration failed to reach stable {expectedState} state within {timeout:F1} seconds.", this);
    }

    private void SetExperiment(MaterialData material, FluidData fluid)
    {
        challenges?.ResetFeedback();
        water.ApplyFluid(fluid);
        materials.ApplyMaterial(material);
        body.isKinematic = false;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.rotation = Quaternion.identity;
        Physics.SyncTransforms();
        body.position = GetExperimentSpawnPosition();
        Physics.SyncTransforms();
        body.WakeUp();
        evaluator.ResetState();
    }

    private void HoldSampleAtExperimentSpawn()
    {
        if (body == null || water == null || sampleCollider == null)
            return;

        body.isKinematic = false;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        body.rotation = Quaternion.identity;
        Physics.SyncTransforms();
        body.position = GetExperimentSpawnPosition();
        Physics.SyncTransforms();
    }

    private void PreparePrediction(MaterialData material, FluidData fluid)
    {
        challenges?.ResetFeedback();
        body.isKinematic = false;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        water.ApplyFluid(fluid);
        materials.ApplyMaterial(material);
        evaluator.ResetState();
    }

    private Vector3 GetExperimentSpawnPosition()
    {
        Collider fluidCollider = water.GetComponent<Collider>();
        if (fluidCollider == null || sampleCollider == null)
        {
            Debug.LogError("Cannot reset the experiment: water or sample collider is missing.", this);
            return body.position;
        }

        Bounds fluidBounds = fluidCollider.bounds;
        Vector3 halfExtents = sampleCollider.bounds.extents;
        const float horizontalMargin = 0.1f;
        float minimumX = fluidBounds.min.x + halfExtents.x + horizontalMargin;
        float maximumX = fluidBounds.max.x - halfExtents.x - horizontalMargin;
        float minimumZ = fluidBounds.min.z + halfExtents.z + horizontalMargin;
        float maximumZ = fluidBounds.max.z - halfExtents.z - horizontalMargin;

        float x = Mathf.Clamp(fluidBounds.center.x, minimumX, maximumX);
        float z = Mathf.Clamp(fluidBounds.center.z, minimumZ, maximumZ);
        float verticalClearance = Mathf.Max(1f, halfExtents.y * 2f);
        float y = fluidBounds.max.y + halfExtents.y + verticalClearance;
        return new Vector3(x, y, z);
    }

    private string ValuesText(string conclusion)
    {
        BuoyancyPhysics.Snapshot values = water.GetPhysicsSnapshot(body, sampleCollider);
        float density = values.Volume > 0f ? body.mass / values.Volume : 0f;
        return $"Object density: {density:F0} kg/m³\nFluid density: {water.fluidDensity:F0} kg/m³\nBuoyant force: {values.BuoyantForce:F0} N\nWeight: {values.Weight:F0} N\nDisplaced volume: {values.DisplacedVolume:F2} m³\n\n{conclusion}";
    }

    private void EnterFreeExperiment()
    {
        State = LabLessonState.FreeExperiment;
        cameraDirector.RestoreLabImmediately();
        SetOpeningWorkspaceVisible(true);
        SetExperimentControls(true);
        freeExperimentPresenter?.BeginFreeExperiment();
        grabber?.EnsureCamera();
        ShowOverlay("EXPERIMENT FREELY", "Drag the object from the lab floor into the fluid to begin the experiment. Change the material and fluid to see how density affects floating and sinking.", false, false, true);
        challengeButton.interactable = true;
        audioManager?.PlayNarration(AudioManager.NarrationCue.FreeExperimentIntroduction);
    }

    private void BeginChallenges()
    {
        State = LabLessonState.Challenges;
        SetSelectionStatusVisible(true);
        SetPhysicsPanel(true);
        SetForceVisualization(true);
        HideOverlay();
        freeExperimentPresenter?.BeginFreeExperiment();
        ChallengeData firstChallenge = ScriptableObject.CreateInstance<ChallengeData>();
        firstChallenge.challengeTitle = "Challenge 1 — Make the Object Float";
        firstChallenge.description = "Choose a material and fluid that will allow the object to float. Then drag the object into the fluid.";
        firstChallenge.targetMaterial = null;
        firstChallenge.targetFluid = null;
        firstChallenge.successCondition = ChallengeSuccessCondition.StableFloating;
        firstChallenge.educationalExplanation = "The object is floating because its density is lower than the fluid density, allowing buoyant force to balance its weight.";
        challenges.BeginChallenge(firstChallenge);
        audioManager?.PlayNarration(AudioManager.NarrationCue.ChallengeIntroduction);
    }

    private void PlayChallengeResultNarration(bool succeeded)
    {
        audioManager?.PlayNarration(succeeded ? AudioManager.NarrationCue.ChallengeSuccess : AudioManager.NarrationCue.ChallengeFailure);
    }

    private void RequestAdvance()
    {
        // Advancing any lesson panel must not leave its narration playing underneath the next state.
        audioManager?.StopNarration();
        advanceRequested = true;
    }

    private void SetOpeningWorkspaceVisible(bool visible)
    {
        SetPhysicsPanel(visible);
        SetForceVisualization(visible);
        if (materialPanel != null) materialPanel.SetActive(visible);
        SetSelectionStatusVisible(visible);
        if (!visible && challenges != null && challenges.challengeStatus != null)
            challenges.challengeStatus.transform.parent.gameObject.SetActive(false);
    }

    private static void SetSelectionStatusVisible(bool visible)
    {
        GameObject selectionStatus = GameObject.Find("Current Selection Status");
        if (selectionStatus != null && selectionStatus.activeSelf != visible)
            selectionStatus.SetActive(visible);
    }

    private void ConfigureLessonPanel(bool openingInstruction)
    {
        if (lessonPanelRect == null) return;
        lessonPanelRect.anchoredPosition = Vector2.zero;
        lessonPanelRect.sizeDelta = openingInstruction ? new Vector2(1360f, 760f) : new Vector2(1120f, 620f);
        // The opening panels intentionally read as dark glass, leaving an atmospheric lab visible behind them.
        ApplyLessonPanelStyle(openingInstruction ? .56f : .60f);
        lessonHeaderImage.color = new Color(0f, 0f, 0f, .58f);
        lessonPanelOutline.enabled = true;
        if (dragHintText != null) dragHintText.gameObject.SetActive(!openingInstruction);
        bodyText.fontSize = openingInstruction ? 40f : 31f;
        bodyText.fontStyle = openingInstruction ? FontStyles.Bold : FontStyles.Normal;
        bodyText.outlineWidth = openingInstruction ? .16f : 0f;
        bodyText.outlineColor = new Color(.005f, .015f, .03f, 1f);
        bodyText.lineSpacing = openingInstruction ? 11f : 7f;
        bodyText.rectTransform.sizeDelta = openingInstruction ? new Vector2(1160f, 440f) : new Vector2(930f, 365f);
        bodyText.rectTransform.anchoredPosition = openingInstruction ? new Vector2(0f, 28f) : new Vector2(0f, 14f);
    }

    private void SetPhysicsPanel(bool visible)
    {
        if (physicsPanel != null && physicsPanel.activeSelf != visible)
            physicsPanel.SetActive(visible);
    }

    private void ApplyLessonPanelStyle(float alpha)
    {
        if (lessonPanelImage == null) return;
        // This is the dynamically-created Image on LessonOverlay/Panel, not a scene or ChallengePanel image.
        lessonPanelImage.material = null;
        lessonPanelImage.color = new Color(0f, 0f, 0f, alpha);
        lessonPanelImage.SetMaterialDirty();
        lessonPanelImage.SetVerticesDirty();
    }

    private void SetForceVisualization(bool visible)
    {
        if (forceVisualizer != null) forceVisualizer.enabled = visible;
    }

    private static AudioManager.NarrationCue GetNarrationCue(string title, string message)
    {
        if (title == "ARCHIMEDES LAB") return AudioManager.NarrationCue.Introduction;
        if (title == "YOUR MISSION") return AudioManager.NarrationCue.Mission;
        if (title == "EXPERIMENT 01") return AudioManager.NarrationCue.Experiment1Introduction;
        if (title == "EXPERIMENT 02") return AudioManager.NarrationCue.Experiment2Introduction;
        if (title == "YOUR PREDICTION") return AudioManager.NarrationCue.Prediction;
        if (title == "ANSWER: PLASTIC FLOATS" || title == "EXPERIMENT NEEDS ATTENTION" && message.Contains("Plastic")) return AudioManager.NarrationCue.PredictionResult;
        if (title == "WOOD FLOATS" || title == "EXPERIMENT NEEDS ATTENTION" && message.Contains("Wood")) return AudioManager.NarrationCue.Experiment1Result;
        if (title == "STEEL SINKS" || title == "EXPERIMENT NEEDS ATTENTION" && message.Contains("Steel")) return AudioManager.NarrationCue.Experiment2Result;
        if (title == "ARCHIMEDES' PRINCIPLE") return AudioManager.NarrationCue.Principle;
        if (title == "THE FORMULA") return AudioManager.NarrationCue.Formula;
        return AudioManager.NarrationCue.Principle;
    }

    private void SkipLesson()
    {
        if (State == LabLessonState.FreeExperiment || State == LabLessonState.Challenges)
            return;
        skipRequested = true;
        HideIntroductionArrow();
        workspaceHighlight?.Clear();
        audioManager?.StopNarration();
        StopAllCoroutines();
        cameraDirector.ReturnToLabImmediately();
        EnterFreeExperiment();
    }

    private void SetExperimentControls(bool enabled)
    {
        if (materialPanel != null)
            materialPanel.SetActive(enabled);
        if (grabber != null)
            grabber.enabled = enabled;
    }

    private void BuildOverlay()
    {
        GameObject root = new GameObject("LessonOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        root.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        root.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

        GameObject panel = CreateUIObject("Panel", root.transform, typeof(Image));
        lessonPanelRect = panel.GetComponent<RectTransform>();
        lessonPanelRect.anchorMin = new Vector2(.5f, .5f); lessonPanelRect.anchorMax = new Vector2(.5f, .5f);
        lessonPanelRect.anchoredPosition = Vector2.zero;
        lessonPanelRect.sizeDelta = new Vector2(1120, 620);
        lessonPanelImage = panel.GetComponent<Image>();
        ApplyLessonPanelStyle(.60f);
        lessonPanelOutline = panel.AddComponent<Outline>();
        lessonPanelOutline.effectColor = new Color(.72f, .84f, .92f, .34f);
        lessonPanelOutline.effectDistance = new Vector2(2f, -2f);
        lessonPanelOutline.enabled = true;
        overlayGroup = panel.AddComponent<CanvasGroup>();
        GameObject header = CreateUIObject("Draggable Header", panel.transform, typeof(Image), typeof(LessonPanelDragHandler));
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f); headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(.5f, 1f); headerRect.anchoredPosition = Vector2.zero; headerRect.sizeDelta = new Vector2(0f, 96f);
        lessonHeaderImage = header.GetComponent<Image>();
        lessonHeaderImage.color = new Color(0f, 0f, 0f, .58f);
        // Lesson panels have a stable centered composition; they are not draggable workspace windows.
        header.GetComponent<LessonPanelDragHandler>().enabled = false;
        titleText = CreateText("Title", header.transform, 48, FontStyles.Bold);
        titleText.rectTransform.anchoredPosition = new Vector2(-20f, -42f);
        titleText.rectTransform.sizeDelta = new Vector2(1000f, 68f);
        titleText.raycastTarget = false;
        dragHintText = CreateText("Drag Hint", header.transform, 16, FontStyles.Bold);
        dragHintText.text = "DRAG"; dragHintText.alignment = TextAlignmentOptions.Right;
        dragHintText.rectTransform.anchoredPosition = new Vector2(390f, -42f); dragHintText.rectTransform.sizeDelta = new Vector2(80f, 34f); dragHintText.color = new Color(.65f, .85f, 1f, .8f); dragHintText.raycastTarget = false;
        bodyText = CreateText("Body", panel.transform, 31, FontStyles.Normal);
        bodyText.rectTransform.anchoredPosition = new Vector2(0f, 10f);
        bodyText.rectTransform.sizeDelta = new Vector2(930f, 365f);
        bodyText.alignment = TextAlignmentOptions.Center;
        bodyText.raycastTarget = false;
        continueButton = CreateButton("Continue", panel.transform, "CONTINUE", new Vector2(0, -250));
        skipButton = CreateButton("Skip", panel.transform, "SKIP LESSON", new Vector2(125, -175));
        challengeButton = CreateButton("Challenges", panel.transform, "START CHALLENGES", new Vector2(0, -175));
        continueButton.onClick.AddListener(RequestAdvance);
        skipButton.transform.SetParent(root.transform, false);
        RectTransform skipRect = skipButton.GetComponent<RectTransform>();
        skipRect.anchorMin = skipRect.anchorMax = new Vector2(1f, 1f);
        skipRect.pivot = new Vector2(1f, 1f);
        skipRect.anchoredPosition = new Vector2(-26f, -26f);
        skipButton.onClick.AddListener(SkipLesson);
        challengeButton.onClick.AddListener(BeginChallenges);
    }

    private void ShowOverlay(string title, string message, bool showContinue, bool showSkip, bool showChallenges)
    {
        overlayGroup.alpha = 1f; overlayGroup.blocksRaycasts = true;
        titleText.text = title; bodyText.text = message;
        continueButton.gameObject.SetActive(showContinue);
        skipButton.gameObject.SetActive(showSkip);
        challengeButton.gameObject.SetActive(showChallenges);
    }

    private void HideOverlay()
    {
        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;
    }

    private TMP_Text CreateText(string name, Transform parent, float size, FontStyles style)
    {
        GameObject go = CreateUIObject(name, parent, typeof(TextMeshProUGUI));
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = size; text.fontStyle = style; text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white; text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(.5f, .5f);
        text.rectTransform.sizeDelta = new Vector2(700, 70);
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 position)
    {
        GameObject go = CreateUIObject(name, parent, typeof(Image), typeof(Button));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(250, 62);
        go.GetComponent<Image>().color = new Color(.06f, .48f, .73f, 1f);
        TMP_Text text = CreateText("Label", go.transform, 23, FontStyles.Bold);
        text.text = label; text.rectTransform.anchoredPosition = Vector2.zero; text.rectTransform.sizeDelta = rect.sizeDelta;
        text.raycastTarget = false;
        return go.GetComponent<Button>();
    }

    private GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject go = new GameObject(name, components);
        go.transform.SetParent(parent, false);
        return go;
    }
}
