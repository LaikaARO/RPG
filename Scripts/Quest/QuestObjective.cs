using UnityEngine;

/// <summary>
/// Classe base para todos os objetivos de quests.
/// </summary>
[System.Serializable]
public abstract class QuestObjective
{
    [Tooltip("Descrição do objetivo")]
    public string description = "Descrição do objetivo";
    
    [Tooltip("Quantidade atual")]
    [HideInInspector]
    public int currentAmount = 0;
    
    [Tooltip("Quantidade alvo necessária para completar o objetivo")]
    public int requiredAmount = 1;
    
    [Tooltip("Se o objetivo está completo")]
    [HideInInspector]
    public bool completed = false;
    
    /// <summary>
    /// Verifica se o objetivo foi concluído
    /// </summary>
    public virtual bool IsCompleted()
    {
        return completed || currentAmount >= requiredAmount;
    }
    
    /// <summary>
    /// Atualiza o progresso do objetivo
    /// </summary>
    public virtual void UpdateProgress(int amount)
    {
        currentAmount += amount;
        if (currentAmount >= requiredAmount && !completed)
        {
            completed = true;
            OnCompleted();
        }
    }
    
    /// <summary>
    /// Reseta o progresso do objetivo
    /// </summary>
    public virtual void ResetProgress()
    {
        currentAmount = 0;
        completed = false;
    }
    
    /// <summary>
    /// Método chamado quando o objetivo é completado
    /// </summary>
    protected virtual void OnCompleted()
    {
        Debug.Log($"Objetivo completado: {description}");
    }
}

/// <summary>
/// Objetivo de matar inimigos específicos
/// </summary>
[System.Serializable]
public class KillObjective : QuestObjective
{
    [Tooltip("ID do inimigo a ser derrotado (1000-9999)")]
    [Range(1000, 9999)]
    public int enemyID;
    
    [Tooltip("Nome do inimigo para exibição")]
    public string enemyName;
}

/// <summary>
/// Objetivo de coletar itens específicos
/// </summary>
[System.Serializable]
public class CollectObjective : QuestObjective
{
    [Tooltip("ID do item a ser coletado (10000-19999)")]
    [Range(10000, 19999)]
    public int itemID;
    
    [Tooltip("Item a ser coletado")]
    public Item item;
}

/// <summary>
/// Objetivo de falar com um NPC específico
/// </summary>
[System.Serializable]
public class TalkObjective : QuestObjective
{
    [Tooltip("ID do NPC para conversar")]
    public int npcID;
    
    [Tooltip("Nome do NPC para exibição")]
    public string npcName;
    
    public TalkObjective()
    {
        requiredAmount = 1;
    }
}

/// <summary>
/// Objetivo de explorar uma área específica
/// </summary>
[System.Serializable]
public class ExploreObjective : QuestObjective
{
    [Tooltip("Nome da área a ser explorada")]
    public string areaName;
    
    [Tooltip("Posição da área no mundo")]
    public Vector3 areaPosition;
    
    [Tooltip("Raio da área a ser explorada")]
    public float areaRadius = 5f;
    
    public ExploreObjective()
    {
        requiredAmount = 1;
    }
}