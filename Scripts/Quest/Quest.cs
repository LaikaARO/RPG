using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe base para todas as quests do jogo.
/// </summary>
[CreateAssetMenu(fileName = "New Quest", menuName = "RPG/Quest")]
public class Quest : ScriptableObject
{
    [Header("Identificação")]
    [Tooltip("ID único da quest (1-999)")]
    [Range(1, 999)]
    public int questID;
    
    [Tooltip("Nome da quest")]
    public string questName = "Nova Quest";
    
    [Tooltip("Descrição da quest")]
    [TextArea(3, 5)]
    public string description = "Descrição da quest";
    
    [Header("Configurações")]
    [Tooltip("Nível mínimo requerido para a quest")]
    public int requiredLevel = 1;
    
    [Tooltip("Quests que precisam ser completadas antes desta")]
    public List<Quest> prerequisites = new List<Quest>();
    
    [Tooltip("Tipo da quest")]
    public QuestType questType = QuestType.Main;
    
    [Tooltip("Tempo limite em minutos para completar a quest (0 = sem limite)")]
    public float timeLimit = 0f;
    
    [Header("Objetivos")]
    [Tooltip("Lista de objetivos da quest")]
    public List<QuestObjective> objectives = new List<QuestObjective>();
    
    [Header("Recompensas")]
    [Tooltip("Experiência ganha ao completar a quest")]
    public int experienceReward = 100;
    
    [Tooltip("Ouro ganho ao completar a quest")]
    public int goldReward = 50;
    
    [Tooltip("Itens recebidos ao completar a quest")]
    public List<QuestItemReward> itemRewards = new List<QuestItemReward>();
    
    /// <summary>
    /// Verifica se todos os objetivos da quest foram concluídos
    /// </summary>
    public bool IsCompleted()
    {
        foreach (var objective in objectives)
        {
            if (!objective.IsCompleted())
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Reseta o progresso de todos os objetivos da quest
    /// </summary>
    public void ResetProgress()
    {
        foreach (var objective in objectives)
        {
            objective.ResetProgress();
        }
    }
}

/// <summary>
/// Tipos de quests disponíveis no jogo
/// </summary>
public enum QuestType
{
    Main,       // Quest principal da história
    Side,       // Quest secundária
    Daily,      // Quest diária
    Repeatable, // Quest que pode ser repetida
    Event       // Quest de evento especial
}

/// <summary>
/// Classe que representa um item de recompensa de quest
/// </summary>
[System.Serializable]
public class QuestItemReward
{
    public Item item;
    public int quantity = 1;
}