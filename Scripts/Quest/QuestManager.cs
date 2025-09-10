using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Gerencia todas as quests do jogador, incluindo quests ativas e completadas.
/// </summary>
public class QuestManager : MonoBehaviour
{
    // Singleton para acesso global
    public static QuestManager Instance { get; private set; }
    
    [Header("Configurações")]
    [Tooltip("Número máximo de quests ativas simultaneamente")]
    [SerializeField] private int maxActiveQuests = 10;
    
    // Listas de quests
    [SerializeField] private List<QuestData> activeQuests = new List<QuestData>();
    [SerializeField] private List<QuestData> completedQuests = new List<QuestData>();
    
    // Referências
    private CharacterStats playerStats;
    private LevelSystem playerLevelSystem;
    
    // Eventos
    public event Action<QuestData> OnQuestAccepted;
    public event Action<QuestData> OnQuestCompleted;
    public event Action<QuestData> OnQuestFailed;
    public event Action<QuestData, QuestObjective> OnObjectiveCompleted;
    public event Action<QuestData, QuestObjective, int> OnObjectiveUpdated;
    
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
        
        // Obter referência ao jogador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
            playerLevelSystem = player.GetComponent<LevelSystem>();
        }
    }
    
    private void Start()
    {
        // Registrar nos eventos necessários
        RegisterEvents();
    }
    
    private void OnDestroy()
    {
        // Desregistrar dos eventos
        UnregisterEvents();
    }
    
    /// <summary>
    /// Registra nos eventos necessários para atualizar objetivos de quests
    /// </summary>
    private void RegisterEvents()
    {
        // Exemplo: registrar no evento de morte de inimigos
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            enemy.OnDeath += (enemyController) => OnEnemyKilled(enemyController);
        }
        
        // Outros eventos podem ser registrados aqui
    }
    
    /// <summary>
    /// Desregistra dos eventos
    /// </summary>
    private void UnregisterEvents()
    {
        // Implementar desregistro de eventos se necessário
    }
    
    /// <summary>
    /// Aceita uma nova quest
    /// </summary>
    public bool AcceptQuest(Quest quest)
    {
        // Verificar se já atingiu o limite de quests ativas
        if (activeQuests.Count >= maxActiveQuests)
        {
            Debug.LogWarning("Limite máximo de quests ativas atingido!");
            return false;
        }
        
        // Verificar se a quest já está ativa
        if (IsQuestActive(quest.questID))
        {
            Debug.LogWarning($"Quest {quest.questName} já está ativa!");
            return false;
        }
        
        // Verificar se a quest já foi completada e não é repetível
        if (IsQuestCompleted(quest.questID) && quest.questType != QuestType.Repeatable && quest.questType != QuestType.Daily)
        {
            Debug.LogWarning($"Quest {quest.questName} já foi completada e não é repetível!");
            return false;
        }
        
        // Verificar pré-requisitos
        foreach (var prerequisite in quest.prerequisites)
        {
            if (!IsQuestCompleted(prerequisite.questID))
            {
                Debug.LogWarning($"Pré-requisito não atendido: {prerequisite.questName}");
                return false;
            }
        }
        
        // Verificar nível mínimo
        if (playerLevelSystem != null && playerLevelSystem.Level < quest.requiredLevel)
        {
            Debug.LogWarning($"Nível insuficiente para a quest {quest.questName}. Requer nível {quest.requiredLevel}");
            return false;
        }
        
        // Criar dados da quest
        QuestData questData = new QuestData(quest);
        activeQuests.Add(questData);
        
        // Disparar evento
        OnQuestAccepted?.Invoke(questData);
        
        Debug.Log($"Quest aceita: {quest.questName}");
        return true;
    }
    
    /// <summary>
    /// Completa uma quest ativa e concede as recompensas
    /// </summary>
    public bool CompleteQuest(int questID)
    {
        QuestData questData = GetActiveQuest(questID);
        if (questData == null)
        {
            Debug.LogWarning($"Quest com ID {questID} não está ativa!");
            return false;
        }
        
        // Verificar se todos os objetivos foram concluídos
        if (!questData.IsCompleted())
        {
            Debug.LogWarning($"Nem todos os objetivos da quest {questData.Quest.questName} foram concluídos!");
            return false;
        }
        
        // Conceder recompensas
        GiveRewards(questData);
        
        // Mover para a lista de quests completadas
        activeQuests.Remove(questData);
        completedQuests.Add(questData);
        
        // Disparar evento
        OnQuestCompleted?.Invoke(questData);
        
        Debug.Log($"Quest completada: {questData.Quest.questName}");
        return true;
    }
    
    /// <summary>
    /// Concede as recompensas de uma quest
    /// </summary>
    private void GiveRewards(QuestData questData)
    {
        if (playerStats == null) return;
        
        Quest quest = questData.Quest;
        
        // Conceder experiência
        if (quest.experienceReward > 0)
        {
            // Implementar ganho de experiência
            Debug.Log($"Ganhou {quest.experienceReward} de experiência");
        }
        
        // Conceder ouro
        if (quest.goldReward > 0)
        {
            // Implementar ganho de ouro
            Debug.Log($"Ganhou {quest.goldReward} de ouro");
        }
        
        // Conceder itens
        foreach (var itemReward in quest.itemRewards)
        {
            if (itemReward.item != null)
            {
                // Implementar adição de item ao inventário
                Debug.Log($"Ganhou {itemReward.quantity}x {itemReward.item.itemName}");
            }
        }
    }
    
    /// <summary>
    /// Abandona uma quest ativa
    /// </summary>
    public bool AbandonQuest(int questID)
    {
        QuestData questData = GetActiveQuest(questID);
        if (questData == null)
        {
            Debug.LogWarning($"Quest com ID {questID} não está ativa!");
            return false;
        }
        
        activeQuests.Remove(questData);
        
        // Disparar evento de quest abandonada
        OnQuestFailed?.Invoke(questData);
        
        Debug.Log($"Quest abandonada: {questData.Quest.questName}");
        return true;
    }
    
    /// <summary>
    /// Falha uma quest ativa (por exemplo, quando um tempo limite é atingido)
    /// </summary>
    public bool FailQuest(int questID, string reason = "")
    {
        QuestData questData = GetActiveQuest(questID);
        if (questData == null)
        {
            Debug.LogWarning($"Quest com ID {questID} não está ativa!");
            return false;
        }
        
        activeQuests.Remove(questData);
        
        // Disparar evento de quest falhada
        OnQuestFailed?.Invoke(questData);
        
        Debug.Log($"Quest falhou: {questData.Quest.questName}. Motivo: {reason}");
        return true;
    }
    
    /// <summary>
    /// Obtém uma cópia da lista de quests ativas (segura para iteração)
    /// </summary>
    public List<QuestData> GetActiveQuests()
    {
        return new List<QuestData>(activeQuests);
    }
    
    /// <summary>
    /// Verifica se uma quest está ativa
    /// </summary>
    public bool IsQuestActive(int questID)
    {
        return GetActiveQuest(questID) != null;
    }
    
    /// <summary>
    /// Verifica se uma quest foi completada
    /// </summary>
    public bool IsQuestCompleted(int questID)
    {
        return GetCompletedQuest(questID) != null;
    }
    
    /// <summary>
    /// Obtém uma quest ativa pelo ID
    /// </summary>
    public QuestData GetActiveQuest(int questID)
    {
        return activeQuests.Find(q => q.Quest.questID == questID);
    }
    
    /// <summary>
    /// Obtém uma quest completada pelo ID
    /// </summary>
    public QuestData GetCompletedQuest(int questID)
    {
        return completedQuests.Find(q => q.Quest.questID == questID);
    }
    
    /// <summary>
    /// Retorna uma cópia da lista de quests completadas (segura para iteração)
    /// </summary>
    public List<QuestData> GetCompletedQuests()
    {
        return new List<QuestData>(completedQuests);
    }
    
    /// <summary>
    /// Chamado quando um inimigo é derrotado
    /// </summary>
    private void OnEnemyKilled(EnemyController enemy)
    {
        // Obter ID do inimigo (implementar método para obter ID)
        int enemyID = GetEnemyID(enemy);
        
        // Atualizar objetivos de matar
        foreach (var questData in activeQuests)
        {
            foreach (var objective in questData.Objectives)
            {
                if (objective is KillObjective killObjective && killObjective.enemyID == enemyID)
                {
                    UpdateObjective(questData, objective, 1);
                }
            }
        }
    }
    
    /// <summary>
    /// Chamado quando um item é coletado
    /// </summary>
    public void OnItemCollected(Item item, int quantity)
    {
        // Obter ID do item (implementar método para obter ID)
        int itemID = GetItemID(item);
        
        // Atualizar objetivos de coleta
        foreach (var questData in activeQuests)
        {
            foreach (var objective in questData.Objectives)
            {
                if (objective is CollectObjective collectObjective && collectObjective.itemID == itemID)
                {
                    UpdateObjective(questData, objective, quantity);
                }
            }
        }
    }
    
    /// <summary>
    /// Chamado quando o jogador fala com um NPC
    /// </summary>
    public void OnTalkToNPC(int npcID)
    {
        // Atualizar objetivos de conversa
        foreach (var questData in activeQuests)
        {
            foreach (var objective in questData.Objectives)
            {
                if (objective is TalkObjective talkObjective && talkObjective.npcID == npcID)
                {
                    UpdateObjective(questData, objective, 1);
                }
            }
        }
    }
    
    /// <summary>
    /// Chamado quando o jogador explora uma área
    /// </summary>
    public void OnAreaExplored(string areaName)
    {
        // Atualizar objetivos de exploração
        foreach (var questData in activeQuests)
        {
            foreach (var objective in questData.Objectives)
            {
                if (objective is ExploreObjective exploreObjective && exploreObjective.areaName == areaName)
                {
                    UpdateObjective(questData, objective, 1);
                }
            }
        }
    }
    
    /// <summary>
    /// Atualiza o progresso de um objetivo
    /// </summary>
    public void UpdateObjective(QuestData questData, QuestObjective objective, int amount)
    {
        // Atualizar progresso
        objective.UpdateProgress(amount);
        
        // Disparar evento de atualização
        OnObjectiveUpdated?.Invoke(questData, objective, amount);
        
        // Verificar se o objetivo foi completado
        if (objective.IsCompleted())
        {
            OnObjectiveCompleted?.Invoke(questData, objective);
            
            // Verificar se a quest foi completada
            if (questData.IsCompleted())
            {
                // Não completar automaticamente, deixar para o jogador entregar a quest
                Debug.Log($"Todos os objetivos da quest {questData.Quest.questName} foram concluídos!");
            }
        }
    }
    
    /// <summary>
    /// Obtém o ID de um inimigo
    /// </summary>
    private int GetEnemyID(EnemyController enemy)
    {
        // Implementar lógica para obter ID do inimigo
        // Por enquanto, retorna um ID temporário
        return 1000; // Temporário
    }
    
    /// <summary>
    /// Obtém o ID de um item
    /// </summary>
    private int GetItemID(Item item)
    {
        // Implementar lógica para obter ID do item
        // Por enquanto, retorna um ID temporário
        return 10000; // Temporário
    }
}

/// <summary>
/// Classe que armazena dados de uma quest em andamento
/// </summary>
[System.Serializable]
public class QuestData
{
    [SerializeField] private Quest quest;
    [SerializeField] private List<QuestObjective> objectives = new List<QuestObjective>();
    [SerializeField] private System.DateTime acceptedTime;
    
    public Quest Quest => quest;
    public List<QuestObjective> Objectives => objectives;
    public System.DateTime AcceptedTime => acceptedTime;
    
    public QuestData(Quest quest)
    {
        this.quest = quest;
        this.acceptedTime = System.DateTime.Now;
        
        // Copiar objetivos da quest
        foreach (var objective in quest.objectives)
        {
            objectives.Add(objective);
        }
    }
    
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
}