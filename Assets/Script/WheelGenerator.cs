using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WheelGenerator : MonoBehaviour
{
    public WheelSegment[] segments;
    public float radius = 3f;
    public GameObject slicePrefab; // un prefab avec une Image (type Filled - Radial360) + TextMeshProUGUI enfant

    public void Generate()
    {
        float totalRate = 0f;
        foreach (var s in segments) totalRate += s.dropRate;

        float currentAngle = 0f;

        foreach (var seg in segments)
        {
            float normalized = seg.dropRate / totalRate;
            float sliceAngle = 360f * normalized;

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
            float angletext  = -90 + (sliceAngle/2);
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
                //slice.transform.GetChild(2).transform.localRotation = Quaternion.Euler(0f, 0f,(sliceAngle/2));

                
                if (seg.artwork != null)
                {
                    artwork.sprite = seg.artwork;
                    artwork.color = new Color(1f, 1f, 1f, 1f);
                    
                    txt.text = "";
                }
                
                slice.transform.GetChild(1).transform.localRotation = Quaternion.Euler(0f, 0f,(sliceAngle/2));

                // --- Prefab setup ---
                seg.prefab = slice;

            currentAngle += sliceAngle;
        }
    }

    public void LoadSegments(WheelSegment[] segments)
    {
        this.segments = segments;
        Generate();
    }
    
}
