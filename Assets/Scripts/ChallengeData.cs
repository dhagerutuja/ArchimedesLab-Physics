using UnityEngine;

public enum ChallengeSuccessCondition { StableFloating, StableSinking, GreatestBuoyantForce, ProveCannotFloat }

[CreateAssetMenu(fileName = "NewChallenge", menuName = "Archimedes/Challenge")]
public class ChallengeData : ScriptableObject
{
    public string challengeTitle;
    [TextArea] public string description;
    public MaterialData targetMaterial;
    public FluidData targetFluid;
    public ChallengeSuccessCondition successCondition;
    [TextArea] public string educationalExplanation;
}
