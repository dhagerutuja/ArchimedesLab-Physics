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
        if (materials != null)
        {
            body = materials.rb;
            sampleCollider = materials.objectCollider;
        }
        materialPanel = GameObject.Find("MaterialPanel");
        evaluator = gameObject.AddComponent<PhysicsStateEvaluator>();
        cameraDirector = gameObject.AddComponent<LabCameraDirector>();
        freeExperimentPresenter = body != null ? body.GetComponent<FreeExperimentObjectPresenter>() : null;
        if (freeExperimentPresenter == null && body != null) freeExperimentPresenter = body.gameObject.AddComponent<FreeExperimentObjectPresenter>();
        HoldSampleAtExperimentSpawn();
        BuildOverlay();
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
        yield return ShowFor("ARCHIMEDES LAB", "Mission: Discover why objects float or sink.", 8f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }
        yield return ShowFor("YOUR MISSION", "Every object placed in a fluid experiences forces. Discover what determines the result.", 9f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        State = LabLessonState.DemonstrationWood;
        PreparePrediction(materials.wood, water.water);
        yield return ShowFor("EXPERIMENT 01", "Wood enters the water. Watch the live values and force arrows.", 8f);
        yield return RunExperiment(materials.wood, water.water, PhysicalState.Floating, "Wood");
        cameraDirector.ReturnToLab(); yield return new WaitForSeconds(1.2f);
        yield return ShowFor(
            experimentReachedExpectedState ? "WOOD FLOATS" : "EXPERIMENT NEEDS ATTENTION",
            experimentReachedExpectedState ? ValuesText("Wood is less dense than water, so it floats.") : "Wood did not reach a stable floating state within the safety timeout. The issue has been logged for debugging; no result is being claimed.", 9f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        State = LabLessonState.DemonstrationSteel;
        PreparePrediction(materials.steel, water.water);
        yield return ShowFor("EXPERIMENT 02", "Now replace the wood with steel. The simulation—not a scripted result—determines what happens.", 8f);
        yield return RunExperiment(materials.steel, water.water, PhysicalState.Sinking, "Steel");
        cameraDirector.ReturnToLab(); yield return new WaitForSeconds(1.2f);
        yield return ShowFor(
            experimentReachedExpectedState ? "STEEL SINKS" : "EXPERIMENT NEEDS ATTENTION",
            experimentReachedExpectedState ? ValuesText("Steel is denser than water. Its weight is greater than the buoyant force, so it sinks.") : "Steel did not reach a stable sinking state within the safety timeout. The issue has been logged for debugging; no result is being claimed.", 9f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        State = LabLessonState.Explanation;
        yield return ShowFor("ARCHIMEDES' PRINCIPLE", "DISPLACED FLUID  ↓  BUOYANT FORCE ↑\n\nBuoyant Force = Fluid Density × Gravity × Displaced Volume\nWeight = Mass × Gravity", 11f);
        HideOverlay(); cameraDirector.FocusExperiment(); yield return new WaitForSeconds(4f); cameraDirector.ReturnToLab(); yield return new WaitForSeconds(1.2f);
        yield return ShowFor("ARCHIMEDES' PRINCIPLE", "The live data and arrows show the formula in action. When buoyant force balances weight, the sample settles.", 8f);
        if (skipRequested) { EnterFreeExperiment(); yield break; }

        State = LabLessonState.GuidedExperiment;
        PreparePrediction(materials.plastic, water.water);
        yield return ShowFor("YOUR PREDICTION", "Plastic density is 950 kg/m³.\nWater density is 1000 kg/m³.\n\nWill plastic float or sink?\n\nContinue to test your prediction.", 11f);
        yield return RunExperiment(materials.plastic, water.water, PhysicalState.Floating, "Plastic");
        cameraDirector.ReturnToLab(); yield return new WaitForSeconds(1.2f);
        yield return ShowFor(
            experimentReachedExpectedState ? "ANSWER: PLASTIC FLOATS" : "EXPERIMENT NEEDS ATTENTION",
            experimentReachedExpectedState ? ValuesText("It is mostly submerged, yet it floats when buoyant force balances weight. Floating does not mean half-submerged.") : "Plastic did not reach a stable floating state within the safety timeout. The issue has been logged for debugging; no result is being claimed.", 9f);
        EnterFreeExperiment();
    }

    private IEnumerator ShowFor(string title, string message, float minimumSeconds = 8f)
    {
        ShowOverlay(title, message, true, true, false);
        advanceRequested = false;
        continueButton.interactable = false;
        yield return new WaitForSeconds(minimumSeconds);
        continueButton.interactable = true;
        while (!skipRequested && !advanceRequested)
        {
            yield return null;
        }
    }

    private IEnumerator RunExperiment(MaterialData material, FluidData fluid, PhysicalState expected, string experimentName)
    {
        HideOverlay();
        cameraDirector.FocusExperiment();
        yield return new WaitForSeconds(1.15f);
        SetExperiment(material, fluid);
        yield return WaitForExpectedState(expected, experimentName, 12f);
        yield return new WaitForSeconds(1.5f);
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
        SetExperimentControls(true);
        freeExperimentPresenter?.BeginFreeExperiment();
        ShowOverlay("EXPERIMENT FREELY", "Drag the object from the lab floor into the fluid to begin the experiment. Change the material and fluid to see how density affects floating and sinking.", false, false, true);
        challengeButton.interactable = true;
    }

    private void BeginChallenges()
    {
        State = LabLessonState.Challenges;
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
        GameObject root = new GameObject("LessonOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        root.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        root.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        overlayGroup = root.GetComponent<CanvasGroup>();

        GameObject panel = CreateUIObject("Panel", root.transform, typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(.5f, .5f); panelRect.anchorMax = new Vector2(.5f, .5f);
        panelRect.anchoredPosition = new Vector2(-470f, 0f);
        panelRect.sizeDelta = new Vector2(920, 480);
        panel.GetComponent<Image>().color = new Color(.025f, .06f, .11f, .92f);
        GameObject header = CreateUIObject("Draggable Header", panel.transform, typeof(Image), typeof(LessonPanelDragHandler));
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f); headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(.5f, 1f); headerRect.anchoredPosition = Vector2.zero; headerRect.sizeDelta = new Vector2(0f, 88f);
        header.GetComponent<Image>().color = new Color(.06f, .22f, .34f, .96f);
        header.GetComponent<LessonPanelDragHandler>().Configure(panelRect);
        titleText = CreateText("Title", header.transform, 44, FontStyles.Bold);
        titleText.rectTransform.anchoredPosition = new Vector2(-20f, -42f);
        titleText.rectTransform.sizeDelta = new Vector2(760f, 64f);
        titleText.raycastTarget = false;
        TMP_Text dragHint = CreateText("Drag Hint", header.transform, 16, FontStyles.Bold);
        dragHint.text = "DRAG"; dragHint.alignment = TextAlignmentOptions.Right;
        dragHint.rectTransform.anchoredPosition = new Vector2(390f, -42f); dragHint.rectTransform.sizeDelta = new Vector2(80f, 34f); dragHint.color = new Color(.65f, .85f, 1f, .8f); dragHint.raycastTarget = false;
        bodyText = CreateText("Body", panel.transform, 28, FontStyles.Normal);
        bodyText.rectTransform.anchoredPosition = new Vector2(0f, 10f);
        bodyText.rectTransform.sizeDelta = new Vector2(820f, 275f);
        bodyText.alignment = TextAlignmentOptions.Center;
        bodyText.raycastTarget = false;
        continueButton = CreateButton("Continue", panel.transform, "CONTINUE", new Vector2(-125, -175));
        skipButton = CreateButton("Skip", panel.transform, "SKIP LESSON", new Vector2(125, -175));
        challengeButton = CreateButton("Challenges", panel.transform, "START CHALLENGES", new Vector2(0, -175));
        continueButton.onClick.AddListener(() => advanceRequested = true);
        skipButton.onClick.AddListener(() => skipRequested = true);
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
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(210, 52);
        go.GetComponent<Image>().color = new Color(.08f, .45f, .7f, 1f);
        TMP_Text text = CreateText("Label", go.transform, 22, FontStyles.Bold);
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
