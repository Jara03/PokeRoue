using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a two fighter battle that takes place inside a circular arena.
/// Units bounce on the borders, collect attack pickups and damage each other on collision.
/// </summary>
public class ArenaCombatController : MonoBehaviour
{
    private static readonly Vector2 InitialDirectionA = new(1f, 0.45f);
    private static readonly Vector2 InitialDirectionB = new(-0.75f, -1f);

    [Header("Arena")]
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private float arenaRadius = 5f;
    [SerializeField] private float collisionDistance = 0.75f;

    [Header("Fighters")]
    [SerializeField] private FighterSlot[] fighters = new FighterSlot[2];

    [Header("Pickups")]
    [SerializeField] private CombatPickup attackPickupPrefab;
    [SerializeField] private CombatPickup specialPickupPrefab;
    [SerializeField, Min(0f)] private float spawnInterval = 4f;
    [SerializeField, Min(1)] private int maxSimultaneousPickups = 3;

    private readonly List<CombatPickup> _activePickups = new();
    private float _spawnTimer;

    private void Awake()
    {
        if (fighters == null || fighters.Length != 2)
        {
            fighters = new FighterSlot[2];
        }

        for (var i = 0; i < fighters.Length; i++)
        {
            fighters[i]?.ResetRuntimeState();
        }
    }

    private void Start()
    {
        if (fighters.Length < 2)
        {
            Debug.LogWarning("ArenaCombatController requires exactly two fighters configured.");
            return;
        }

        InitializeFighter(fighters[0], InitialDirectionA);
        InitializeFighter(fighters[1], InitialDirectionB);
    }

    private void InitializeFighter(FighterSlot slot, Vector2 suggestedDirection)
    {
        if (slot == null)
        {
            return;
        }

        slot.ResetRuntimeState();
        if (suggestedDirection.sqrMagnitude < 0.001f)
        {
            suggestedDirection = UnityEngine.Random.insideUnitCircle;
        }

        if (suggestedDirection.sqrMagnitude < 0.001f)
        {
            suggestedDirection = Vector2.right;
        }

        slot.CurrentVelocity = suggestedDirection.normalized * slot.MoveSpeed;

        if (slot.UnitData != null)
        {
            slot.CurrentHealth = slot.UnitData.pv;
        }

        // If the fighter starts outside the arena, clamp it to the circle.
        ConstrainInsideArena(slot);
    }

    private void FixedUpdate()
    {
        if (fighters.Length < 2)
        {
            return;
        }

        SimulateMovement();
        HandlePickups();
        HandleFighterCollision();
        SpawnPickups();
    }

    private void SimulateMovement()
    {
        foreach (var fighter in fighters)
        {
            if (fighter == null || !fighter.IsRuntimeReady)
            {
                continue;
            }

            if (fighter.CurrentVelocity.sqrMagnitude < 0.001f)
            {
                var randomDirection = UnityEngine.Random.insideUnitCircle;
                if (randomDirection.sqrMagnitude < 0.01f)
                {
                    randomDirection = Vector2.up;
                }

                fighter.CurrentVelocity = randomDirection.normalized * fighter.MoveSpeed;
            }

            var currentPosition = fighter.GetPosition();
            currentPosition += (Vector3)(fighter.CurrentVelocity * Time.fixedDeltaTime);
            fighter.SetPosition(currentPosition);

            var offset = (Vector2)(currentPosition - ArenaCenterPosition);
            var distance = offset.magnitude;

            if (distance > arenaRadius)
            {
                var normal = offset.normalized;
                fighter.SetPosition(ArenaCenterPosition + (Vector3)(normal * arenaRadius));
                var reflected = Vector2.Reflect(fighter.CurrentVelocity, normal);
                if (reflected.sqrMagnitude < 0.001f)
                {
                    reflected = Vector2.Perpendicular(normal);
                }

                fighter.CurrentVelocity = reflected.normalized * fighter.MoveSpeed;
            }
        }
    }

    private void HandlePickups()
    {
        foreach (var fighter in fighters)
        {
            if (fighter == null || !fighter.IsRuntimeReady)
            {
                continue;
            }

            for (var i = _activePickups.Count - 1; i >= 0; i--)
            {
                var pickup = _activePickups[i];
                if (pickup == null)
                {
                    _activePickups.RemoveAt(i);
                    continue;
                }

                var distance = Vector2.Distance(fighter.GetPosition(), pickup.transform.position);
                if (distance <= pickup.PickupRadius)
                {
                    AssignPickupToFighter(fighter, pickup);
                    _activePickups.RemoveAt(i);
                    Destroy(pickup.gameObject);
                    break;
                }
            }
        }
    }

