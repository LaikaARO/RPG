/*using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Classe de exemplo que demonstra como criar e configurar quests no jogo.
/// </summary>
public class ExampleQuest : MonoBehaviour
{
    // Referência ao QuestManager
    private QuestManager questManager;
    
    // Referência ao IDManager
    private IDManager idManager;
    
    // Quests de exemplo
    [SerializeField] private Quest exampleKillQuest;
    [SerializeField] private Quest exampleCollectQuest;
    
    private void Start()
    {
        // Obter referências
        questManager = QuestManager.Instance;
        idManager = IDManager.Instance;
        
        if (questManager == null || idManager == null)
        {
            Debug.LogError("QuestManager ou IDManager não encontrado!");
            return;
        }
        
        // Criar quests de exemplo se não existirem
        if (exampleKillQuest == null)
        {
            CreateExampleKillQuest();
        }
        
        if (exampleCollectQuest == null)
        {
            CreateExampleCollectQuest();
        }
    }
    
    /// <summary>
    /// Cria uma quest de exemplo do tipo "matar inimigos"
    /// </summary>
    private void CreateExampleKillQuest()
    {
        // Criar a quest
        exampleKillQuest = ScriptableObject.CreateInstance<Quest>();
        
        // Configurar propriedades básicas
        exampleKillQuest.questID = 1; // ID dentro do intervalo de quests (1-999)
        exampleKillQuest.questName = "Caçador de Lobos";
        exampleKillQuest.description = "Os lobos estão atacando os viajantes na estrada. Elimine alguns deles para tornar a região mais segura.";
        exampleKillQuest.questType = QuestType.Side;
        exampleKillQuest.requiredLevel = 1;
        exampleKillQuest.timeLimit = 0; // Sem tempo limite
        
        // Criar objetivo de matar
        KillObjective killObjective = new KillObjective
        {
            description = "Mate lobos selvagens",
            requiredAmount = 5,
            enemyID = 1001, // ID dentro do intervalo de mobs (1000-9999)
            enemyName = "Lobo Selvagem"
        };
        
        // Adicionar objetivo à quest
        exampleKillQuest.objectives = new List<QuestObjective> { killObjective };
        
        // Configurar recompensas
        exampleKillQuest.experienceReward = 100;
        exampleKillQuest.goldReward = 50;
        
        // Registrar a quest no IDManager
        if (idManager != null)
        {
            idManager.RegisterQuest(exampleKillQuest, exampleKillQuest.questID);
        }
        
        // Salvar a quest como asset
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(exampleKillQuest, "Assets/Resources/Quests/KillWolvesQuest.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        #endif
    }
    
    /// <summary>
    /// Cria uma quest de exemplo do tipo "coletar itens"
    /// </summary>
    private void CreateExampleCollectQuest()
    {
        // Criar a quest
        exampleCollectQuest = ScriptableObject.CreateInstance<Quest>();
        
        // Configurar propriedades básicas
        exampleCollectQuest.questID = 2; // ID dentro do intervalo de quests (1-999)
        exampleCollectQuest.questName = "Ervas Medicinais";
        exampleCollectQuest.description = "O curandeiro da vila precisa de ervas medicinais para preparar poções. Colete algumas para ele.";
        exampleCollectQuest.questType = QuestType.Repeatable;
        exampleCollectQuest.requiredLevel = 1;
        
        // Criar objetivo de coleta
        CollectObjective collectObjective = new CollectObjective
        {
            description = "Colete ervas medicinais",
            requiredAmount = 10,
            itemID = 10001 // ID dentro do intervalo de itens (10000-19999)
        };
        
        // Adicionar objetivo à quest
        exampleCollectQuest.objectives = new List<QuestObjective> { collectObjective };
        
        // Configurar recompensas
        exampleCollectQuest.experienceReward = 75;
        exampleCollectQuest.goldReward = 30;
        
        // Adicionar recompensa de item (poção de cura)
        Item healthPotion = Resources.Load<Item>("HealthPotion");
        if (healthPotion != null)
        {
            QuestItemReward itemReward = new QuestItemReward
            {
                item = healthPotion,
                quantity = 2
            };
            
            exampleCollectQuest.itemRewards = new List<QuestItemReward> { itemReward };
        }
        
        // Registrar a quest no IDManager
        if (idManager != null)
        {
            idManager.RegisterQuest(exampleCollectQuest, exampleCollectQuest.questID);
        }
        
        // Salvar a quest como asset
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(exampleCollectQuest, "Assets/Resources/Quests/CollectHerbsQuest.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        #endif
    }
    
    /// <summary>
    /// Método para aceitar a quest de matar lobos (pode ser chamado por um botão na UI)
    /// </summary>
    public void AcceptKillWolvesQuest()
    {
        if (questManager != null && exampleKillQuest != null)
        {
            questManager.AcceptQuest(exampleKillQuest);
        }
    }
    
    /// <summary>
    /// Método para aceitar a quest de coletar ervas (pode ser chamado por um botão na UI)
    /// </summary>
    public void AcceptCollectHerbsQuest()
    {
        if (questManager != null && exampleCollectQuest != null)
        {
            questManager.AcceptQuest(exampleCollectQuest);
        }
    }
    
    /// <summary>
    /// Método para simular a morte de um lobo (para teste)
    /// </summary>
    public void SimulateWolfKill()
    {
        if (questManager == null) return;
        
        // Simular a morte de um lobo atualizando os objetivos de kill
        foreach (var questData in questManager.GetActiveQuests())
        {
            foreach (var objective in questData.Objectives)
            {
                if (objective is KillObjective killObjective && killObjective.enemyID == 1001)
                {
                    questManager.UpdateObjective(questData, objective, 1);
                    Debug.Log("Lobo morto! Progresso atualizado.");
                }
            }
        }
    }
    
    /// <summary>
    /// Método para simular a coleta de uma erva (para teste)
    /// </summary>
    public void SimulateHerbCollection()
    {
        if (questManager == null) return;
        
        // Simular a coleta de uma erva atualizando os objetivos de coleta
        foreach (var questData in questManager.GetActiveQuests())
        {
            foreach (var objective in questData.Objectives)
            {
                if (objective is CollectObjective collectObjective && collectObjective.itemID == 10001)
                {
                    questManager.UpdateObjective(questData, objective, 1);
                    Debug.Log("Erva coletada! Progresso atualizado.");
                }
            }
        }
    }
}
*/