using UnityEngine;

/// <summary>
/// Behaviour attached to attack pickup instances that can be collected by PokeUnits.
/// </summary>
public class CombatPickup : MonoBehaviour
{
    [SerializeField] private AttackPickupType type = AttackPickupType.Attack;
    [SerializeField, Min(0.05f)] private float pickupRadius = 0.5f;

    public AttackPickupType Type => type;
    public float PickupRadius => pickupRadius;

    public void Configure(AttackPickupType pickupType, float radius)
    {
        type = pickupType;
        pickupRadius = Mathf.Max(radius, 0.05f);
    }
}