    private void HandleFighterCollision()
    {
        var fighterA = fighters[0];
        var fighterB = fighters[1];

        if (fighterA == null || fighterB == null || !fighterA.IsRuntimeReady || !fighterB.IsRuntimeReady)
        {
            return;
        }

        var distance = Vector2.Distance(fighterA.GetPosition(), fighterB.GetPosition());
        if (distance > collisionDistance)
        {
            return;
        }

        FighterSlot attacker = null;
        FighterSlot defender = null;

        if (fighterA.HasPickup && !fighterB.HasPickup)
        {
            attacker = fighterA;
            defender = fighterB;
        }
        else if (!fighterA.HasPickup && fighterB.HasPickup)
        {
            attacker = fighterB;
            defender = fighterA;
        }
        else if (fighterA.HasPickup && fighterB.HasPickup)
        {
            // In the rare situation both fighters somehow have a pickup, pick the faster one to attack.
            attacker = fighterA.MoveSpeed >= fighterB.MoveSpeed ? fighterA : fighterB;
            defender = attacker == fighterA ? fighterB : fighterA;
            defender.ClearPickup();
        }

        if (attacker == null || defender == null)
        {
            return;
        }

        ApplyDamage(attacker, defender);
        attacker.ClearPickup();

        ApplyBounceResponse(fighterA, fighterB);
    }

    private void ApplyBounceResponse(FighterSlot fighterA, FighterSlot fighterB)
    {
        if (fighterA == null || fighterB == null)
        {
            return;
        }

        var separation = (Vector2)(fighterB.GetPosition() - fighterA.GetPosition());
        var distanceSquared = separation.sqrMagnitude;
        if (distanceSquared < 0.0001f)
        {
            separation = Vector2.up * collisionDistance;
            distanceSquared = separation.sqrMagnitude;
        }

        var collisionNormal = separation.normalized;
        var relativeVelocity = fighterA.CurrentVelocity - fighterB.CurrentVelocity;

        if (Vector2.Dot(relativeVelocity, collisionNormal) > 0f)
        {
            // Fighters already separating, no bounce required.
            return;
        }

        fighterA.CurrentVelocity = Vector2.Reflect(fighterA.CurrentVelocity, collisionNormal).normalized * fighterA.MoveSpeed;
        fighterB.CurrentVelocity = Vector2.Reflect(fighterB.CurrentVelocity, -collisionNormal).normalized * fighterB.MoveSpeed;
    }

