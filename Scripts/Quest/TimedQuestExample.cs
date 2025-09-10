/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Exemplo de criação de uma quest com tempo limite
/// </summary>
public class TimedQuestExample : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    
    // Quest com tempo limite
    private Quest timedQuest;
    
    private void Start()
    {
        if (questManager == null)
        {
            questManager = QuestManager.Instance;
        }
        
        // Criar a quest com tempo limite
        CreateTimedQuest();
    }
    
    /// <summary>
    /// Cria uma quest com tempo limite
    /// </summary>
    private void CreateTimedQuest()
    {
        // Criar a quest
        timedQuest = ScriptableObject.CreateInstance<Quest>();
        
        // Configurar propriedades básicas
        timedQuest.questID = 5; // ID dentro do intervalo de quests (1-999)
        timedQuest.questName = "Entrega Urgente";
        timedQuest.description = "Entregue este pacote urgente ao mensageiro na cidade vizinha antes que seja tarde demais!";
        timedQuest.questType = QuestType.Side;
        timedQuest.requiredLevel = 1;
        timedQuest.timeLimit = 5f; // 5 minutos de tempo limite
        
        // Criar objetivo de entrega
        CollectObjective deliverObjective = new CollectObjective
        {
            description = "Entregar pacote ao mensageiro",
            requiredAmount = 1,
            itemID = 1001, // ID de um item de pacote
            itemName = "Pacote Urgente"
        };
        
        // Adicionar objetivo à quest
        timedQuest.objectives = new List<QuestObjective> { deliverObjective };
        
        // Configurar recompensas
        timedQuest.experienceReward = 150;
        timedQuest.goldReward = 75;
        
        // Registrar a quest no IDManager
        IDManager idManager = IDManager.Instance;
        if (idManager != null)
        {
            idManager.RegisterQuest(timedQuest, timedQuest.questID);
        }
        
        Debug.Log($"Quest com tempo limite criada: {timedQuest.questName} (ID: {timedQuest.questID}). Tempo limite: {timedQuest.timeLimit} minutos");
    }
    
    /// <summary>
    /// Aceita a quest com tempo limite (para testes)
    /// </summary>
    public void AcceptTimedQuest()
    {
        if (questManager != null && timedQuest != null)
        {
            bool accepted = questManager.AcceptQuest(timedQuest);
            if (accepted)
            {
                Debug.Log($"Quest com tempo limite aceita: {timedQuest.questName}. Você tem {timedQuest.timeLimit} minutos para completá-la.");
            }
        }
    }
}
*/