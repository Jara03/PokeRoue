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
    public int pv = 0;

    [Tooltip("Puissance des attaques physiques")]
    public int attaque = 0;

    [Tooltip("Défense contre les attaques ennemies")]
    public int defense = 0;

    [Tooltip("Puissance des attaques spéciales")]
    public int attaqueSpeciale = 0;
    
    [Tooltip("Défense contre les attaques ennemies")]
    public int defenseSpeciale = 0;

    [Tooltip("Vitesse d'exécution ou d'esquive")]
    public int vitesse = 0;

    
    [Tooltip("Nature du Pokemon")]
    public Nature nature = Nature.None;
    
    [Header("Type élémentaire")]
    [Tooltip("Type principal de la créature (ex: Feu, Eau, Plante, etc.)")]
    public PokeType type = PokeType.Normal;
    public PokeType2 PokeType2 = PokeType2.Normal;
    
    [Header("Type de PokeBall")]
    public Pokeball pokeball = Pokeball.Pokeball;
}

public enum PokeType
{
    Normal,
    Feu,
    Eau,
    Plante,
    Électric,
    Poison,
    Roche,
    Sol,
    Vol,
    Glace,
    Combat,
    Psy,
    Insecte,
    Spectre,
    Dragon,
    Ténèbres,
    Fée,
    Acier,
}

public enum PokeType2
{
    None,
    Normal,
    Feu,
    Eau,
    Plante,
    Électrik,
    Poison,
    Roche,
    Sol,
    Vol,
    Glace,
    Combat,
    Psy,
    Insecte,
    Spectre,
    Dragon,
    Ténèbres,
    Fée,
    Acier,
}

[System.Flags] 
public enum Pokeball
{
    PremierBall = 0,
    Pokeball = 1 << 1,
    GreatBall = 1 << 2,
    UltraBall = 1 << 3,
    MasterBall = 1 << 4,
    LuxuryBall = 1 << 5,
    SafaryBall = 1 << 6,
   // CherishBall = 1 << 6,
   // BeastBall = 1 << 7,
    StrangeBall = 1 << 7,
}

public enum Nature
{
    None,
    Lonely,
    Brave,
    Adamant,
    Naughty,
    Bold,
    Docile,
    Relaxed,
    Impish,
    Lax,
    Timid,
    Hasty,
    Serious,
    Modest,
    Mild,
    Quiet,
    Bashful,
    Rash,
    Calm,
    Gentle,
    Sassy,
    Careful,
    Quirky,
    Hardy,
    Jolly,
    Naive,
}