using UnityEngine;

[System.Serializable]
public struct ZoneStage
{
    public string stageName;
    public float radius;
    public float duration;            // seconds
    public float damagePerSecStart;   // %/s at stage start
    public float damagePerSecEnd;     // %/s at stage end
}
