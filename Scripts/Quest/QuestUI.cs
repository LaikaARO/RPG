using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerencia a interface de usuário para o sistema de quests.
/// </summary>
public class QuestUI : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject questLogPanel;
    [SerializeField] private GameObject questPrefab;
    [SerializeField] private Transform questListContent;
    [SerializeField] private GameObject questDetailPanel;
    
    [Header("Elementos de Detalhes da Quest")]
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private Transform objectivesListContent;
    [SerializeField] private GameObject objectivePrefab;
    [SerializeField] private Transform rewardsListContent;
    [SerializeField] private GameObject rewardPrefab;
    
    [Header("Botões")]
    [SerializeField] private Button abandonQuestButton;
    [SerializeField] private Button completeQuestButton;
    
    // Referência ao QuestManager
    private QuestManager questManager;
    
    // Quest selecionada atualmente
    private QuestData selectedQuest;
    
    // Lista de itens de UI para quests
    private List<QuestListItem> questListItems = new List<QuestListItem>();
    
    private void Start()
    {
        // Obter referência ao QuestManager
        questManager = QuestManager.Instance;
        if (questManager == null)
        {
            Debug.LogError("QuestManager não encontrado!");
            return;
        }
        
        // Configurar botões
        abandonQuestButton.onClick.AddListener(AbandonSelectedQuest);
        completeQuestButton.onClick.AddListener(CompleteSelectedQuest);
        
        // Registrar nos eventos do QuestManager
        questManager.OnQuestAccepted += OnQuestAccepted;
        questManager.OnQuestCompleted += OnQuestCompleted;
        questManager.OnQuestFailed += OnQuestFailed;
        questManager.OnObjectiveUpdated += OnObjectiveUpdated;
        
        // Inicialmente, esconder o painel de detalhes
        questDetailPanel.SetActive(false);
        
        // Atualizar a lista de quests
        UpdateQuestList();
    }
    
    private void OnDestroy()
    {
        // Desregistrar dos eventos
        if (questManager != null)
        {
            questManager.OnQuestAccepted -= OnQuestAccepted;
            questManager.OnQuestCompleted -= OnQuestCompleted;
            questManager.OnQuestFailed -= OnQuestFailed;
            questManager.OnObjectiveUpdated -= OnObjectiveUpdated;
        }
    }
    
    /// <summary>
    /// Abre ou fecha o painel de quests
    /// </summary>
    public void ToggleQuestLog()
    {
        questLogPanel.SetActive(!questLogPanel.activeSelf);
        
        // Se estiver abrindo o painel, atualizar a lista
        if (questLogPanel.activeSelf)
        {
            UpdateQuestList();
        }
    }
    
    /// <summary>
    /// Atualiza a lista de quests ativas
    /// </summary>
    private void UpdateQuestList()
    {
        // Limpar lista atual
        foreach (var item in questListItems)
        {
            Destroy(item.gameObject);
        }
        questListItems.Clear();
        
        // Obter quests ativas do QuestManager
        List<QuestData> activeQuests = new List<QuestData>();
        
        // Criar item de UI para cada quest
        foreach (var questData in activeQuests)
        {
            GameObject questItemObj = Instantiate(questPrefab, questListContent);
            QuestListItem questItem = questItemObj.GetComponent<QuestListItem>();
            
            if (questItem != null)
            {
                questItem.Initialize(questData);
                questItem.OnQuestSelected += SelectQuest;
                questListItems.Add(questItem);
            }
        }
    }
    
    /// <summary>
    /// Seleciona uma quest para exibir detalhes
    /// </summary>
    public void SelectQuest(QuestData questData)
    {
        selectedQuest = questData;
        
        if (questData == null)
        {
            questDetailPanel.SetActive(false);
            return;
        }
        
        // Exibir painel de detalhes
        questDetailPanel.SetActive(true);
        
        // Atualizar informações da quest
        questTitleText.text = questData.Quest.questName;
        questDescriptionText.text = questData.Quest.description;
        
        // Limpar listas
        foreach (Transform child in objectivesListContent)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Transform child in rewardsListContent)
        {
            Destroy(child.gameObject);
        }
        
        // Adicionar objetivos
        foreach (var objective in questData.Objectives)
        {
            GameObject objectiveObj = Instantiate(objectivePrefab, objectivesListContent);
            QuestObjectiveUI objectiveUI = objectiveObj.GetComponent<QuestObjectiveUI>();
            
            if (objectiveUI != null)
            {
                objectiveUI.Initialize(objective);
            }
        }
        
        // Adicionar recompensas
        // Experiência
        if (questData.Quest.experienceReward > 0)
        {
            GameObject rewardObj = Instantiate(rewardPrefab, rewardsListContent);
            QuestRewardUI rewardUI = rewardObj.GetComponent<QuestRewardUI>();
            
            if (rewardUI != null)
            {
                rewardUI.Initialize("XP", questData.Quest.experienceReward.ToString());
            }
        }
        
        // Ouro
        if (questData.Quest.goldReward > 0)
        {
            GameObject rewardObj = Instantiate(rewardPrefab, rewardsListContent);
            QuestRewardUI rewardUI = rewardObj.GetComponent<QuestRewardUI>();
            
            if (rewardUI != null)
            {
                rewardUI.Initialize("Ouro", questData.Quest.goldReward.ToString());
            }
        }
        
        // Itens
        foreach (var itemReward in questData.Quest.itemRewards)
        {
            if (itemReward.item != null)
            {
                GameObject rewardObj = Instantiate(rewardPrefab, rewardsListContent);
                QuestRewardUI rewardUI = rewardObj.GetComponent<QuestRewardUI>();
                
                if (rewardUI != null)
                {
                    rewardUI.Initialize(itemReward.item.itemName, itemReward.quantity.ToString(), itemReward.item.icon);
                }
            }
        }
        
        // Atualizar botões
        UpdateButtons();
    }
    
    /// <summary>
    /// Atualiza o estado dos botões com base na quest selecionada
    /// </summary>
    private void UpdateButtons()
    {
        if (selectedQuest == null)
        {
            abandonQuestButton.interactable = false;
            completeQuestButton.interactable = false;
            return;
        }
        
        // Botão de abandonar sempre disponível para quests ativas
        abandonQuestButton.interactable = true;
        
        // Botão de completar disponível apenas se todos os objetivos estiverem concluídos
        completeQuestButton.interactable = selectedQuest.IsCompleted();
    }
    
    /// <summary>
    /// Abandona a quest selecionada
    /// </summary>
    private void AbandonSelectedQuest()
    {
        if (selectedQuest == null || questManager == null) return;
        
        if (questManager.AbandonQuest(selectedQuest.Quest.questID))
        {
            // Atualizar UI
            UpdateQuestList();
            questDetailPanel.SetActive(false);
            selectedQuest = null;
        }
    }
    
    /// <summary>
    /// Completa a quest selecionada
    /// </summary>
    private void CompleteSelectedQuest()
    {
        if (selectedQuest == null || questManager == null) return;
        
        if (questManager.CompleteQuest(selectedQuest.Quest.questID))
        {
            // Atualizar UI
            UpdateQuestList();
            questDetailPanel.SetActive(false);
            selectedQuest = null;
        }
    }
    
    /// <summary>
    /// Chamado quando uma quest é aceita
    /// </summary>
    private void OnQuestAccepted(QuestData questData)
    {
        UpdateQuestList();
    }
    
    /// <summary>
    /// Chamado quando uma quest é completada
    /// </summary>
    private void OnQuestCompleted(QuestData questData)
    {
        UpdateQuestList();
        
        // Se a quest completada era a selecionada, fechar o painel de detalhes
        if (selectedQuest != null && selectedQuest.Quest.questID == questData.Quest.questID)
        {
            questDetailPanel.SetActive(false);
            selectedQuest = null;
        }
    }
    
    /// <summary>
    /// Chamado quando um objetivo é atualizado
    /// </summary>
    private void OnObjectiveUpdated(QuestData questData, QuestObjective objective, int amount)
    {
        // Se a quest atualizada é a selecionada, atualizar os detalhes
        if (selectedQuest != null && selectedQuest.Quest.questID == questData.Quest.questID)
        {
            SelectQuest(questData); // Recarregar detalhes
        }
    }
    
    /// <summary>
    /// Chamado quando uma quest falha
    /// </summary>
    private void OnQuestFailed(QuestData questData)
    {
        UpdateQuestList();
        
        // Se a quest que falhou era a selecionada, fechar o painel de detalhes
        if (selectedQuest != null && selectedQuest.Quest.questID == questData.Quest.questID)
        {
            questDetailPanel.SetActive(false);
            selectedQuest = null;
        }
    }
}

