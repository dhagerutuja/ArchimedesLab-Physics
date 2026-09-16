using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>Executes data-defined challenges against the same settled physics state used by lessons.</summary>
public class ChallengeManager : MonoBehaviour
{
    private enum ChallengeState { NotStarted, Intro, Evaluating, Success, Failure }

    public Rigidbody rb;
    public Collider objectCollider;
    public WaterVolume water;
    public TMP_Text challengeStatus;
    public GameObject challengeResultPanel;
    public TMP_Text challengeResultText;
    public MaterialController materialController;

    private PhysicsStateEvaluator stateEvaluator;
    private ChallengeData activeChallenge;
    private ChallengeState state = ChallengeState.NotStarted;
    private bool attemptInFluid;
    private MaterialData lastMaterial;
    private FluidData lastFluid;
    private GameObject challengePanel;
    private ObjectGrabber grabber;

    void Start()
    {
        stateEvaluator = GetComponent<PhysicsStateEvaluator>();
        if (stateEvaluator == null)
            stateEvaluator = gameObject.AddComponent<PhysicsStateEvaluator>();
        if (challengeResultPanel != null) challengeResultPanel.SetActive(false);
        challengePanel = challengeStatus != null ? challengeStatus.transform.parent.gameObject : null;
        StylePanels();
        if (challengePanel != null) challengePanel.SetActive(false);
        lastMaterial = materialController.currentMaterial;
        lastFluid = water.currentFluid;
        grabber = FindAnyObjectByType<ObjectGrabber>();
    }

    void Update()
    {
        if (materialController.currentMaterial != lastMaterial || water.currentFluid != lastFluid)
        {
            BeginRetry();
            lastMaterial = materialController.currentMaterial;
            lastFluid = water.currentFluid;
        }
    }

    void FixedUpdate()
    {
        if (activeChallenge == null || rb == null || objectCollider == null || water == null ||
            state == ChallengeState.Success || state == ChallengeState.Failure)
            return;

        // A held object is not an experiment result.  It must be released and actually
        // interacting with the fluid before this attempt begins evaluation.
        if (rb.isKinematic || (grabber != null && grabber.IsGrabbing))
        {
            stateEvaluator.ResetState();
            return;
        }

        BuoyancyPhysics.Snapshot snapshot = water.GetPhysicsSnapshot(rb, objectCollider);
        if (!attemptInFluid)
        {
            if (!snapshot.IsInFluid) return;
            attemptInFluid = true;
            state = ChallengeState.Evaluating;
            stateEvaluator.ResetState();
            return;
        }

        stateEvaluator.Evaluate(snapshot, rb);
        if (!stateEvaluator.IsStable) return;
        if (MeetsSuccessCondition()) CompleteChallenge();
        else if (stateEvaluator.CurrentState == PhysicalState.Sinking) ShowFailure();
    }

    public void BeginChallenge(ChallengeData challenge)
    {
        activeChallenge = challenge;
        state = challenge == null ? ChallengeState.NotStarted : ChallengeState.Intro;
        attemptInFluid = false;
        stateEvaluator.ResetState();
        if (challengeResultPanel != null) challengeResultPanel.SetActive(false);
        if (challengePanel != null) challengePanel.SetActive(challenge != null);
        SetStatus(challenge == null ? string.Empty : ActiveChallengeText());
    }

    public void ResetFeedback()
    {
        BeginRetry();
    }

    private void BeginRetry()
    {
        if (activeChallenge == null) return;
        // Deliberately do not write to ChallengePanel here. The initial instruction is
        // one-time only, and a SUCCESS/FAIL result stays visible during the next attempt.
        attemptInFluid = false;
        stateEvaluator?.ResetState();
        if (state != ChallengeState.Intro)
            state = ChallengeState.Evaluating;
        if (challengeResultPanel != null) challengeResultPanel.SetActive(false);
    }

    private bool MeetsSuccessCondition()
    {
        if (activeChallenge.targetMaterial != null && materialController.currentMaterial != activeChallenge.targetMaterial)
            return false;
        if (activeChallenge.targetFluid != null && water.currentFluid != activeChallenge.targetFluid)
            return false;

        switch (activeChallenge.successCondition)
        {
            case ChallengeSuccessCondition.StableFloating:
                return stateEvaluator.CurrentState == PhysicalState.Floating;
            case ChallengeSuccessCondition.StableSinking:
                return stateEvaluator.CurrentState == PhysicalState.Sinking;
            // These are intentionally data-ready for Stage 4, where their multi-attempt rules are introduced.
            default:
                return false;
        }
    }

    private void CompleteChallenge()
    {
        state = ChallengeState.Success;
        string explanation = string.IsNullOrWhiteSpace(activeChallenge.educationalExplanation)
            ? "The stable result matches your prediction."
            : activeChallenge.educationalExplanation;
        SetStatus($"<size=26><b>CHALLENGE 01</b></size>\n\n<align=center><size=38><b>SUCCESS</b></size>\n\nThe object is floating.\n\n<size=18>{explanation}</size></align>");
    }

    private void ShowFailure()
    {
        state = ChallengeState.Failure;
        SetStatus("<b>CHALLENGE 01</b>\n\n<align=center><size=30><b>FAIL — TRY AGAIN</b></size>\n\nThe object is sinking.\n\n<size=18>Choose a less dense material or a denser fluid, then drag it into the water again.</size></align>");
    }

    private string ActiveChallengeText()
    {
        if (activeChallenge == null) return string.Empty;
        return $"<size=26><b>CHALLENGE 01</b></size>\n\n<b>MAKE THE OBJECT FLOAT</b>\n\n{activeChallenge.description}";
    }

    private void SetStatus(string value)
    {
        if (challengeStatus != null)
            challengeStatus.text = value;
    }

    private void StylePanels()
    {
        if (challengePanel != null)
        {
            RectTransform rect = challengePanel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-26f, -26f); rect.sizeDelta = new Vector2(400f, 270f);
            Image image = challengePanel.GetComponent<Image>(); if (image != null) image.color = new Color(.025f, .08f, .14f, .94f);
            foreach (Transform child in challengePanel.transform)
                if (child != challengeStatus.transform) child.gameObject.SetActive(false);
            RectTransform textRect = challengeStatus.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f); textRect.anchorMax = new Vector2(1f, 1f); textRect.offsetMin = new Vector2(24f, 20f); textRect.offsetMax = new Vector2(-24f, -18f);
            challengeStatus.alignment = TextAlignmentOptions.TopLeft; challengeStatus.fontSize = 20f; challengeStatus.color = new Color(.88f, .95f, 1f);
        }
        // Challenge 01 always reports in ChallengePanel; this legacy panel remains hidden.
        if (challengeResultPanel != null) challengeResultPanel.SetActive(false);
    }

}
