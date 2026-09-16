using UnityEngine;

public enum PhysicalState { Settling, Floating, Sinking, Submerged, OutOfFluid }

/// <summary>Classifies a settled result, never a momentary downward movement.</summary>
public class PhysicsStateEvaluator : MonoBehaviour
{
    [SerializeField] private float stableVelocity = 0.08f;
    [SerializeField] private float requiredStableTime = 0.8f;
    [SerializeField] private float forceTolerance = 0.08f;
    private PhysicalState candidate;
    private float candidateTime;
    public PhysicalState CurrentState { get; private set; } = PhysicalState.Settling;
    public bool IsStable { get; private set; }

    public void ResetState()
    {
        candidate = PhysicalState.Settling;
        candidateTime = 0f;
        CurrentState = PhysicalState.Settling;
        IsStable = false;
    }

    public void Evaluate(BuoyancyPhysics.Snapshot snapshot, Rigidbody body)
    {
        PhysicalState next = Classify(snapshot, body);
        bool atRest = body != null && Mathf.Abs(body.linearVelocity.y) <= stableVelocity;
        if (!atRest || next == PhysicalState.Settling)
        {
            ResetState();
            return;
        }

        if (next != candidate)
        {
            candidate = next;
            candidateTime = 0f;
            IsStable = false;
        }

        candidateTime += Time.fixedDeltaTime;
        if (candidateTime >= requiredStableTime)
        {
            CurrentState = candidate;
            IsStable = true;
        }
    }

    private PhysicalState Classify(BuoyancyPhysics.Snapshot snapshot, Rigidbody body)
    {
        if (!snapshot.IsInFluid || snapshot.SubmergedRatio <= 0.001f)
            return PhysicalState.OutOfFluid;

        float balanceTolerance = Mathf.Max(0.5f, snapshot.Weight * forceTolerance);
        bool balanced = Mathf.Abs(snapshot.BuoyantForce - snapshot.Weight) <= balanceTolerance;
        // A 95% submerged plastic block is floating; surface intersection, not a 50% rule, matters.
        if (snapshot.SubmergedRatio < 0.999f && balanced)
            return PhysicalState.Floating;
        if (snapshot.SubmergedRatio >= 0.999f && snapshot.BuoyantForce < snapshot.Weight - balanceTolerance)
            return PhysicalState.Sinking;
        return PhysicalState.Submerged;
    }
}