/// <summary>
/// Componente para um item na lista de quests
/// </summary>
public class QuestListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI questTypeText;
    [SerializeField] private Image questIcon;
    [SerializeField] private Slider progressSlider;
    
    private QuestData questData;
    
    // Evento quando a quest é selecionada
    public System.Action<QuestData> OnQuestSelected;
    
    /// <summary>
    /// Inicializa o item com dados da quest
    /// </summary>
    public void Initialize(QuestData questData)
    {
        this.questData = questData;
        
        if (questData != null)
        {
            questNameText.text = questData.Quest.questName;
            questTypeText.text = questData.Quest.questType.ToString();
            
            // Calcular progresso
            float progress = CalculateProgress();
            progressSlider.value = progress;
            
            // Configurar botão
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnQuestSelected?.Invoke(questData));
            }
        }
    }
    
    /// <summary>
    /// Calcula o progresso da quest (0-1)
    /// </summary>
    private float CalculateProgress()
    {
        if (questData == null || questData.Objectives.Count == 0)
            return 0f;
        
        int completedObjectives = 0;
        foreach (var objective in questData.Objectives)
        {
            if (objective.IsCompleted())
                completedObjectives++;
        }
        
        return (float)completedObjectives / questData.Objectives.Count;
    }
}

/// <summary>
/// Componente para exibir um objetivo de quest na UI
/// </summary>
public class QuestObjectiveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image completionIcon;
    
    /// <summary>
    /// Inicializa o componente com dados do objetivo
    /// </summary>
    public void Initialize(QuestObjective objective)
    {
        if (objective != null)
        {
            objectiveText.text = objective.description;
            progressText.text = $"{objective.currentAmount}/{objective.requiredAmount}";
            
            // Atualizar ícone de conclusão
            if (completionIcon != null)
            {
                completionIcon.enabled = objective.IsCompleted();
            }
        }
    }
}

/// <summary>
/// Componente para exibir uma recompensa de quest na UI
/// </summary>
public class QuestRewardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardNameText;
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    [SerializeField] private Image rewardIcon;
    
    /// <summary>
    /// Inicializa o componente com dados da recompensa
    /// </summary>
    public void Initialize(string name, string amount, Sprite icon = null)
    {
        rewardNameText.text = name;
        rewardAmountText.text = amount;
        
        if (rewardIcon != null)
        {
            rewardIcon.enabled = icon != null;
            rewardIcon.sprite = icon;
        }
    }
}