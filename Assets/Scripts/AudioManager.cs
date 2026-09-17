using UnityEngine;

/// <summary>Owns the single 2D narration channel used by the lab lesson.</summary>
public class AudioManager : MonoBehaviour
{
    public enum NarrationCue
    {
        Introduction, Mission, Experiment1Introduction, Experiment1Result,
        Experiment2Introduction, Experiment2Result, Principle, Formula,
        Prediction, PredictionResult, FreeExperimentIntroduction,
        ChallengeIntroduction, ChallengeSuccess, ChallengeFailure,
        PhysicsPanelIntroduction, MaterialFluidPanelIntroduction, TankIntroduction, TestObjectIntroduction
    }

    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioClip introduction;
    [SerializeField] private AudioClip mission;
    [SerializeField] private AudioClip experiment1Introduction;
    [SerializeField] private AudioClip experiment1Result;
    [SerializeField] private AudioClip experiment2Introduction;
    [SerializeField] private AudioClip experiment2Result;
    [SerializeField] private AudioClip principle;
    [SerializeField] private AudioClip formula;
    [SerializeField] private AudioClip prediction;
    [SerializeField] private AudioClip predictionResult;
    [SerializeField] private AudioClip freeExperimentIntroduction;
    [SerializeField] private AudioClip challengeIntroduction;
    [SerializeField] private AudioClip challengeSuccess;
    [SerializeField] private AudioClip challengeFailure;
    [SerializeField] private AudioClip physicsPanelIntroduction;
    [SerializeField] private AudioClip materialFluidPanelIntroduction;
    [SerializeField] private AudioClip tankIntroduction;
    [SerializeField] private AudioClip testObjectIntroduction;

    public bool IsNarrationPlaying => narrationSource != null && narrationSource.isPlaying;

    private void Awake()
    {
        if (narrationSource == null)
            narrationSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        narrationSource.playOnAwake = false;
        narrationSource.loop = false;
        narrationSource.spatialBlend = 0f;
    }

    public bool PlayNarration(NarrationCue cue)
    {
        AudioClip clip = GetClip(cue);
        if (clip == null)
        {
            Debug.LogWarning($"Narration clip for {cue} is not assigned.", this);
            return false;
        }
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
        return true;
    }

    public void StopNarration()
    {
        if (narrationSource != null) narrationSource.Stop();
    }

    private AudioClip GetClip(NarrationCue cue)
    {
        switch (cue)
        {
            case NarrationCue.Introduction: return introduction;
            case NarrationCue.Mission: return mission;
            case NarrationCue.Experiment1Introduction: return experiment1Introduction;
            case NarrationCue.Experiment1Result: return experiment1Result;
            case NarrationCue.Experiment2Introduction: return experiment2Introduction;
            case NarrationCue.Experiment2Result: return experiment2Result;
            case NarrationCue.Principle: return principle;
            case NarrationCue.Formula: return formula;
            case NarrationCue.Prediction: return prediction;
            case NarrationCue.PredictionResult: return predictionResult;
            case NarrationCue.FreeExperimentIntroduction: return freeExperimentIntroduction;
            case NarrationCue.ChallengeIntroduction: return challengeIntroduction;
            case NarrationCue.ChallengeSuccess: return challengeSuccess;
            case NarrationCue.ChallengeFailure: return challengeFailure;
            case NarrationCue.PhysicsPanelIntroduction: return physicsPanelIntroduction;
            case NarrationCue.MaterialFluidPanelIntroduction: return materialFluidPanelIntroduction;
            case NarrationCue.TankIntroduction: return tankIntroduction;
            case NarrationCue.TestObjectIntroduction: return testObjectIntroduction;
            default: return null;
        }
    }
}
