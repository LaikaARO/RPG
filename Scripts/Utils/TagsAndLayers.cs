using UnityEngine;

/// <summary>
/// Classe utilitária para gerenciar tags e layers do projeto.
/// Centraliza as constantes de tags e layers para evitar erros de digitação.
/// </summary>
public static class TagsAndLayers
{
    // Tags
    public static readonly string PLAYER_TAG = "Player";
    public static readonly string ENEMY_TAG = "Enemy";
    public static readonly string GROUND_TAG = "Ground";
    public static readonly string INTERACTABLE_TAG = "Interactable";
    public static readonly string NPC_TAG = "NPC";
    
    // Layers
    public static readonly int PLAYER_LAYER = LayerMask.NameToLayer("Player");
    public static readonly int ENEMY_LAYER = LayerMask.NameToLayer("Enemy");
    public static readonly int GROUND_LAYER = LayerMask.NameToLayer("Ground");
    public static readonly int INTERACTABLE_LAYER = LayerMask.NameToLayer("Interactable");
    public static readonly int NPC_LAYER = LayerMask.NameToLayer("NPC");
    
    // Layer Masks
    public static readonly int GROUND_LAYER_MASK = 1 << GROUND_LAYER;
    public static readonly int ENEMY_LAYER_MASK = 1 << ENEMY_LAYER;
    public static readonly int INTERACTABLE_LAYER_MASK = 1 << INTERACTABLE_LAYER;
    
    /// <summary>
    /// Verifica se as tags e layers necessárias estão configuradas no projeto.
    /// </summary>
    public static void ValidateTagsAndLayers()
    {
        // Lista de tags necessárias
        string[] requiredTags = new string[] 
        { 
            PLAYER_TAG, 
            ENEMY_TAG, 
            GROUND_TAG, 
            INTERACTABLE_TAG, 
            NPC_TAG 
        };
        
        // Lista de layers necessárias
        string[] requiredLayers = new string[] 
        { 
            "Player", 
            "Enemy", 
            "Ground", 
            "Interactable", 
            "NPC" 
        };
        
        // Verificar tags
        foreach (string tag in requiredTags)
        {
            bool tagExists = false;
            foreach (string existingTag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (existingTag == tag)
                {
                    tagExists = true;
                    break;
                }
            }
            
            if (!tagExists)
            {
                Debug.LogWarning($"A tag '{tag}' não está configurada no projeto. Adicione-a em Edit > Project Settings > Tags and Layers.");
            }
        }
        
        // Verificar layers
        foreach (string layer in requiredLayers)
        {
            if (LayerMask.NameToLayer(layer) == -1)
            {
                Debug.LogWarning($"A layer '{layer}' não está configurada no projeto. Adicione-a em Edit > Project Settings > Tags and Layers.");
            }
        }
    }
}

