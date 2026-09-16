using UnityEngine;

[CreateAssetMenu(fileName = "NewFluidData", menuName = "Archimedes/Fluid Data")]
public class FluidData : ScriptableObject
{
    public string fluidName;
    public float density;
}