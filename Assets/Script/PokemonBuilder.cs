#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
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
    
    //le tuple nature produit un buff et un débuff de chaque coté du Tuple
    private string nature = null ;
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

    public WheelSegment[] getPokeBallList()
    {
        //fait une liste de toutes les pokeball de PokeUnit
        List<WheelSegment> segments = new List<WheelSegment>();

        foreach (Pokeball pokeball in System.Enum.GetValues(typeof(Pokeball)))
        {
            WheelSegment segment = new WheelSegment
            {
                //label = pokeball.ToString(),
                dropRate = 1f, // ou pondéré selon ton système
                color = Random.ColorHSV(),
                artwork = Resources.Load<Image>(resourcePath + pokeball.ToString()) // charger l'image de l'artwork
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
        
        Debug.Log($"{{segments.Count}} PokeType chargés depuis {resourcePath}");
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
        CreateClone(); // continue
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
