using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia os IDs de quests, mobs e itens no jogo.
/// Garante que os IDs estejam dentro dos intervalos especificados:
/// - Quests: 1-999
/// - Mobs: 1000-9999
/// - Itens: 10000-19999
/// </summary>
public class IDManager : MonoBehaviour
{
    // Singleton para acesso global
    public static IDManager Instance { get; private set; }
    
    // Dicionários para mapear objetos aos seus IDs
    private Dictionary<int, Quest> questIDMap = new Dictionary<int, Quest>();
    private Dictionary<int, GameObject> mobIDMap = new Dictionary<int, GameObject>();
    private Dictionary<int, Item> itemIDMap = new Dictionary<int, Item>();
    
    // Conjuntos para rastrear IDs já utilizados
    private HashSet<int> usedQuestIDs = new HashSet<int>();
    private HashSet<int> usedMobIDs = new HashSet<int>();
    private HashSet<int> usedItemIDs = new HashSet<int>();
    
    // Constantes para os intervalos de IDs
    public const int MIN_QUEST_ID = 1;
    public const int MAX_QUEST_ID = 999;
    
    public const int MIN_MOB_ID = 1000;
    public const int MAX_MOB_ID = 9999;
    
    public const int MIN_ITEM_ID = 10000;
    public const int MAX_ITEM_ID = 19999;
    
    private void Awake()
    {
        // Configuração do singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    /// <summary>
    /// Registra uma quest com um ID específico
    /// </summary>
    public bool RegisterQuest(Quest quest, int id)
    {
        // Verificar se o ID está no intervalo válido
        if (id < MIN_QUEST_ID || id > MAX_QUEST_ID)
        {
            Debug.LogError($"ID de quest inválido: {id}. Deve estar entre {MIN_QUEST_ID} e {MAX_QUEST_ID}");
            return false;
        }
        
        // Verificar se o ID já está em uso
        if (usedQuestIDs.Contains(id))
        {
            Debug.LogError($"ID de quest {id} já está em uso");
            return false;
        }
        
        // Registrar a quest
        questIDMap[id] = quest;
        usedQuestIDs.Add(id);
        return true;
    }
    
    /// <summary>
    /// Registra um mob com um ID específico
    /// </summary>
    public bool RegisterMob(GameObject mob, int id)
    {
        // Verificar se o ID está no intervalo válido
        if (id < MIN_MOB_ID || id > MAX_MOB_ID)
        {
            Debug.LogError($"ID de mob inválido: {id}. Deve estar entre {MIN_MOB_ID} e {MAX_MOB_ID}");
            return false;
        }
        
        // Verificar se o ID já está em uso
        if (usedMobIDs.Contains(id))
        {
            Debug.LogError($"ID de mob {id} já está em uso");
            return false;
        }
        
        // Registrar o mob
        mobIDMap[id] = mob;
        usedMobIDs.Add(id);
        return true;
    }
    
    /// <summary>
    /// Registra um item com um ID específico
    /// </summary>
    public bool RegisterItem(Item item, int id)
    {
        // Verificar se o ID está no intervalo válido
        if (id < MIN_ITEM_ID || id > MAX_ITEM_ID)
        {
            Debug.LogError($"ID de item inválido: {id}. Deve estar entre {MIN_ITEM_ID} e {MAX_ITEM_ID}");
            return false;
        }
        
        // Verificar se o ID já está em uso
        if (usedItemIDs.Contains(id))
        {
            Debug.LogError($"ID de item {id} já está em uso");
            return false;
        }
        
        // Registrar o item
        itemIDMap[id] = item;
        usedItemIDs.Add(id);
        return true;
    }
    
    /// <summary>
    /// Obtém uma quest pelo ID
    /// </summary>
    public Quest GetQuestByID(int id)
    {
        if (questIDMap.TryGetValue(id, out Quest quest))
        {
            return quest;
        }
        return null;
    }
    
    /// <summary>
    /// Obtém um mob pelo ID
    /// </summary>
    public GameObject GetMobByID(int id)
    {
        if (mobIDMap.TryGetValue(id, out GameObject mob))
        {
            return mob;
        }
        return null;
    }
    
    /// <summary>
    /// Obtém um item pelo ID
    /// </summary>
    public Item GetItemByID(int id)
    {
        if (itemIDMap.TryGetValue(id, out Item item))
        {
            return item;
        }
        return null;
    }
    
    /// <summary>
    /// Obtém o ID de uma quest
    /// </summary>
    public int GetQuestID(Quest quest)
    {
        foreach (var pair in questIDMap)
        {
            if (pair.Value == quest)
            {
                return pair.Key;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Obtém o ID de um mob
    /// </summary>
    public int GetMobID(GameObject mob)
    {
        foreach (var pair in mobIDMap)
        {
            if (pair.Value == mob)
            {
                return pair.Key;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Obtém o ID de um item
    /// </summary>
    public int GetItemID(Item item)
    {
        foreach (var pair in itemIDMap)
        {
            if (pair.Value == item)
            {
                return pair.Key;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Gera um novo ID de quest não utilizado
    /// </summary>
    public int GenerateNewQuestID()
    {
        for (int id = MIN_QUEST_ID; id <= MAX_QUEST_ID; id++)
        {
            if (!usedQuestIDs.Contains(id))
            {
                return id;
            }
        }
        Debug.LogError("Não há mais IDs de quest disponíveis!");
        return -1;
    }
    
    /// <summary>
    /// Gera um novo ID de mob não utilizado
    /// </summary>
    public int GenerateNewMobID()
    {
        for (int id = MIN_MOB_ID; id <= MAX_MOB_ID; id++)
        {
            if (!usedMobIDs.Contains(id))
            {
                return id;
            }
        }
        Debug.LogError("Não há mais IDs de mob disponíveis!");
        return -1;
    }
    
    /// <summary>
    /// Gera um novo ID de item não utilizado
    /// </summary>
    public int GenerateNewItemID()
    {
        for (int id = MIN_ITEM_ID; id <= MAX_ITEM_ID; id++)
        {
            if (!usedItemIDs.Contains(id))
            {
                return id;
            }
        }
        Debug.LogError("Não há mais IDs de item disponíveis!");
        return -1;
    }
}