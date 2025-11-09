using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class SpinWheelManager : MonoBehaviour
{
    public WheelGenerator wheel;
    public float spinDuration = 4f;
    public AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public int seed = 0;

    private bool spinning = false;
    private float[] cumulativeWeights;
    
    public event Action<WheelSegment> OnSegmentSelected;


    void Start()
    {
        if (wheel == null) wheel = GetComponent<WheelGenerator>();
        wheel.Generate();
        ComputeCumulativeWeights();
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !spinning)
        {
            StartCoroutine(SpinWheel());
        }
    }

    void ComputeCumulativeWeights()
    {
        if (wheel == null || wheel.segments == null || wheel.segments.Length == 0)
        {
            cumulativeWeights = Array.Empty<float>();
            return;
        }

        float total = 0f;
        cumulativeWeights = new float[wheel.segments.Length];
        for (int i = 0; i < wheel.segments.Length; i++)
        {
            total += wheel.segments[i].dropRate;
            cumulativeWeights[i] = total;
        }
    }

    IEnumerator SpinWheel()
    {
        if (cumulativeWeights == null || cumulativeWeights.Length == 0)
            yield break;

        spinning = true;
        Random.InitState(seed == 0 ? System.Environment.TickCount : seed);

        float total = cumulativeWeights[cumulativeWeights.Length - 1];
        if (total <= 0f)
        {
            spinning = false;
            yield break;
        }
        float rand = Random.Range(0f, total);

        // Trouve le segment gagnant
        int winnerIndex = 0;
        for (int i = 0; i < cumulativeWeights.Length; i++)
        {
            if (rand <= cumulativeWeights[i])
            {
                winnerIndex = i;
                break;
            }
        }

        float targetAngle = GetTargetAngle(winnerIndex);
        float fullRotations = 720f; // 2 tours de base
        float startAngle = wheel.transform.eulerAngles.z;
        float endAngle = startAngle + fullRotations + targetAngle;

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = spinCurve.Evaluate(elapsed / spinDuration);
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            wheel.transform.eulerAngles = new Vector3(0, 0, currentAngle);
            yield return null;
        }

        wheel.transform.eulerAngles = new Vector3(0, 0, endAngle);
        spinning = false;
        //attendre x secondes avant de terminer la fonction
        yield return new WaitForSeconds(2f);

        Debug.Log($"Résultat : {wheel.segments[winnerIndex].label}");
        OnSegmentSelected?.Invoke(wheel.segments[winnerIndex]);
    }

    float GetTargetAngle(int index)
    {
        return -wheel.GetSegmentCenterAngle(index); // sens anti-horaire
    }
}
