using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema para monitorar quests com tempo limite e falhar automaticamente quests expiradas
/// </summary>
public class QuestTimer : MonoBehaviour
{
    [Tooltip("Intervalo em segundos para verificar quests expiradas")]
    [SerializeField] private float checkInterval = 30f;
    
    private QuestManager questManager;
    private Coroutine timerCoroutine;
    
    private void Awake()
    {
        questManager = QuestManager.Instance;
        if (questManager == null)
        {
            Debug.LogError("QuestManager não encontrado!");
            enabled = false;
            return;
        }
    }
    
    private void OnEnable()
    {
        // Iniciar a verificação periódica
        if (timerCoroutine == null)
        {
            timerCoroutine = StartCoroutine(CheckQuestTimers());
        }
        
        // Registrar no evento de quest aceita para monitorar novas quests
        if (questManager != null)
        {
            questManager.OnQuestAccepted += OnQuestAccepted;
        }
    }
    
    private void OnDisable()
    {
        // Parar a verificação periódica
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // Desregistrar do evento
        if (questManager != null)
        {
            questManager.OnQuestAccepted -= OnQuestAccepted;
        }
    }
    
    /// <summary>
    /// Chamado quando uma nova quest é aceita
    /// </summary>
    private void OnQuestAccepted(QuestData questData)
    {
        // Verificar se a quest tem tempo limite
        if (questData.Quest.timeLimit > 0)
        {
            Debug.Log($"Quest com tempo limite aceita: {questData.Quest.questName}. Tempo limite: {questData.Quest.timeLimit} minutos");
        }
    }
    
    /// <summary>
    /// Verifica periodicamente se alguma quest expirou
    /// </summary>
    private IEnumerator CheckQuestTimers()
    {
        while (true)
        {
            // Aguardar o intervalo configurado
            yield return new WaitForSeconds(checkInterval);
            
            // Verificar todas as quests ativas
            CheckExpiredQuests();
        }
    }
    
    /// <summary>
    /// Verifica se alguma quest ativa expirou
    /// </summary>
    private void CheckExpiredQuests()
    {
        if (questManager == null) return;
        
        List<QuestData> activeQuests = questManager.GetActiveQuests();
        List<QuestData> expiredQuests = new List<QuestData>();
        
        foreach (var questData in activeQuests)
        {
            // Verificar se a quest tem tempo limite
            if (questData.Quest.timeLimit <= 0) continue;
            
            // Calcular o tempo decorrido desde que a quest foi aceita
            float elapsedMinutes = (float)(System.DateTime.Now - questData.AcceptedTime).TotalMinutes;
            
            // Verificar se o tempo limite foi atingido
            if (elapsedMinutes >= questData.Quest.timeLimit)
            {
                expiredQuests.Add(questData);
            }
        }
        
        // Falhar todas as quests expiradas
        foreach (var questData in expiredQuests)
        {
            questManager.FailQuest(questData.Quest.questID, "Tempo limite atingido");
            Debug.Log($"Quest expirada: {questData.Quest.questName}. Tempo decorrido: {(System.DateTime.Now - questData.AcceptedTime).TotalMinutes:F1} minutos");
        }
    }
}