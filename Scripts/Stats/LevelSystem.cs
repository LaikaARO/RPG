using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de níveis e progressão do personagem.
/// Gerencia a experiência, subida de níveis e distribuição de pontos de atributos.
/// </summary>
public class LevelSystem : MonoBehaviour
{
    [Header("Configurações de Nível")]
    [Tooltip("Nível inicial do personagem")]
    [SerializeField] private int level = 1;
    
    [Tooltip("Experiência atual do personagem")]
    [SerializeField] private int currentExp = 0;
    
    [Tooltip("Experiência necessária para o primeiro nível")]
    [SerializeField] private int baseExpToLevelUp = 100;
    
    [Tooltip("Multiplicador de experiência para cada nível")]
    [SerializeField] private float expMultiplier = 1.5f;
    
    [Header("Recompensas por Nível")]
    [Tooltip("Pontos de atributo ganhos por nível")]
    [SerializeField] private int attributePointsPerLevel = 5;

    // Referência ao componente de estatísticas do personagem
    private CharacterStats characterStats;
    
    // Cache para cálculos de experiência
    private Dictionary<int, int> expToLevelCache = new Dictionary<int, int>();

    // Eventos
    public event Action<int> OnLevelUp;           // Disparado quando o personagem sobe de nível (novo nível)
    public event Action<int, int, int> OnExpGain; // Disparado quando o personagem ganha experiência (exp atual, exp para próximo nível, exp ganho)
    
    // Constantes para cálculos
    private const int MIN_LEVEL = 1;
    private const int MIN_EXP = 0;

    private void Awake()
    {
        InitializeComponents();
        InitializeExpCache();
    }
    
    /// <summary>
    /// Inicializa os componentes necessários
    /// </summary>
    private void InitializeComponents()
    {
        // Obter referência ao componente de estatísticas
        characterStats = GetComponent<CharacterStats>();
        if (characterStats == null)
        {
            Debug.LogError("LevelSystem requer um componente CharacterStats no mesmo GameObject.");
        }
    }
    
    /// <summary>
    /// Inicializa o cache de experiência para níveis iniciais
    /// </summary>
    private void InitializeExpCache()
    {
        // Pré-calcular experiência para os primeiros 10 níveis
        for (int i = 1; i <= 10; i++)
        {
            expToLevelCache[i] = CalculateExpForLevel(i);
        }
    }

    private void Start()
    {
        // Validar valores iniciais
        ValidateInitialValues();
    }
    
    /// <summary>
    /// Valida os valores iniciais para garantir que estão dentro dos limites aceitáveis
    /// </summary>
    private void ValidateInitialValues()
    {
        // Garantir que o nível não seja menor que o mínimo
        if (level < MIN_LEVEL)
        {
            level = MIN_LEVEL;
            Debug.LogWarning("Nível inicial ajustado para o mínimo (1).");
        }
        
        // Garantir que a experiência não seja negativa
        if (currentExp < MIN_EXP)
        {
            currentExp = MIN_EXP;
            Debug.LogWarning("Experiência inicial ajustada para o mínimo (0).");
        }
        
        // Garantir que o multiplicador de experiência seja válido
        if (expMultiplier <= 1.0f)
        {
            expMultiplier = 1.1f;
            Debug.LogWarning("Multiplicador de experiência ajustado para 1.1 (mínimo válido).");
        }
    }

