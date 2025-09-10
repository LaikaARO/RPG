using UnityEngine;

/// <summary>
/// Item consumível que pode restaurar vida, mana ou aplicar efeitos temporários
/// </summary>
[CreateAssetMenu(fileName = "New Consumable", menuName = "RPG/Consumable Item")]
public class ConsumableItem : Item
{
    [Header("Efeitos de Consumo")]
    [Tooltip("Quantidade de vida restaurada")]
    public int healthRestore = 0;
    
    [Tooltip("Quantidade de mana restaurada")]
    public int manaRestore = 0;
    
    [Header("Efeitos de Atributos Temporários")]
    [Tooltip("Modificador temporário de força")]
    public int strengthBonus = 0;
    
    [Tooltip("Modificador temporário de destreza")]
    public int dexterityBonus = 0;
    
    [Tooltip("Modificador temporário de inteligência")]
    public int intelligenceBonus = 0;
    
    [Tooltip("Modificador temporário de vitalidade")]
    public int vitalityBonus = 0;
    
    [Tooltip("Duração dos efeitos temporários em segundos")]
    public float effectDuration = 30f;

    public override void UseItem(CharacterStats stats)
    {
        if (stats == null) return;

        // Aplicar restauração de vida
        if (healthRestore > 0)
        {
            stats.ModifyHealth(healthRestore);
            Debug.Log($"Restaurou {healthRestore} de vida com {itemName}");
        }

        // Aplicar restauração de mana
        if (manaRestore > 0)
        {
            stats.ModifyMana(manaRestore);
            Debug.Log($"Restaurou {manaRestore} de mana com {itemName}");
        }

        // Aplicar efeitos temporários de atributos
        if (strengthBonus != 0 || dexterityBonus != 0 || intelligenceBonus != 0 || vitalityBonus != 0)
        {
            // Aqui seria ideal ter um sistema de buffs/debuffs mais elaborado
            // Por enquanto, aplicamos diretamente aos modificadores
            stats.AddStrengthModifier(strengthBonus);
            stats.AddDexterityModifier(dexterityBonus);
            stats.AddIntelligenceModifier(intelligenceBonus);
            stats.AddVitalityModifier(vitalityBonus);
            
            Debug.Log($"Aplicou efeitos temporários com {itemName} por {effectDuration} segundos");
            
            // Seria necessário implementar um sistema para remover os efeitos após o tempo
            // Por simplicidade, não implementaremos isso agora
        }
    }
}