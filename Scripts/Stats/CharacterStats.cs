using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia os atributos básicos do personagem: Força, Destreza, Inteligência e Vitalidade.
/// Calcula estatísticas derivadas como HP, Mana, Dano, etc.
/// Versão atualizada com suporte a bônus de equipamentos.
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Header("Atributos Básicos")]
    [SerializeField] private int baseStrength = 10;     // Força - Aumenta dano físico e capacidade de carga
    [SerializeField] private int baseDexterity = 10;    // Destreza - Aumenta velocidade de ataque e chance de esquiva
    [SerializeField] private int baseIntelligence = 10; // Inteligência - Aumenta mana e dano mágico
    [SerializeField] private int baseVitality = 10;     // Vitalidade - Aumenta HP e resistência física

    [Header("Estatísticas Derivadas")]
    [SerializeField] private int maxHealth;            // Vida máxima
    [SerializeField] private int currentHealth;        // Vida atual
    [SerializeField] private int maxMana;              // Mana máxima
    [SerializeField] private int currentMana;          // Mana atual
    [SerializeField] private float attackPower;        // Poder de ataque
    [SerializeField] private float magicPower;         // Poder mágico
    [SerializeField] private float attackSpeed;        // Velocidade de ataque
    [SerializeField] private float moveSpeed;          // Velocidade de movimento
    [SerializeField] private float dodgeChance;        // Chance de esquiva
    [SerializeField] private float criticalChance;     // Chance de crítico
    [SerializeField] private float criticalMultiplier; // Multiplicador de dano crítico
    [SerializeField] private int physicalDefense;      // Defesa física
    [SerializeField] private int magicalDefense;       // Defesa mágica

    // Pontos de atributos disponíveis para distribuir
    [SerializeField] private int attributePoints = 0;

    // Modificadores temporários de atributos (buffs/debuffs)
    private int strengthModifier = 0;
    private int dexterityModifier = 0;
    private int intelligenceModifier = 0;
    private int vitalityModifier = 0;

    // Bônus de equipamentos
    private int equipmentHealthBonus = 0;
    private int equipmentManaBonus = 0;
    private float equipmentAttackPowerBonus = 0;
    private float equipmentMagicPowerBonus = 0;
    private float equipmentAttackSpeedBonus = 0;
    private int equipmentPhysicalDefenseBonus = 0;
    private int equipmentMagicalDefenseBonus = 0;

    // Eventos
    public event Action<int, int> OnHealthChanged;  // (currentHealth, maxHealth)
    public event Action<int, int> OnManaChanged;    // (currentMana, maxMana)
    public event Action OnStatsUpdated;             // Disparado quando qualquer estatística é atualizada
    public UnityEngine.Events.UnityEvent OnHealthReachedZero; // Disparado quando a vida chega a zero
    
    // Cache de cálculos para melhorar performance
    private Dictionary<string, float> statsCache = new Dictionary<string, float>();
    private bool statsDirty = true;

    private void Awake()
    {
        // Inicializar o dicionário de cache
        InitializeStatsCache();
        
        // Garantir que o evento OnHealthReachedZero esteja inicializado
        if (OnHealthReachedZero == null)
        {
            OnHealthReachedZero = new UnityEngine.Events.UnityEvent();
        }
    }
    
    private void Start()
    {
        // Calcular estatísticas iniciais
        CalculateStats();
        
        // Inicializar vida e mana com valores máximos
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        // Disparar eventos iniciais para atualizar a UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
        OnStatsUpdated?.Invoke();

        // Inscrever-se nos eventos do equipamento
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged += OnEquipmentChanged;
        }
    }
    
    /// <summary>
    /// Inicializa o dicionário de cache para estatísticas
    /// </summary>
    private void InitializeStatsCache()
    {
        statsCache = new Dictionary<string, float>
        {
            { "maxHealth", 0 },
            { "maxMana", 0 },
            { "attackPower", 0 },
            { "magicPower", 0 },
            { "attackSpeed", 0 },
            { "moveSpeed", 0 },
            { "dodgeChance", 0 },
            { "criticalChance", 0 },
            { "criticalMultiplier", 0 },
            { "physicalDefense", 0 },
            { "magicalDefense", 0 }
        };
    }

    /// <summary>
    /// Calcula todas as estatísticas derivadas com base nos atributos
    /// </summary>
    public void CalculateStats()
    {
        // Atualizar bônus de equipamentos
        UpdateEquipmentBonuses();

        // Constantes para cálculos de estatísticas
        const int HEALTH_PER_VITALITY = 10;
        const int HEALTH_PER_STRENGTH = 2;
        const int MANA_PER_INTELLIGENCE = 10;
        const int MANA_PER_VITALITY = 1;
        const float ATTACK_PER_STRENGTH = 2.0f;
        const float ATTACK_PER_DEXTERITY = 0.5f;
        const float MAGIC_PER_INTELLIGENCE = 2.0f;
        const float MAGIC_PER_VITALITY = 0.2f;
        const float BASE_ATTACK_SPEED = 1.0f;
        const float ATTACK_SPEED_PER_DEXTERITY = 0.01f;
        const float BASE_MOVE_SPEED = 3.5f;
        const float MOVE_SPEED_PER_DEXTERITY = 0.005f;
        const float DODGE_PER_DEXTERITY = 0.2f;
        const float CRIT_CHANCE_PER_DEXTERITY = 0.1f;
        const float CRIT_CHANCE_PER_INTELLIGENCE = 0.05f;
        const float BASE_CRIT_MULTIPLIER = 1.5f;
        const float CRIT_MULTIPLIER_PER_STRENGTH = 0.01f;

        // Calcular HP máximo (Vitalidade * 10 + Força * 2 + bônus de equipamentos)
        int oldMaxHealth = maxHealth;
        maxHealth = (Vitality * HEALTH_PER_VITALITY) + (Strength * HEALTH_PER_STRENGTH) + equipmentHealthBonus;
        statsCache["maxHealth"] = maxHealth;
        
        // Ajustar HP atual proporcionalmente se o máximo mudar
        if (oldMaxHealth > 0 && currentHealth > 0)
        {
            currentHealth = Mathf.CeilToInt((float)currentHealth / oldMaxHealth * maxHealth);
        }

        // Calcular Mana máxima (Inteligência * 10 + Vitalidade * 1 + bônus de equipamentos)
        int oldMaxMana = maxMana;
        maxMana = (Intelligence * MANA_PER_INTELLIGENCE) + (Vitality * MANA_PER_VITALITY) + equipmentManaBonus;
        statsCache["maxMana"] = maxMana;
        
        // Ajustar Mana atual proporcionalmente se o máximo mudar
        if (oldMaxMana > 0 && currentMana > 0)
        {
            currentMana = Mathf.CeilToInt((float)currentMana / oldMaxMana * maxMana);
        }

        // Calcular poder de ataque (Força * 2 + Destreza * 0.5 + bônus de equipamentos)
        attackPower = (Strength * ATTACK_PER_STRENGTH) + (Dexterity * ATTACK_PER_DEXTERITY) + equipmentAttackPowerBonus;
        statsCache["attackPower"] = attackPower;

        // Calcular poder mágico (Inteligência * 2 + Vitalidade * 0.2 + bônus de equipamentos)
        magicPower = (Intelligence * MAGIC_PER_INTELLIGENCE) + (Vitality * MAGIC_PER_VITALITY) + equipmentMagicPowerBonus;
        statsCache["magicPower"] = magicPower;

        // Calcular velocidade de ataque (base + Destreza * 0.01 + bônus de equipamentos)
        attackSpeed = BASE_ATTACK_SPEED + (Dexterity * ATTACK_SPEED_PER_DEXTERITY) + equipmentAttackSpeedBonus;
        statsCache["attackSpeed"] = attackSpeed;

        // Calcular velocidade de movimento (base + Destreza * 0.005)
        moveSpeed = BASE_MOVE_SPEED + (Dexterity * MOVE_SPEED_PER_DEXTERITY);
        statsCache["moveSpeed"] = moveSpeed;

        // Calcular chance de esquiva (Destreza * 0.2%)
        dodgeChance = Dexterity * DODGE_PER_DEXTERITY;
        statsCache["dodgeChance"] = dodgeChance;

        // Calcular chance de crítico (Destreza * 0.1% + Inteligência * 0.05%)
        criticalChance = (Dexterity * CRIT_CHANCE_PER_DEXTERITY) + (Intelligence * CRIT_CHANCE_PER_INTELLIGENCE);
        statsCache["criticalChance"] = criticalChance;
        
        // Calcular multiplicador de dano crítico (base + Força * 0.01)
        criticalMultiplier = BASE_CRIT_MULTIPLIER + (Strength * CRIT_MULTIPLIER_PER_STRENGTH);
        statsCache["criticalMultiplier"] = criticalMultiplier;

        // Calcular defesas (bônus de equipamentos + base dos atributos)
        physicalDefense = equipmentPhysicalDefenseBonus + (Vitality / 2);
        statsCache["physicalDefense"] = physicalDefense;
        
        magicalDefense = equipmentMagicalDefenseBonus + (Intelligence / 3);
        statsCache["magicalDefense"] = magicalDefense;

        // Marcar estatísticas como atualizadas
        statsDirty = false;

        // Disparar evento de atualização de estatísticas
        OnStatsUpdated?.Invoke();
        
        // Disparar eventos para atualizar a UI de vida e mana quando os valores máximos mudam
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    /// <summary>
    /// Atualiza os bônus de equipamentos
    /// </summary>
    private void UpdateEquipmentBonuses()
    {
        if (EquipmentManager.Instance == null) return;

        // Resetar bônus
        ResetEquipmentBonuses();

        // Somar bônus de todos os equipamentos
        var equippedItems = EquipmentManager.Instance.GetAllEquippedItems();
        if (equippedItems == null) return;
        
        foreach (var kvp in equippedItems)
        {
            if (kvp.Value != null)
            {
                AddEquipmentBonuses(kvp.Value);
            }
        }
        
        // Marcar estatísticas como desatualizadas
        statsDirty = true;
    }
    
    /// <summary>
    /// Reseta todos os bônus de equipamentos
    /// </summary>
    private void ResetEquipmentBonuses()
    {
        equipmentHealthBonus = 0;
        equipmentManaBonus = 0;
        equipmentAttackPowerBonus = 0;
        equipmentMagicPowerBonus = 0;
        equipmentAttackSpeedBonus = 0;
        equipmentPhysicalDefenseBonus = 0;
        equipmentMagicalDefenseBonus = 0;
    }
    
    /// <summary>
    /// Adiciona os bônus de um equipamento específico
    /// </summary>
    private void AddEquipmentBonuses(EquipmentItem item)
    {
        if (item == null) return;
        
        equipmentHealthBonus += item.healthBonus;
        equipmentManaBonus += item.manaBonus;
        equipmentAttackPowerBonus += item.attackPowerBonus;
        equipmentMagicPowerBonus += item.magicPowerBonus;
        equipmentAttackSpeedBonus += item.attackSpeedBonus;
        equipmentPhysicalDefenseBonus += item.physicalDefense;
        equipmentMagicalDefenseBonus += item.magicalDefense;
    }

    /// <summary>
    /// Evento chamado quando os equipamentos mudam
    /// </summary>
    private void OnEquipmentChanged()
    {
        CalculateStats();
    }

    #region Propriedades de Atributos

    /// <summary>
    /// Força total (base + modificadores)
    /// </summary>
    public int Strength => baseStrength + strengthModifier;

    /// <summary>
    /// Destreza total (base + modificadores)
    /// </summary>
    public int Dexterity => baseDexterity + dexterityModifier;

    /// <summary>
    /// Inteligência total (base + modificadores)
    /// </summary>
    public int Intelligence => baseIntelligence + intelligenceModifier;

    /// <summary>
    /// Vitalidade total (base + modificadores)
    /// </summary>
    public int Vitality => baseVitality + vitalityModifier;

    /// <summary>
    /// Pontos de atributo disponíveis para distribuir
    /// </summary>
    public int AttributePoints => attributePoints;
    
    /// <summary>
    /// Vida atual do personagem
    /// </summary>
    public int CurrentHealth => currentHealth;
    
    /// <summary>
    /// Vida máxima do personagem
    /// </summary>
    public int MaxHealth => maxHealth;
    
    /// <summary>
    /// Mana atual do personagem
    /// </summary>
    public int CurrentMana => currentMana;
    
    /// <summary>
    /// Mana máxima do personagem
    /// </summary>
    public int MaxMana => maxMana;

    /// <summary>
    /// Poder de ataque atual
    /// </summary>
    public float AttackPower 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("attackPower", out float value) ? value : attackPower;
        }
    }

    /// <summary>
    /// Poder mágico atual
    /// </summary>
    public float MagicPower 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("magicPower", out float value) ? value : magicPower;
        }
    }

    /// <summary>
    /// Defesa física atual
    /// </summary>
    public int PhysicalDefense 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("physicalDefense", out float value) ? (int)value : physicalDefense;
        }
    }

    /// <summary>
    /// Defesa mágica atual
    /// </summary>
    public int MagicalDefense 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("magicalDefense", out float value) ? (int)value : magicalDefense;
        }
    }

    /// <summary>
    /// Velocidade de movimento atual
    /// </summary>
    public float MoveSpeed 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("moveSpeed", out float value) ? value : moveSpeed;
        }
    }

    /// <summary>
    /// Velocidade de ataque atual
    /// </summary>
    public float AttackSpeed 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("attackSpeed", out float value) ? value : attackSpeed;
        }
    }

    /// <summary>
    /// Chance de esquiva atual
    /// </summary>
    public float DodgeChance 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("dodgeChance", out float value) ? value : dodgeChance;
        }
    }

    /// <summary>
    /// Chance de crítico atual
    /// </summary>
    public float CriticalChance 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("criticalChance", out float value) ? value : criticalChance;
        }
    }

    /// <summary>
    /// Multiplicador de crítico atual
    /// </summary>
    public float CriticalMultiplier 
    {
        get 
        {
            if (statsDirty) CalculateStats();
            return statsCache.TryGetValue("criticalMultiplier", out float value) ? value : criticalMultiplier;
        }
    }

    #endregion

    #region Métodos de Modificação de Atributos

    /// <summary>
    /// Adiciona pontos ao atributo de Força
    /// </summary>
    public bool AddStrength(int points)
    {
        if (points <= 0) return false;
        if (points > attributePoints) return false;
        
        baseStrength += points;
        attributePoints -= points;
        statsDirty = true;
        CalculateStats();
        return true;
    }

    /// <summary>
    /// Adiciona pontos ao atributo de Destreza
    /// </summary>
    public bool AddDexterity(int points)
    {
        if (points <= 0) return false;
        if (points > attributePoints) return false;
        
        baseDexterity += points;
        attributePoints -= points;
        statsDirty = true;
        CalculateStats();
        return true;
    }

    /// <summary>
    /// Adiciona pontos ao atributo de Inteligência
    /// </summary>
    public bool AddIntelligence(int points)
    {
        if (points <= 0) return false;
        if (points > attributePoints) return false;
        
        baseIntelligence += points;
        attributePoints -= points;
        statsDirty = true;
        CalculateStats();
        return true;
    }

    /// <summary>
    /// Adiciona pontos ao atributo de Vitalidade
    /// </summary>
    public bool AddVitality(int points)
    {
        if (points <= 0) return false;
        if (points > attributePoints) return false;
        
        baseVitality += points;
        attributePoints -= points;
        statsDirty = true;
        CalculateStats();
        return true;
    }

    /// <summary>
    /// Adiciona pontos de atributo disponíveis para distribuição
    /// </summary>
    public void AddAttributePoints(int points)
    {
        if (points <= 0) return;
        
        attributePoints += points;
        OnStatsUpdated?.Invoke();
    }

    #endregion

    #region Métodos de Modificadores Temporários

    /// <summary>
    /// Adiciona um modificador temporário de Força
    /// </summary>
    public void AddStrengthModifier(int modifier)
    {
        strengthModifier += modifier;
        statsDirty = true;
        CalculateStats();
    }

    /// <summary>
    /// Adiciona um modificador temporário de Destreza
    /// </summary>
    public void AddDexterityModifier(int modifier)
    {
        dexterityModifier += modifier;
        statsDirty = true;
        CalculateStats();
    }

    /// <summary>
    /// Adiciona um modificador temporário de Inteligência
    /// </summary>
    public void AddIntelligenceModifier(int modifier)
    {
        intelligenceModifier += modifier;
        statsDirty = true;
        CalculateStats();
    }

    /// <summary>
    /// Adiciona um modificador temporário de Vitalidade
    /// </summary>
    public void AddVitalityModifier(int modifier)
    {
        vitalityModifier += modifier;
        statsDirty = true;
        CalculateStats();
    }

    /// <summary>
    /// Remove todos os modificadores temporários
    /// </summary>
    public void ClearAllModifiers()
    {
        if (strengthModifier == 0 && dexterityModifier == 0 && 
            intelligenceModifier == 0 && vitalityModifier == 0)
            return;
            
        strengthModifier = 0;
        dexterityModifier = 0;
        intelligenceModifier = 0;
        vitalityModifier = 0;
        statsDirty = true;
        CalculateStats();
    }

    #endregion

    #region Métodos de Vida e Mana

    /// <summary>
    /// Modifica a vida atual do personagem
    /// </summary>
    public void ModifyHealth(int amount)
    {
        // Se o personagem já está morto e está recebendo dano (valor negativo), ignorar
        if (isDead && amount < 0)
        {
            Debug.Log($"{gameObject.name} já está morto, ignorando dano de {Mathf.Abs(amount)}");
            return;
        }
        
        int oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        
        // Só dispara o evento se a vida realmente mudou
        if (oldHealth != currentHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Mostrar texto de dano se for dano recebido (valor negativo) e NÃO for causado pelo sistema de combate
            // O sistema de combate já exibe o texto de dano, então não precisamos exibir novamente aqui
            // Isso evita a duplicação de texto de dano nos mobs
            if (amount < 0 && !IsDamageFromCombatSystem())
            {
                // Converter para valor positivo para exibição
                int damageAmount = Mathf.Abs(amount);
                
                // Verificar se o DamageTextManager está disponível
                if (DamageTextManager.Instance != null)
                {
                    // Determinar se é um golpe crítico (pode ser implementado com base em alguma lógica do jogo)
                    bool isCritical = false; // Por padrão não é crítico
                    
                    // Mostrar o texto de dano na posição do personagem
                    DamageTextManager.Instance.ShowDamageText(transform.position, damageAmount, isCritical);
                }
            }
            
            // Verificar se o personagem morreu
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    /// <summary>
    /// Verifica se o dano está sendo causado pelo sistema de combate
    /// </summary>
    /// <returns>True se o dano vier do sistema de combate, False caso contrário</returns>
    // Flag para controlar se o dano está vindo do CombatSystem
    private static bool isDamageFromCombatSystem = false;
    
    /// <summary>
    /// Define se o dano está vindo do sistema de combate
    /// </summary>
    public static void SetDamageFromCombatSystem(bool value)
    {
        isDamageFromCombatSystem = value;
    }
    
    /// <summary>
    /// Verifica se o dano está sendo causado pelo sistema de combate
    /// </summary>
    private bool IsDamageFromCombatSystem()
    {
        return isDamageFromCombatSystem;
    }
    
    /// <summary>
    /// Modifica a mana atual do personagem
    /// </summary>
    public void ModifyMana(int amount)
    {
        int oldMana = currentMana;
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        
        // Só dispara o evento se a mana realmente mudou
        if (oldMana != currentMana)
        {
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }

    /// <summary>
    /// Restaura completamente a vida do personagem
    /// </summary>
    public void RestoreFullHealth()
    {
        if (currentHealth == maxHealth) return;
        
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Restaura completamente a mana do personagem
    /// </summary>
    public void RestoreFullMana()
    {
        if (currentMana == maxMana) return;
        
        currentMana = maxMana;
        OnManaChanged?.Invoke(currentMana, maxMana);
    }
    
    /// <summary>
    /// Verifica se o personagem está vivo
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    #endregion

    /// <summary>
    /// Método chamado quando o personagem morre
    /// </summary>
    private void Die()
    {
        // Evitar chamar Die() múltiplas vezes
        if (currentHealth <= 0 && isDead) return;
        
        isDead = true;
        
        // Implementação básica da morte
        Debug.Log($"{gameObject.name} morreu!");
        
        // Disparar evento de morte para que outros sistemas possam reagir
        OnHealthReachedZero?.Invoke();
    }
    
    // Flag para controlar se o personagem já está morto
    private bool isDead = false;

    /// <summary>
    /// Retorna as estatísticas atuais como string para debug
    /// </summary>
    public string GetStatsInfo()
    {
        return $"Força: {Strength}\n" +
               $"Destreza: {Dexterity}\n" +
               $"Inteligência: {Intelligence}\n" +
               $"Vitalidade: {Vitality}\n" +
               $"HP: {currentHealth}/{maxHealth}\n" +
               $"Mana: {currentMana}/{maxMana}\n" +
               $"Poder de Ataque: {attackPower:F1}\n" +
               $"Poder Mágico: {magicPower:F1}\n" +
               $"Velocidade de Ataque: {attackSpeed:F2}\n" +
               $"Velocidade de Movimento: {moveSpeed:F2}\n" +
               $"Chance de Esquiva: {dodgeChance:F1}%\n" +
               $"Chance de Crítico: {criticalChance:F1}%\n" +
               $"Multiplicador Crítico: {criticalMultiplier:F2}x\n" +
               $"Defesa Física: {physicalDefense}\n" +
               $"Defesa Mágica: {magicalDefense}";
    }

    private void OnDestroy()
    {
        // Cancelar inscrição nos eventos
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= OnEquipmentChanged;
        }
        
        // Limpar eventos para evitar memory leaks
        OnHealthChanged = null;
        OnManaChanged = null;
        OnStatsUpdated = null;
        
        if (OnHealthReachedZero != null)
        {
            OnHealthReachedZero.RemoveAllListeners();
        }
    }
}