    /// <summary>
    /// Adiciona experiência ao personagem e verifica se subiu de nível
    /// </summary>
    /// <param name="amount">Quantidade de experiência a adicionar</param>
    /// <returns>True se a experiência foi adicionada com sucesso</returns>
    public bool AddExperience(int amount)
    {
        if (amount <= 0) return false;

        try
        {
            // Adicionar experiência
            currentExp += amount;
            
            // Verificar se subiu de nível
            CheckLevelUp();
            
            // Notificar sobre o ganho de experiência
            int expToNext = GetExpToNextLevel();
            OnExpGain?.Invoke(currentExp, expToNext, amount);
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao adicionar experiência: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifica se o personagem subiu de nível e aplica as recompensas
    /// </summary>
    private void CheckLevelUp()
    {
        try
        {
            int expToNextLevel = GetExpToNextLevel();
            bool leveledUp = false;
            
            // Enquanto tiver experiência suficiente para subir de nível
            while (currentExp >= expToNextLevel)
            {
                // Subtrair a experiência necessária
                currentExp -= expToNextLevel;
                
                // Aumentar o nível
                level++;
                leveledUp = true;
                
                // Aplicar recompensas de nível
                ApplyLevelUpRewards();
                
                // Notificar sobre a subida de nível
                OnLevelUp?.Invoke(level);
                
                // Recalcular a experiência necessária para o próximo nível
                expToNextLevel = GetExpToNextLevel();
            }
            
            // Se subiu de nível, adicionar ao cache se ainda não existir
            if (leveledUp && !expToLevelCache.ContainsKey(level + 1))
            {
                expToLevelCache[level + 1] = CalculateExpForLevel(level + 1);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao verificar subida de nível: {ex.Message}");
        }
    }

    /// <summary>
    /// Aplica as recompensas por subir de nível
    /// </summary>
    private void ApplyLevelUpRewards()
    {
        try
        {
            if (characterStats == null) return;
            
            // Adicionar pontos de atributo
            characterStats.AddAttributePoints(attributePointsPerLevel);
            
            // Restaurar vida e mana ao máximo
            characterStats.RestoreFullHealth();
            characterStats.RestoreFullMana();
            
            Debug.Log($"Nível aumentado para {level}! Ganhou {attributePointsPerLevel} pontos de atributo.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao aplicar recompensas de nível: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula a experiência necessária para um nível específico
    /// </summary>
    private int CalculateExpForLevel(int targetLevel)
    {
        if (targetLevel <= 1) return baseExpToLevelUp;
        
        // Fórmula: Exp base * (multiplicador ^ (nível - 1))
        return Mathf.RoundToInt(baseExpToLevelUp * Mathf.Pow(expMultiplier, targetLevel - 1));
    }

    /// <summary>
    /// Retorna a experiência necessária para o próximo nível
    /// </summary>
    public int GetExpToNextLevel()
    {
        int nextLevel = level + 1;
        
        // Verificar se o valor está no cache
        if (expToLevelCache.TryGetValue(nextLevel, out int cachedExp))
        {
            return cachedExp;
        }
        
        // Calcular e armazenar no cache
        int expRequired = CalculateExpForLevel(nextLevel);
        expToLevelCache[nextLevel] = expRequired;
        
        return expRequired;
    }

    /// <summary>
    /// Retorna a porcentagem de progresso para o próximo nível (0-1)
    /// </summary>
    public float GetLevelProgress()
    {
        int expToNextLevel = GetExpToNextLevel();
        if (expToNextLevel <= 0) return 0;
        
        // Garantir que o valor esteja entre 0 e 1
        float progress = (float)currentExp / expToNextLevel;
        return Mathf.Clamp01(progress);
    }

    #region Propriedades

    /// <summary>
    /// Nível atual do personagem
    /// </summary>
    public int Level => level;

    /// <summary>
    /// Experiência atual do personagem
    /// </summary>
    public int CurrentExp => currentExp;
    
    /// <summary>
    /// Verifica se o personagem está no nível máximo (para jogos com limite de nível)
    /// </summary>
    public bool IsMaxLevel => false; // Implementar lógica de nível máximo se necessário

    #endregion
    
    /// <summary>
    /// Limpa os eventos ao destruir o objeto para evitar memory leaks
    /// </summary>
    private void OnDestroy()
    {
        OnLevelUp = null;
        OnExpGain = null;
    }

    /// <summary>
    /// Define o nível do personagem (usado para inicialização ou debug)
    /// </summary>
    /// <returns>True se o nível foi definido com sucesso</returns>
    public bool SetLevel(int newLevel)
    {
        if (newLevel <= 0)
        {
            Debug.LogWarning("Tentativa de definir nível inválido (menor ou igual a zero).");
            return false;
        }

        try
        {
            // Calcular quantos níveis subiu
            int levelDifference = newLevel - level;
            
            if (levelDifference > 0)
            {
                // Subir de nível e aplicar recompensas
                level = newLevel;
                
                // Aplicar recompensas para cada nível ganho
                if (characterStats != null)
                {
                    characterStats.AddAttributePoints(attributePointsPerLevel * levelDifference);
                    
                    // Restaurar vida e mana
                    characterStats.RestoreFullHealth();
                    characterStats.RestoreFullMana();
                }
                
                // Resetar experiência atual
                currentExp = 0;
                
                // Notificar sobre a subida de nível
                OnLevelUp?.Invoke(level);
                
                // Atualizar cache para o próximo nível
                if (!expToLevelCache.ContainsKey(level + 1))
                {
                    expToLevelCache[level + 1] = CalculateExpForLevel(level + 1);
                }
                
                return true;
            }
            else if (levelDifference < 0)
            {
                // Reduzir nível (para debug ou efeitos especiais)
                level = newLevel;
                currentExp = 0;
                
                // Notificar sobre a mudança de nível
                OnLevelUp?.Invoke(level);
                
                return true;
            }
            
            return false; // Nenhuma mudança
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao definir nível: {ex.Message}");
            return false;
        }
    }
}