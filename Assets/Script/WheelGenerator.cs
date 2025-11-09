using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WheelGenerator : MonoBehaviour
{
    [Serializable]
    public struct SegmentLayout
    {
        // Angles exprimés en degrés dans l'espace de la roue : 0 = haut,
        // valeurs positives = rotation anti-horaire, valeurs négatives = horaire.
        public float startAngle;
        public float endAngle;
        public float centerAngle;
    }

    public WheelSegment[] segments;
    public float radius = 3f;
    public GameObject slicePrefab; // un prefab avec une Image (type Filled - Radial360) + TextMeshProUGUI enfant

    public SegmentLayout[] Layouts { get; private set; }

    public void Generate()
    {
        ClearChildren();

        if (segments == null || segments.Length == 0)
        {
            Layouts = Array.Empty<SegmentLayout>();
            return;
        }

        Array.Sort(segments, (a, b) => b.dropRate.CompareTo(a.dropRate));

        float totalRate = 0f;
        foreach (var s in segments) totalRate += s.dropRate;

        if (totalRate <= 0f)
        {
            Layouts = Array.Empty<SegmentLayout>();
            return;
        }

        Layouts = new SegmentLayout[segments.Length];

        float currentAngle = 0f;

        for (int i = 0; i < segments.Length; i++)
        {
            var seg = segments[i];
            float normalized = seg.dropRate / totalRate;
            float sliceAngle = 360f * normalized;
            float startAngle = -(currentAngle + sliceAngle);
            float endAngle = -currentAngle;
            float centerAngle = -(currentAngle + sliceAngle / 2f);

            Layouts[i] = new SegmentLayout
            {
                startAngle = startAngle,
                endAngle = endAngle,
                centerAngle = centerAngle
            };

            // --- Instantiation ---
            GameObject slice = Instantiate(slicePrefab, transform);
            slice.transform.localPosition = Vector3.zero;
            slice.transform.localRotation = Quaternion.identity;

            // --- Image setup ---
            var img = slice.GetComponent<Image>();
            if (img)
            {
                img.color = seg.color;
                img.fillAmount = normalized;
                img.fillOrigin = (int)Image.Origin360.Top; // assure que le départ est vers le haut
                img.fillClockwise = false;
            }

            // --- Rotation du slice ---
            // On tourne chaque slice de façon à aligner son centre avec le bon angle
            float rotationOffset = currentAngle + sliceAngle;
            slice.transform.localRotation = Quaternion.Euler(0f, 0f, -rotationOffset);
            float angletext = -90 + (sliceAngle / 2f);
            // --- Texte setup ---
            var txt = slice.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (txt)
            {
                txt.text = seg.label;

                // Positionne le texte au centre de la part

                txt.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angletext);
            }

            // --- Artwork setup ---
            var artwork = slice.transform.GetChild(1).GetChild(0).GetComponent<Image>();

            if (seg.artwork != null)
            {
                artwork.sprite = seg.artwork;
                artwork.color = new Color(1f, 1f, 1f, 1f);

                slice.transform.GetChild(1).transform.localRotation = Quaternion.Euler(0f, 0f, sliceAngle / 2f);

                txt.text = string.Empty;
            }

            currentAngle += sliceAngle;
        }
    }

    public float GetSegmentCenterAngle(int index)
    {
        if (Layouts == null || index < 0 || index >= Layouts.Length) return 0f;
        return Layouts[index].centerAngle;
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    public void LoadSegments(WheelSegment[] segments)
    {
        this.segments = segments;
        Generate();
    }

}
