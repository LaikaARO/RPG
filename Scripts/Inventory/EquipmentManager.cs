using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.Assertions;
#endif

/// <summary>
/// Gerencia os equipamentos do jogador e seus efeitos nas estatísticas
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    [Header("Slots de Equipamento")]
    [SerializeField] private Dictionary<EquipmentSlot, EquipmentItem> equippedItems = new Dictionary<EquipmentSlot, EquipmentItem>();
    
    // Cache para cálculos de estatísticas
    private Dictionary<string, float> statsCache = new Dictionary<string, float>();
    private bool statsCacheDirty = true;
    
    [Header("Pontos de Equipamento Visual")]
    [SerializeField] private Transform mainHandPoint;
    [SerializeField] private Transform offHandPoint;
    [SerializeField] private Transform helmetPoint;
    [SerializeField] private Transform chestPoint;
    [SerializeField] private Transform legsPoint;
    [SerializeField] private Transform bootsPoint;
    [SerializeField] private Transform glovesPoint;
    
    // Referências aos componentes do jogador
    private CharacterStats playerStats;
    private LevelSystem playerLevelSystem;
    
    // Eventos
    public event Action<EquipmentSlot, EquipmentItem> OnItemEquipped;
    public event Action<EquipmentSlot, EquipmentItem> OnItemUnequipped;
    public event Action OnEquipmentChanged;
    
    // Singleton
    public static EquipmentManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeEquipmentSlots();
            InitializeStatsCache();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        try
        {
            InitializeComponents();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao inicializar EquipmentManager: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Inicializa os componentes necessários
    /// </summary>
    private void InitializeComponents()
    {
        // Obter referências aos componentes do jogador
        playerStats = GetComponent<CharacterStats>();
        playerLevelSystem = GetComponent<LevelSystem>();
        
        // Validar componentes críticos
        if (playerStats == null)
        {
            Debug.LogError("EquipmentManager: CharacterStats não encontrado no mesmo GameObject!");
        }
        
        if (playerLevelSystem == null)
        {
            Debug.LogError("EquipmentManager: LevelSystem não encontrado no mesmo GameObject!");
        }
        
        #if UNITY_EDITOR
        // Validações adicionais no editor
        ValidateEquipmentPoints();
        #endif
    }
    
    /// <summary>
    /// Inicializa o cache de estatísticas
    /// </summary>
    private void InitializeStatsCache()
    {
        statsCache["physicalDefense"] = 0;
        statsCache["magicalDefense"] = 0;
        statsCache["attackPower"] = 0;
        statsCache["magicPower"] = 0;
        statsCacheDirty = true;
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Valida os pontos de equipamento no editor
    /// </summary>
    private void ValidateEquipmentPoints()
    {
        // Verificar pontos de equipamento críticos
        Assert.IsNotNull(mainHandPoint, "Ponto de equipamento da mão principal não configurado!");
        Assert.IsNotNull(chestPoint, "Ponto de equipamento do peitoral não configurado!");
    }
    #endif

    /// <summary>
    /// Inicializa todos os slots de equipamento
    /// </summary>
    private void InitializeEquipmentSlots()
    {
        // Inicializar todos os slots como vazios
        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            equippedItems[slot] = null;
        }
    }

    /// <summary>
    /// Equipa um item
    /// </summary>
    /// <param name="item">Item a ser equipado</param>
    /// <returns>Item que foi desequipado (se houver)</returns>
    public EquipmentItem EquipItem(EquipmentItem item)
    {
        if (item == null) return null;
        
        try
        {
            // Verificar se o jogador pode equipar o item
            if (playerStats == null || playerLevelSystem == null || !item.CanEquip(playerStats, playerLevelSystem))
            {
                Debug.LogWarning($"Não foi possível equipar {item.itemName}: requisitos não atendidos");
                return null;
            }
            
            EquipmentSlot targetSlot = item.equipmentSlot;
            EquipmentItem previousItem = null;
            
            // Se já há algo equipado neste slot, desequipar primeiro
            if (equippedItems.ContainsKey(targetSlot) && equippedItems[targetSlot] != null)
            {
                previousItem = UnequipItem(targetSlot);
            }
            
            // Equipar o novo item
            equippedItems[targetSlot] = item;
            
            // Aplicar os bônus do equipamento
            ApplyEquipmentBonuses(item, true);
            
            // Atualizar o visual do equipamento
            UpdateEquipmentVisual(targetSlot, item);
            
            // Remover o item do inventário
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.RemoveItem(item, 1);
            }
            
            // Marcar cache como desatualizado
            statsCacheDirty = true;
            
            // Disparar eventos
            OnItemEquipped?.Invoke(targetSlot, item);
            OnEquipmentChanged?.Invoke();
            
            Debug.Log($"Equipado: {item.itemName} no slot {targetSlot}");
            
            return previousItem;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao equipar item {item.itemName}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Desequipa um item de um slot específico
    /// </summary>
    /// <param name="slot">Slot do equipamento</param>
    /// <returns>Item que foi desequipado</returns>
    public EquipmentItem UnequipItem(EquipmentSlot slot)
    {
        if (!equippedItems.ContainsKey(slot) || equippedItems[slot] == null)
        {
            return null;
        }
        
        try
        {
            EquipmentItem item = equippedItems[slot];
            
            // Remover os bônus do equipamento
            ApplyEquipmentBonuses(item, false);
            
            // Remover o item do slot
            equippedItems[slot] = null;
            
            // Remover o visual do equipamento
            UpdateEquipmentVisual(slot, null);
            
            // Adicionar o item de volta ao inventário
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(item, 1);
            }
            
            // Marcar cache como desatualizado
            statsCacheDirty = true;
            
            // Disparar eventos
            OnItemUnequipped?.Invoke(slot, item);
            OnEquipmentChanged?.Invoke();
            
            Debug.Log($"Desequipado: {item.itemName} do slot {slot}");
            
            return item;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao desequipar item do slot {slot}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Aplica ou remove os bônus de um equipamento
    /// </summary>
    /// <param name="item">Item do equipamento</param>
    /// <param name="apply">True para aplicar, false para remover</param>
    /// <returns>True se os bônus foram aplicados com sucesso</returns>
    private bool ApplyEquipmentBonuses(EquipmentItem item, bool apply)
    {
        if (item == null || playerStats == null) return false;
        
        try
        {
            int multiplier = apply ? 1 : -1;
            
            // Aplicar bônus de atributos
            playerStats.AddStrengthModifier(item.strengthBonus * multiplier);
            playerStats.AddDexterityModifier(item.dexterityBonus * multiplier);
            playerStats.AddIntelligenceModifier(item.intelligenceBonus * multiplier);
            playerStats.AddVitalityModifier(item.vitalityBonus * multiplier);
            
            // Aplicar bônus de vida e mana se o CharacterStats suportar
            if (item.healthBonus != 0)
            {
                // Aqui poderia ser implementado um método para modificar a vida máxima
                // playerStats.AddMaxHealthModifier(item.healthBonus * multiplier);
            }
            
            if (item.manaBonus != 0)
            {
                // Aqui poderia ser implementado um método para modificar a mana máxima
                // playerStats.AddMaxManaModifier(item.manaBonus * multiplier);
            }
            
            // Marcar cache como desatualizado
            statsCacheDirty = true;
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao aplicar bônus do equipamento: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Atualiza o visual do equipamento
    /// </summary>
    /// <param name="slot">Slot do equipamento</param>
    /// <param name="item">Item (null para remover)</param>
    /// <returns>True se o visual foi atualizado com sucesso</returns>
    private bool UpdateEquipmentVisual(EquipmentSlot slot, EquipmentItem item)
    {
        try
        {
            Transform equipmentPoint = GetEquipmentPoint(slot);
            if (equipmentPoint == null) 
            {
                Debug.LogWarning($"Ponto de equipamento não encontrado para o slot {slot}");
                return false;
            }
            
            // Remover modelo antigo
            foreach (Transform child in equipmentPoint)
            {
                Destroy(child.gameObject);
            }
            
            // Adicionar novo modelo se houver
            if (item != null && item.equipmentModel != null)
            {
                GameObject model = Instantiate(item.equipmentModel, equipmentPoint);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao atualizar visual do equipamento: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtém o ponto de ancoragem para um slot de equipamento
    /// </summary>
    /// <param name="slot">Slot do equipamento</param>
    /// <returns>Transform do ponto de ancoragem</returns>
    private Transform GetEquipmentPoint(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.MainHand: return mainHandPoint;
            case EquipmentSlot.OffHand: return offHandPoint;
            case EquipmentSlot.Helmet: return helmetPoint;
            case EquipmentSlot.Chest: return chestPoint;
            case EquipmentSlot.Legs: return legsPoint;
            case EquipmentSlot.Boots: return bootsPoint;
            case EquipmentSlot.Gloves: return glovesPoint;
            default: return null;
        }
    }

    /// <summary>
    /// Retorna o item equipado em um slot específico
    /// </summary>
    /// <param name="slot">Slot do equipamento</param>
    /// <returns>Item equipado ou null</returns>
    public EquipmentItem GetEquippedItem(EquipmentSlot slot)
    {
        return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
    }

    /// <summary>
    /// Retorna todos os itens equipados
    /// </summary>
    /// <returns>Dicionário com todos os equipamentos</returns>
    public Dictionary<EquipmentSlot, EquipmentItem> GetAllEquippedItems()
    {
        return new Dictionary<EquipmentSlot, EquipmentItem>(equippedItems);
    }

    /// <summary>
    /// Verifica se um slot está vazio
    /// </summary>
    /// <param name="slot">Slot a verificar</param>
    /// <returns>True se vazio</returns>
    public bool IsSlotEmpty(EquipmentSlot slot)
    {
        return !equippedItems.ContainsKey(slot) || equippedItems[slot] == null;
    }

    /// <summary>
    /// Atualiza o cache de estatísticas se necessário
    /// </summary>
    private void UpdateStatsCache()
    {
        if (!statsCacheDirty) return;
        
        try
        {
            // Resetar valores
            statsCache["physicalDefense"] = 0;
            statsCache["magicalDefense"] = 0;
            statsCache["attackPower"] = 0;
            statsCache["magicPower"] = 0;
            
            // Calcular novos valores
            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    statsCache["physicalDefense"] += kvp.Value.physicalDefense;
                    statsCache["magicalDefense"] += kvp.Value.magicalDefense;
                    statsCache["attackPower"] += kvp.Value.attackPowerBonus;
                    statsCache["magicPower"] += kvp.Value.magicPowerBonus;
                }
            }
            
            statsCacheDirty = false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao atualizar cache de estatísticas: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula o total de defesa física de todos os equipamentos
    /// </summary>
    /// <returns>Defesa física total</returns>
    public int GetTotalPhysicalDefense()
    {
        UpdateStatsCache();
        return (int)statsCache["physicalDefense"];
    }

    /// <summary>
    /// Calcula o total de defesa mágica de todos os equipamentos
    /// </summary>
    /// <returns>Defesa mágica total</returns>
    public int GetTotalMagicalDefense()
    {
        UpdateStatsCache();
        return (int)statsCache["magicalDefense"];
    }

    /// <summary>
    /// Calcula o total de poder de ataque de todos os equipamentos
    /// </summary>
    /// <returns>Poder de ataque total</returns>
    public float GetTotalAttackPower()
    {
        UpdateStatsCache();
        return statsCache["attackPower"];
    }

    /// <summary>
    /// Calcula o total de poder mágico de todos os equipamentos
    /// </summary>
    /// <returns>Poder mágico total</returns>
    public float GetTotalMagicPower()
    {
        UpdateStatsCache();
        return statsCache["magicPower"];
    }

    /// <summary>
    /// Força o recálculo de todas as estatísticas
    /// </summary>
    /// <returns>True se o recálculo foi bem-sucedido</returns>
    public bool RefreshEquipmentBonuses()
    {
        if (playerStats == null) return false;
        
        try
        {
            // Remover todos os modificadores atuais
            playerStats.ClearAllModifiers();
            
            // Reaplicar todos os bônus
            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    ApplyEquipmentBonuses(kvp.Value, true);
                }
            }
            
            // Marcar cache como desatualizado
            statsCacheDirty = true;
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao recalcular bônus de equipamento: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Limpa os eventos ao destruir o objeto para evitar memory leaks
    /// </summary>
    private void OnDestroy()
    {
        OnItemEquipped = null;
        OnItemUnequipped = null;
        OnEquipmentChanged = null;
        
        // Limpar referência singleton
        if (Instance == this)
        {
            Instance = null;
        }
    }
}