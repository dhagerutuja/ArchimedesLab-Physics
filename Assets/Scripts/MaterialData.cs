using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterialData", menuName = "Archimedes/Material Data")]
public class MaterialData : ScriptableObject
{
    public string materialName;
    public float density;
}