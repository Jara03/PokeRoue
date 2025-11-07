using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPokeUnit",
    menuName = "Game Data/PokeUnit",
    order = 1)]
public class PokeUnit : ScriptableObject
{
    [Header("Informations Générales")]
    [Tooltip("Nom de l'unité ou créature")]
    public string unitName = "Nom par défaut";

    [Tooltip("Sprite principal de la créature")]
    public Sprite artwork;

    [Header("Statistiques de base")]
    [Tooltip("Points de vie")]
    public int pv = 100;

    [Tooltip("Puissance des attaques spéciales")]
    public int attaqueSpeciale = 50;

    [Tooltip("Défense contre les attaques ennemies")]
    public int defense = 40;
    
    [Tooltip("Défense contre les attaques ennemies")]
    public int defenseSpeciale = 40;

    [Tooltip("Vitesse d'exécution ou d'esquive")]
    public int vitesse = 60;

    [Header("Type élémentaire")]
    [Tooltip("Type principal de la créature (ex: Feu, Eau, Plante, etc.)")]
    public PokeType type = PokeType.Normal;
}

public enum PokeType
{
    Normal,
    Feu,
    Eau,
    Plante,
    Électrik,
    Roche,
    Sol,
    Glace,
    Combat,
    Psy,
    Spectre,
    Dragon,
    Ténèbres,
    Fée
}