using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WheelSegment
{
    public string label;
    [Range(0f, 1f)] public float dropRate;
    public Color color = Color.white;
    public Sprite artwork;
    public GameObject prefab;
}