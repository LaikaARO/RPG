using UnityEngine;

/// <summary>
/// Classe base para todos os itens do jogo.
/// Define propriedades básicas como nome, descrição, ícone e raridade.
/// Versão atualizada com melhor suporte a equipamentos
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "RPG/Item")]
public class Item : ScriptableObject
{
    // ID do item para o sistema de quests (10000-19999)
    [Header("Quest System")]
    [Tooltip("ID único do item para o sistema de quests (10000-19999)")]
    public int itemID = 10000;
    [Header("Informações Básicas")]
    [Tooltip("Nome do item")]
    public string itemName = "Novo Item";
    
    [Tooltip("Descrição do item")]
    [TextArea(3, 5)]
    public string description = "Um item comum";
    
    [Tooltip("Ícone do item para exibição no inventário")]
    public Sprite icon;
    
    [Header("Propriedades")]
    [Tooltip("Raridade do item")]
    public ItemRarity rarity = ItemRarity.Common;
    
    [Tooltip("Valor do item em moedas")]
    public int value = 1;
    
    [Tooltip("Se o item pode ser empilhado")]
    public bool isStackable = false;
    
    [Tooltip("Quantidade máxima por pilha")]
    public int maxStackSize = 1;
    
    [Tooltip("Tipo do item")]
    public ItemType itemType = ItemType.Miscellaneous;

    /// <summary>
    /// Usa o item - implementação base (pode ser sobrescrita)
    /// </summary>
    public virtual void UseItem(CharacterStats stats)
    {
        Debug.Log($"Usou o item: {itemName}");
    }

    /// <summary>
    /// Retorna a cor baseada na raridade do item
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.white;
            case ItemRarity.Uncommon:
                return Color.green;
            case ItemRarity.Rare:
                return Color.blue;
            case ItemRarity.Epic:
                return new Color(0.7f, 0.3f, 1f); // Roxo
            case ItemRarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Retorna uma descrição detalhada do item (pode ser sobrescrita)
    /// </summary>
    /// <returns>Descrição formatada</returns>
    public override string ToString()
    {
        string desc = $"{itemName}\n{description}\n\n";
        
        // Informações básicas
        desc += $"Tipo: {GetItemTypeDisplayName()}\n";
        desc += $"Raridade: {GetRarityDisplayName()}\n";
        desc += $"Valor: {value} moedas\n";
        
        if (isStackable)
        {
            desc += $"Empilhável (máx: {maxStackSize})\n";
        }
        
        return desc;
    }

    /// <summary>
    /// Retorna o nome de exibição do tipo do item
    /// </summary>
    protected virtual string GetItemTypeDisplayName()
    {
        switch (itemType)
        {
            case ItemType.Weapon: return "Arma";
            case ItemType.Armor: return "Armadura";
            case ItemType.Consumable: return "Consumível";
            case ItemType.Miscellaneous: return "Diversos";
            case ItemType.QuestItem: return "Item de Quest";
            default: return itemType.ToString();
        }
    }

    /// <summary>
    /// Retorna o nome de exibição da raridade
    /// </summary>
    protected string GetRarityDisplayName()
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "Comum";
            case ItemRarity.Uncommon: return "Incomum";
            case ItemRarity.Rare: return "Raro";
            case ItemRarity.Epic: return "Épico";
            case ItemRarity.Legendary: return "Lendário";
            default: return rarity.ToString();
        }
    }

    /// <summary>
    /// Verifica se este item é um equipamento
    /// </summary>
    public bool IsEquipment()
    {
        return this is EquipmentItem;
    }

    /// <summary>
    /// Verifica se este item é consumível
    /// </summary>
    public bool IsConsumable()
    {
        return itemType == ItemType.Consumable || this is ConsumableItem;
    }

    /// <summary>
    /// Retorna informações de comparação entre dois itens (útil para tooltips)
    /// </summary>
    /// <param name="other">Item para comparar</param>
    /// <returns>String com comparação ou null se não comparável</returns>
    public virtual string GetComparisonText(Item other)
    {
        if (other == null || other.GetType() != this.GetType())
            return null;
        
        // Implementação base - pode ser sobrescrita em subclasses
        string comparison = "";
        
        if (value != other.value)
        {
            int valueDiff = value - other.value;
            comparison += $"Valor: {(valueDiff > 0 ? "+" : "")}{valueDiff}\n";
        }
        
        return string.IsNullOrEmpty(comparison) ? null : comparison;
    }
}

/// <summary>
/// Enumeração para tipos de raridade de itens
/// </summary>
public enum ItemRarity
{
    Common,    // Comum - Branco
    Uncommon,  // Incomum - Verde
    Rare,      // Raro - Azul
    Epic,      // Épico - Roxo
    Legendary  // Lendário - Dourado
}

/// <summary>
/// Enumeração para tipos de itens
/// </summary>
public enum ItemType
{
    Weapon,        // Arma
    Armor,         // Armadura
    Consumable,    // Consumível
    Miscellaneous, // Diversos
    QuestItem      // Item de Quest
}