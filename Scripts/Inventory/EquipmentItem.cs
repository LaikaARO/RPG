using UnityEngine;

/// <summary>
/// Item de equipamento que pode ser equipado pelo jogador (armas, armaduras, acessórios)
/// </summary>
[CreateAssetMenu(fileName = "New Equipment", menuName = "RPG/Equipment Item")]
public class EquipmentItem : Item
{
    [Header("Configurações de Equipamento")]
    [Tooltip("Tipo de equipamento")]
    public EquipmentType equipmentType;
    
    [Tooltip("Slot específico onde o item pode ser equipado")]
    public EquipmentSlot equipmentSlot;
    
    [Header("Bônus de Atributos")]
    [Tooltip("Bônus de força")]
    public int strengthBonus = 0;
    
    [Tooltip("Bônus de destreza")]
    public int dexterityBonus = 0;
    
    [Tooltip("Bônus de inteligência")]
    public int intelligenceBonus = 0;
    
    [Tooltip("Bônus de vitalidade")]
    public int vitalityBonus = 0;
    
    [Header("Bônus de Combat")]
    [Tooltip("Bônus de poder de ataque")]
    public float attackPowerBonus = 0;
    
    [Tooltip("Bônus de poder mágico")]
    public float magicPowerBonus = 0;
    
    [Tooltip("Bônus de velocidade de ataque")]
    public float attackSpeedBonus = 0;
    
    [Tooltip("Bônus de vida máxima")]
    public int healthBonus = 0;
    
    [Tooltip("Bônus de mana máxima")]
    public int manaBonus = 0;
    
    [Header("Defesa")]
    [Tooltip("Defesa física")]
    public int physicalDefense = 0;
    
    [Tooltip("Defesa mágica")]
    public int magicalDefense = 0;
    
    [Header("Requisitos")]
    [Tooltip("Nível mínimo necessário")]
    public int requiredLevel = 1;
    
    [Tooltip("Força mínima necessária")]
    public int requiredStrength = 0;
    
    [Tooltip("Destreza mínima necessária")]
    public int requiredDexterity = 0;
    
    [Tooltip("Inteligência mínima necessária")]
    public int requiredIntelligence = 0;
    
    [Header("Visual")]
    [Tooltip("Prefab do modelo 3D do equipamento")]
    public GameObject equipmentModel;

    /// <summary>
    /// Verifica se o jogador atende aos requisitos para equipar este item
    /// </summary>
    /// <param name="stats">Estatísticas do personagem</param>
    /// <param name="levelSystem">Sistema de nível do personagem</param>
    /// <returns>True se pode equipar</returns>
    public bool CanEquip(CharacterStats stats, LevelSystem levelSystem)
    {
        if (stats == null || levelSystem == null) return false;
        
        // Verificar nível
        if (levelSystem.Level < requiredLevel)
        {
            Debug.Log($"Nível insuficiente. Necessário: {requiredLevel}, Atual: {levelSystem.Level}");
            return false;
        }
        
        // Verificar atributos
        if (stats.Strength < requiredStrength)
        {
            Debug.Log($"Força insuficiente. Necessária: {requiredStrength}, Atual: {stats.Strength}");
            return false;
        }
        
        if (stats.Dexterity < requiredDexterity)
        {
            Debug.Log($"Destreza insuficiente. Necessária: {requiredDexterity}, Atual: {stats.Dexterity}");
            return false;
        }
        
        if (stats.Intelligence < requiredIntelligence)
        {
            Debug.Log($"Inteligência insuficiente. Necessária: {requiredIntelligence}, Atual: {stats.Intelligence}");
            return false;
        }
        
        return true;
    }

    public override void UseItem(CharacterStats stats)
    {
        // Para equipamentos, "usar" significa equipar
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.EquipItem(this);
        }
        else
        {
            Debug.LogError("EquipmentManager não encontrado!");
        }
    }
    
    /// <summary>
    /// Retorna uma descrição detalhada do equipamento
    /// </summary>
    /// <returns>Descrição formatada</returns>
    public override string ToString()
    {
        string desc = $"{itemName}\n{description}\n\n";
        
        // Requisitos
        if (requiredLevel > 1 || requiredStrength > 0 || requiredDexterity > 0 || requiredIntelligence > 0)
        {
            desc += "Requisitos:\n";
            if (requiredLevel > 1) desc += $"Nível: {requiredLevel}\n";
            if (requiredStrength > 0) desc += $"Força: {requiredStrength}\n";
            if (requiredDexterity > 0) desc += $"Destreza: {requiredDexterity}\n";
            if (requiredIntelligence > 0) desc += $"Inteligência: {requiredIntelligence}\n";
            desc += "\n";
        }
        
        // Bônus de atributos
        if (strengthBonus > 0 || dexterityBonus > 0 || intelligenceBonus > 0 || vitalityBonus > 0)
        {
            desc += "Bônus de Atributos:\n";
            if (strengthBonus > 0) desc += $"Força: +{strengthBonus}\n";
            if (dexterityBonus > 0) desc += $"Destreza: +{dexterityBonus}\n";
            if (intelligenceBonus > 0) desc += $"Inteligência: +{intelligenceBonus}\n";
            if (vitalityBonus > 0) desc += $"Vitalidade: +{vitalityBonus}\n";
            desc += "\n";
        }
        
        // Defesa
        if (physicalDefense > 0 || magicalDefense > 0)
        {
            desc += "Defesa:\n";
            if (physicalDefense > 0) desc += $"Física: +{physicalDefense}\n";
            if (magicalDefense > 0) desc += $"Mágica: +{magicalDefense}\n";
            desc += "\n";
        }
        
        return desc;
    }
}

/// <summary>
/// Tipos de equipamento
/// </summary>
public enum EquipmentType
{
    Weapon,     // Arma
    Armor,      // Armadura
    Accessory   // Acessório
}

/// <summary>
/// Slots específicos de equipamento
/// </summary>
public enum EquipmentSlot
{
    // Armas
    MainHand,       // Arma principal
    OffHand,        // Arma secundária/escudo
    
    // Armaduras
    Helmet,         // Capacete
    Chest,          // Peitoral
    Legs,           // Pernas
    Boots,          // Botas
    Gloves,         // Luvas
    
    // Acessórios
    Ring1,          // Anel 1
    Ring2,          // Anel 2
    Necklace,       // Colar
    Earring1,       // Brinco 1
    Earring2        // Brinco 2
}