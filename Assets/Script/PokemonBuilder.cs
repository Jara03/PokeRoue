#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Script;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PokemonBuilder : MonoBehaviour
{
    public WheelGenerator wheel;
    public SpinWheelManager spinWheelManager;
    [Header("Source")]
    [Tooltip("Le PokeUnit de base (issu du Pokédex originel)")]
    public PokeUnit baseUnit;

    [Header("Configuration de génération")]
    [Tooltip("Dossier de destination pour les nouvelles unités (dans Assets/)")]
    public string outputFolder = "Pokemon/GeneratedUnits";
    public string resourcePath = "Pokemon/";
    public string author;

    [Tooltip("Compteur interne pour nommer les unités")]
    public int createdCount = 0;
    private Pokeball pkb = Pokeball.Pokeball;
    private Arena arena = Arena.None;
    
    //le tuple nature produit un buff et un débuff de chaque coté du Tuple
    private string nature = null ;
    private string accelerator = null;
    private void Start()
    {
        selectPokeBall();
    }
    

    [ContextMenu("Generate New Unit")]

    public void selectPokeBall()
    {
     
            Debug.Log("Selection du PokeUnit de base : ");
            wheel.LoadSegments(getPokeBallList());
            spinWheelManager.OnSegmentSelected += OnPokeballSelected;
        
    }

    public void selectBaseUnit()
    {
        if (baseUnit == null)
        {
            Debug.Log("Selection du PokeUnit de base : ");
            spinWheelManager.OnSegmentSelected += OnBaseUnitSelected;
            wheel.LoadSegments(getPokeUnitList());
        }
    }

    public void selectNature()
    {
        if (nature == null)
        {
            Debug.Log("Selection de la nature de base : ");
            spinWheelManager.OnSegmentSelected += OnNatureSelected;
            wheel.LoadSegments(getNature());
        }  
    }

    public void selectArena()
    {
        if (arena == Arena.None)
        {
            Debug.Log("Selection de l'arene de base : ");
            spinWheelManager.OnSegmentSelected += OnArenaSelected;
            wheel.LoadSegments(getArena()); 
        }
    }

    public WheelSegment[] getPokeBallList()
    {
        //fait une liste de toutes les pokeball de PokeUnit
        List<WheelSegment> segments = new List<WheelSegment>();

        foreach (Pokeball pokeball in System.Enum.GetValues(typeof(Pokeball)))
        {
            WheelSegment segment = new WheelSegment
            {
                label = pokeball.ToString(),
                dropRate = 1f, // ou pondéré selon ton système
                color = Random.ColorHSV(),
                artwork = Resources.Load<Sprite>("Balls/" + pokeball.ToString()) // charger l'image de l'artwork
            };

            segments.Add(segment);
        }

        Debug.Log($"✅ {segments.Count} PokeBall chargés depuis {resourcePath}");
        return segments.ToArray();

    }
    public WheelSegment[] getPokeUnitList()
    {
        // 🔍 Charge tous les PokeUnits depuis Resources/Pokemon
        PokeUnit[] pokeUnits = Resources.LoadAll<PokeUnit>(resourcePath);
        
        // filtrage des pokeball
        PokeUnit[] filteredUnits = pokeUnits
            .Where(u => (u.pokeball & pkb) != 0)
            .ToArray();

        
        List<WheelSegment> segments = new List<WheelSegment>();

        foreach (var pokeUnit in filteredUnits)
        {
            if (pokeUnit == null) continue;

            WheelSegment segment = new WheelSegment
            {
                label = pokeUnit.unitName,
                dropRate = 1f, // ou pondéré selon ton système
                color = GetColorFromType(pokeUnit.type)
            };

            segments.Add(segment);
        }

        Debug.Log($"✅ {segments.Count} PokeUnits chargés depuis {resourcePath}");
        return segments.ToArray();
    }


    public WheelSegment[] getNature()
    {
        //fait une liste de toutes les pokeball de PokeUnit
        List<WheelSegment> segments = new List<WheelSegment>();

        foreach (Nature nat in System.Enum.GetValues(typeof(Nature)))
        {
            if (nat == Nature.None) continue;

            WheelSegment segment = new WheelSegment
            {
                label = nat.ToString(),
                dropRate = 1f,
                color = Random.ColorHSV(),
            };
            
            segments.Add(segment);

        }
        
        Debug.Log($"{{segments.Count}} Natures chargés depuis {resourcePath}");
        return segments.ToArray();

    }

    public WheelSegment[] getArena()
    {
        //fait une liste de toutes les pokeball de PokeUnit
        List<WheelSegment> segments = new List<WheelSegment>();

        foreach (Arena arena in System.Enum.GetValues(typeof(Arena)))
        {
            if (arena == Arena.None) continue;
            WheelSegment segment = new WheelSegment
            {
                label = arena.ToString(),
                dropRate = 1f,
                color = Random.ColorHSV(),
            };
            
            segments.Add(segment);
        }
        
        Debug.Log($"{{segments.Count}} Arenes chargés depuis {resourcePath}");
        return segments.ToArray();
    }
    
    private void OnPokeballSelected(WheelSegment segment)
    {
        spinWheelManager.OnSegmentSelected -= OnPokeballSelected;
        pkb = (Pokeball)Enum.Parse(typeof(Pokeball), segment.label);
        Debug.Log($"✅ Pokéball choisie : {segment.label}");
        selectBaseUnit(); // continue le flux
    }

    private void OnBaseUnitSelected(WheelSegment segment)
    {
        spinWheelManager.OnSegmentSelected -= OnBaseUnitSelected;
        baseUnit = Resources
            .LoadAll<PokeUnit>(resourcePath)
            .FirstOrDefault(u => u.unitName == segment.label);
        Debug.Log($"✅ Base unit choisie : {baseUnit.unitName}");
        selectNature(); // continue
    }

    private void OnNatureSelected(WheelSegment segment)
    {
        spinWheelManager.OnSegmentSelected -= OnNatureSelected;
        nature = segment.label;
        Debug.Log($"✅ Nature choisie : {segment.label}");
        selectArena(); // continue
    }

    private void OnArenaSelected(WheelSegment segment)
    {
        spinWheelManager.OnSegmentSelected -= OnArenaSelected;
        arena = (Arena)Enum.Parse(typeof(Arena),segment.label);
        Debug.Log($"✅ Arene choisie : {segment.label}");
        selectAccelerateur(); // continue

    }

    public void selectAccelerateur()
    {
        Debug.Log("Selection de l'accelerateur : ");
        spinWheelManager.OnSegmentSelected += OnAccelerateurSelected;
        wheel.LoadSegments(getAccelerateurList());
    }

    private void CreateClone()
    {
        // ici, le code de création du nouveau PokeUnit
        PokeUnit newUnit = ScriptableObject.CreateInstance<PokeUnit>();

        newUnit.unitName = $"{baseUnit.unitName}_Clone_{author}";
        newUnit.artwork = baseUnit.artwork;
        newUnit.type = baseUnit.type;
        newUnit.PokeType2 = baseUnit.PokeType2;
        newUnit.pokeball = baseUnit.pokeball;
        newUnit.nature = (Nature)Enum.Parse(typeof(Nature), nature);
        
        
        newUnit.pv = baseUnit.pv;
        newUnit.attaque = baseUnit.attaque;
        newUnit.defense = baseUnit.defense;
        newUnit.attaqueSpeciale = baseUnit.attaqueSpeciale;
        newUnit.defenseSpeciale = baseUnit.defenseSpeciale;
        newUnit.vitesse = baseUnit.vitesse;

        setNature(newUnit);
        ApplyAccelerator(newUnit);

        string assetPath = $"Assets/Resources/{outputFolder}/{newUnit.unitName}.asset";

        #if UNITY_EDITOR
                AssetDatabase.CreateAsset(newUnit, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"✅ Nouveau PokeUnit créé : {assetPath}");
        #else
            Debug.LogWarning("⚠️ La génération de ScriptableObjects n’est possible que dans l’éditeur Unity.");
        #endif

        createdCount++;
    }

    private void OnAccelerateurSelected(WheelSegment segment)
    {
        spinWheelManager.OnSegmentSelected -= OnAccelerateurSelected;
        accelerator = segment.label;
        Debug.Log($"✅ Accélérateur choisi : {segment.label}");
        CreateClone();
    }

    public WheelSegment[] getAccelerateurList()
    {
        List<WheelSegment> segments = new List<WheelSegment>
        {
            new WheelSegment { label = "attaque", dropRate = 0.05f, color = Random.ColorHSV() },
            new WheelSegment { label = "défense", dropRate = 0.05f, color = Random.ColorHSV() },
            new WheelSegment { label = "None1", dropRate = 0.10f, color = Random.ColorHSV() },
            new WheelSegment { label = "attaqueSpeciale", dropRate = 0.05f, color = Random.ColorHSV() },
            new WheelSegment { label = "défenseSpeciale", dropRate = 0.05f, color = Random.ColorHSV() },
            new WheelSegment { label = "None2", dropRate = 0.10f, color = Random.ColorHSV() },
            new WheelSegment { label = "vitesse", dropRate = 0.05f, color = Random.ColorHSV() },
            new WheelSegment { label = "pv", dropRate = 0.05f, color = Random.ColorHSV() },
            new WheelSegment { label = "None3", dropRate = 0.10f, color = Random.ColorHSV() }
        };

        return segments.ToArray();
    }

    private void ApplyAccelerator(PokeUnit newUnit)
    {
        if (string.IsNullOrEmpty(accelerator) || accelerator.StartsWith("None"))
        {
            return;
        }

        int bonus;
        switch (accelerator)
        {
            case "attaque":
                bonus = Mathf.RoundToInt(newUnit.attaque * 0.1f);
                newUnit.attaque += bonus;
                break;
            case "défense":
                bonus = Mathf.RoundToInt(newUnit.defense * 0.1f);
                newUnit.defense += bonus;
                break;
            case "attaqueSpeciale":
                bonus = Mathf.RoundToInt(newUnit.attaqueSpeciale * 0.1f);
                newUnit.attaqueSpeciale += bonus;
                break;
            case "défenseSpeciale":
                bonus = Mathf.RoundToInt(newUnit.defenseSpeciale * 0.1f);
                newUnit.defenseSpeciale += bonus;
                break;
            case "vitesse":
                bonus = Mathf.RoundToInt(newUnit.vitesse * 0.1f);
                newUnit.vitesse += bonus;
                break;
            case "pv":
                bonus = Mathf.RoundToInt(newUnit.pv * 0.1f);
                newUnit.pv += bonus;
                break;
        }
    }

    private void setNature(PokeUnit newUnit)
    {
        switch (newUnit.nature)
        {
            case Nature.Hardy:
                break;
            case Nature.Lonely:
                newUnit.attaque += (baseUnit.attaque/10);
                newUnit.defense -= (baseUnit.defense/10);
                break;
            case Nature.Brave:
                newUnit.attaque += (baseUnit.attaque/10);
                newUnit.vitesse -= (baseUnit.vitesse/10);
                break;
            case Nature.Adamant:
                newUnit.attaque += (baseUnit.attaque/10);
                newUnit.attaqueSpeciale -= (baseUnit.attaqueSpeciale/10);
                break;
            case Nature.Naughty:
                newUnit.attaque += (baseUnit.attaque/10);
                newUnit.defenseSpeciale -= (baseUnit.defenseSpeciale/10);
                break;
            case Nature.Bold:
                newUnit.defense += (baseUnit.defense/10);
                newUnit.attaque -= (baseUnit.attaque/10);
                break;
            case Nature.Impish:
                newUnit.defense += (baseUnit.defense/10);
                newUnit.attaqueSpeciale -= (baseUnit.attaqueSpeciale/10);
                break;
            case Nature.Lax:
                newUnit.defense += (baseUnit.defense/10);
                newUnit.defenseSpeciale -= (baseUnit.defenseSpeciale/10);
                break;
            case Nature.Relaxed:
                newUnit.defense += (baseUnit.defense/10);
                newUnit.vitesse -= (baseUnit.vitesse/10);
                break;
            case Nature.Modest:
                newUnit.attaqueSpeciale += (baseUnit.attaqueSpeciale/10);
                newUnit.attaque -= (baseUnit.attaque/10);
                break;
            case Nature.Mild:
                newUnit.attaqueSpeciale += (baseUnit.attaqueSpeciale/10);
                newUnit.defense -= (baseUnit.defense/10);
                break;
            case Nature.Quiet:
                newUnit.attaqueSpeciale += (baseUnit.attaqueSpeciale/10);
                newUnit.vitesse -= (baseUnit.vitesse/10);
                break;
            case Nature.Rash:
                newUnit.attaqueSpeciale += (baseUnit.attaqueSpeciale/10);
                newUnit.defense -= (baseUnit.defense/10);
                break;
            case Nature.Calm:
                newUnit.defenseSpeciale += (baseUnit.defenseSpeciale/10);
                newUnit.attaque -= (baseUnit.attaque/10);
                break;
            case Nature.Gentle:
                newUnit.defenseSpeciale += (baseUnit.defenseSpeciale/10);
                newUnit.defense -= (baseUnit.defense/10);
                break;
            case Nature.Sassy:
                newUnit.defenseSpeciale += (baseUnit.defenseSpeciale/10);
                newUnit.vitesse -= (baseUnit.vitesse/10);
                break;
            case Nature.Careful:
                newUnit.defenseSpeciale += (baseUnit.defenseSpeciale/10);
                newUnit.attaqueSpeciale -= (baseUnit.attaqueSpeciale/10);
                break;
            case Nature.Timid:
                newUnit.vitesse += (baseUnit.vitesse/10);
                newUnit.attaque -= (baseUnit.attaque/10);
                break;
            case Nature.Hasty:
                newUnit.vitesse += (baseUnit.vitesse/10);
                newUnit.defense -= (baseUnit.defense/10);
                break;
            case Nature.Jolly:
                newUnit.vitesse += (baseUnit.vitesse/10);
                newUnit.attaqueSpeciale -= (baseUnit.attaqueSpeciale/10);
                break;
            case Nature.Naive:
                newUnit.vitesse += (baseUnit.vitesse/10);
                newUnit.defenseSpeciale -= (baseUnit.defenseSpeciale/10);
                break;
        }
    }
    


    private Color GetColorFromType(PokeType type)
    {
        switch (type)
        {
            case PokeType.Normal:   return new Color(0.78f, 0.78f, 0.62f);   // Beige-gris
            case PokeType.Feu:      return new Color(1f, 0.4f, 0.2f);        // Orange vif
            case PokeType.Eau:      return new Color(0.2f, 0.5f, 1f);        // Bleu océan
            case PokeType.Plante:   return new Color(0.3f, 0.8f, 0.3f);      // Vert feuille
            case PokeType.Électric: return new Color(1f, 0.9f, 0.2f);        // Jaune vif
            case PokeType.Poison:   return new Color(0.6f, 0.2f, 0.7f);      // Violet toxique
            case PokeType.Roche:    return new Color(0.6f, 0.5f, 0.4f);      // Brun pierre
            case PokeType.Sol:      return new Color(0.82f, 0.7f, 0.35f);    // Brun sable
            case PokeType.Vol:      return new Color(0.55f, 0.7f, 1f);       // Bleu ciel
            case PokeType.Glace:    return new Color(0.6f, 0.9f, 1f);        // Bleu glacé
            case PokeType.Combat:   return new Color(0.8f, 0.3f, 0.3f);      // Rouge brique
            case PokeType.Psy:      return new Color(0.9f, 0.2f, 0.9f);      // Rose-violet
            case PokeType.Insecte:  return new Color(0.55f, 0.75f, 0.25f);    // Vert olive
            case PokeType.Spectre:  return new Color(0.5f, 0.3f, 0.8f);      // Violet profond
            case PokeType.Dragon:   return new Color(0.3f, 0.2f, 0.9f);      // Bleu indigo
            case PokeType.Ténèbres: return new Color(0.2f, 0.2f, 0.2f);      // Gris anthracite
            case PokeType.Fée:      return new Color(1f, 0.6f, 0.9f);        // Rose pastel
            case PokeType.Acier:    return new Color(0.7f, 0.7f, 0.8f);      // Gris métal
            default:                return new Color(0.5f, 0.5f, 0.5f);     // Gris clair;
        }

    }
    
}