    private void ApplyDamage(FighterSlot attacker, FighterSlot defender)
    {
        if (attacker == null || defender == null)
        {
            return;
        }

        var baseDamage = attacker.GetDamageValue();
        var collisionIntensity = attacker.CurrentVelocity.magnitude;
        var damageMultiplier = 1f + collisionIntensity * 0.25f;
        var damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * damageMultiplier));
        defender.CurrentHealth -= damage;
        defender.CurrentHealth = Mathf.Max(defender.CurrentHealth, 0f);

        Debug.Log($"{attacker.DisplayName} attaque {defender.DisplayName} et inflige {damage} dégâts. PV restants: {defender.CurrentHealth}");

        if (defender.CurrentHealth <= 0f)
        {
            Debug.Log($"{defender.DisplayName} est K.O !");
        }
    }

    private void AssignPickupToFighter(FighterSlot fighter, CombatPickup pickup)
    {
        if (fighter == null || pickup == null)
        {
            return;
        }

        foreach (var other in fighters)
        {
            if (other == null || other == fighter)
            {
                continue;
            }

            if (other.HasPickup)
            {
                other.ClearPickup();
            }
        }

        fighter.EquipPickup(pickup.Type);
        Debug.Log($"{fighter.DisplayName} ramasse une attaque {pickup.Type}.");
    }

    private void SpawnPickups()
    {
        if (_activePickups.Count >= maxSimultaneousPickups)
        {
            return;
        }

        _spawnTimer += Time.fixedDeltaTime;
        if (_spawnTimer < spawnInterval)
        {
            return;
        }

        _spawnTimer = 0f;
        var randomType = UnityEngine.Random.value < 0.5f ? AttackPickupType.Attack : AttackPickupType.SpecialAttack;
        var prefab = randomType == AttackPickupType.Attack ? attackPickupPrefab : specialPickupPrefab;

        if (prefab == null)
        {
            Debug.LogWarning("Missing pickup prefab for spawned type.");
            return;
        }

        var spawnPosition = RandomPointInsideArena();
        var instance = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
        instance.Configure(randomType, arenaRadius * 0.05f);
        _activePickups.Add(instance);
    }

    private Vector3 RandomPointInsideArena()
    {
        for (var i = 0; i < 8; i++)
        {
            var randomOffset = UnityEngine.Random.insideUnitCircle * arenaRadius;
            if (randomOffset.sqrMagnitude < 0.001f)
            {
                randomOffset = Vector2.up * arenaRadius * 0.5f;
            }

            var candidate = ArenaCenterPosition + (Vector3)randomOffset;

            if (IsPointFarFromFighters(candidate))
            {
                return candidate;
            }
        }

        return ArenaCenterPosition;
    }

    private bool IsPointFarFromFighters(Vector3 point)
    {
        foreach (var fighter in fighters)
        {
            if (fighter == null || !fighter.IsRuntimeReady)
            {
                continue;
            }

            var distance = Vector2.Distance(point, fighter.GetPosition());
            if (distance < collisionDistance)
            {
                return false;
            }
        }

        return true;
    }

    private void ConstrainInsideArena(FighterSlot fighter)
    {
        if (fighter == null || !fighter.IsRuntimeReady)
        {
            return;
        }

        var offset = fighter.GetPosition() - ArenaCenterPosition;
        var magnitude = offset.magnitude;
        if (magnitude > arenaRadius)
        {
            fighter.SetPosition(ArenaCenterPosition + offset.normalized * arenaRadius);
        }
    }

    private Vector3 ArenaCenterPosition => arenaCenter != null ? arenaCenter.position : transform.position;
}

[Serializable]
public class FighterSlot
{
    [SerializeField] private string displayName;
    [SerializeField] private Transform transform;
    [SerializeField] private PokeUnit unitData;
    [SerializeField] private float baseSpeed = 4f;

    public float CurrentHealth { get; set; }
    public Vector2 CurrentVelocity { get; set; }
    public AttackPickupType EquippedPickup { get; private set; } = AttackPickupType.None;

    public string DisplayName => !string.IsNullOrWhiteSpace(displayName) ? displayName : unitData != null ? unitData.unitName : "Inconnu";
    public Transform Transform => transform;
    public PokeUnit UnitData => unitData;
    public float MoveSpeed
    {
        get
        {
            if (unitData != null && unitData.vitesse > 0)
            {
                return Mathf.Max(baseSpeed, unitData.vitesse * 0.05f);
            }

            return baseSpeed;
        }
    }
    public bool HasPickup => EquippedPickup != AttackPickupType.None;
    public bool IsRuntimeReady => transform != null;

    public Vector3 GetPosition()
    {
        if (transform is RectTransform rectTransform)
        {
            return rectTransform.position;
        }

        return transform.position;
    }

    public void SetPosition(Vector3 position)
    {
        if (transform is RectTransform rectTransform)
        {
            rectTransform.position = position;
        }
        else
        {
            transform.position = position;
        }
    }

    public void ResetRuntimeState()
    {
        CurrentHealth = unitData != null ? unitData.pv : 0f;
        CurrentVelocity = Vector2.zero;
        EquippedPickup = AttackPickupType.None;
    }

    public void EquipPickup(AttackPickupType pickupType)
    {
        EquippedPickup = pickupType;
    }

    public void ClearPickup()
    {
        if (EquippedPickup != AttackPickupType.None)
        {
            Debug.Log($"{DisplayName} perd son bonus d'attaque {EquippedPickup}.");
        }

        EquippedPickup = AttackPickupType.None;
    }

    public int GetDamageValue()
    {
        if (unitData == null)
        {
            return 0;
        }

        return EquippedPickup switch
        {
            AttackPickupType.Attack => Mathf.Max(1, unitData.attaque),
            AttackPickupType.SpecialAttack => Mathf.Max(1, unitData.attaqueSpeciale),
            _ => 0
        };
    }
}

public enum AttackPickupType
{
    None,
    Attack,
    SpecialAttack
}
