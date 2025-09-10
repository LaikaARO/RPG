/*using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
/// <summary>
/// Editor script para criar prefabs de UI para o sistema de quests.
/// </summary>
public class QuestPrefabCreator : MonoBehaviour
{
    [MenuItem("RPG/Create Quest UI Prefabs")]
    public static void CreateQuestUIPrefabs()
    {
        // Criar diretório se não existir
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Quests"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Quests");
        }
        
        // Criar prefab de item de quest
        CreateQuestItemPrefab();
        
        // Criar prefab de objetivo de quest
        CreateQuestObjectivePrefab();
        
        // Criar prefab de recompensa de quest
        CreateQuestRewardPrefab();
        
        // Criar prefab do painel de quests
        CreateQuestLogPrefab();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Prefabs de UI para quests criados com sucesso!");
    }
    
    /// <summary>
    /// Cria o prefab de item de quest para a lista
    /// </summary>
    private static void CreateQuestItemPrefab()
    {
        // Criar objeto base
        GameObject questItemObj = new GameObject("QuestItem");
        questItemObj.AddComponent<RectTransform>().sizeDelta = new Vector2(400, 60);
        
        // Adicionar componentes
        Image background = questItemObj.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Button button = questItemObj.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1, 1, 1, 1);
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1);
        colors.selectedColor = new Color(0.8f, 0.8f, 0.8f, 1);
        button.colors = colors;
        
        // Adicionar componente de QuestListItem
        questItemObj.AddComponent<QuestListItem>();
        
        // Criar ícone
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(questItemObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(30, 0);
        iconRect.sizeDelta = new Vector2(40, 40);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.white;
        
        // Criar texto do nome
        GameObject nameObj = new GameObject("QuestName");
        nameObj.transform.SetParent(questItemObj.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 0.5f);
        nameRect.pivot = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = new Vector2(50, 10);
        nameRect.sizeDelta = new Vector2(-100, 20);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 14;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.text = "Nome da Quest";
        
        // Criar texto do tipo
        GameObject typeObj = new GameObject("QuestType");
        typeObj.transform.SetParent(questItemObj.transform, false);
        RectTransform typeRect = typeObj.AddComponent<RectTransform>();
        typeRect.anchorMin = new Vector2(0, 0.5f);
        typeRect.anchorMax = new Vector2(1, 0.5f);
        typeRect.pivot = new Vector2(0.5f, 0.5f);
        typeRect.anchoredPosition = new Vector2(50, -10);
        typeRect.sizeDelta = new Vector2(-100, 20);
        TextMeshProUGUI typeText = typeObj.AddComponent<TextMeshProUGUI>();
        typeText.fontSize = 12;
        typeText.alignment = TextAlignmentOptions.Left;
        typeText.text = "Tipo da Quest";
        typeText.color = new Color(0.7f, 0.7f, 0.7f);
        
        // Criar barra de progresso
        GameObject progressObj = new GameObject("Progress");
        progressObj.transform.SetParent(questItemObj.transform, false);
        RectTransform progressRect = progressObj.AddComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(1, 0.5f);
        progressRect.anchorMax = new Vector2(1, 0.5f);
        progressRect.pivot = new Vector2(1, 0.5f);
        progressRect.anchoredPosition = new Vector2(-10, 0);
        progressRect.sizeDelta = new Vector2(100, 10);
        Slider progressSlider = progressObj.AddComponent<Slider>();
        progressSlider.value = 0.5f;
        
        // Criar background da barra
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(progressObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f);
        progressSlider.targetGraphic = bgImage;
        
        // Criar fill da barra
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(progressObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.7f, 0.2f);
        progressSlider.fillRect = fillRect;
        
        // Salvar prefab
        string prefabPath = "Assets/Resources/Quests/QuestItem.prefab";
        PrefabUtility.SaveAsPrefabAsset(questItemObj, prefabPath);
        GameObject.DestroyImmediate(questItemObj);
    }
    
    /// <summary>
    /// Cria o prefab de objetivo de quest
    /// </summary>
    private static void CreateQuestObjectivePrefab()
    {
        // Criar objeto base
        GameObject objectiveObj = new GameObject("QuestObjective");
        objectiveObj.AddComponent<RectTransform>().sizeDelta = new Vector2(400, 30);
        
        // Adicionar componente de QuestObjectiveUI
        objectiveObj.AddComponent<QuestObjectiveUI>();
        
        // Criar ícone de conclusão
        GameObject iconObj = new GameObject("CompletionIcon");
        iconObj.transform.SetParent(objectiveObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(15, 0);
        iconRect.sizeDelta = new Vector2(20, 20);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = new Color(0.2f, 0.8f, 0.2f);
        
        // Criar texto do objetivo
        GameObject textObj = new GameObject("ObjectiveText");
        textObj.transform.SetParent(objectiveObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(50, 0);
        textRect.sizeDelta = new Vector2(-100, 20);
        TextMeshProUGUI objectiveText = textObj.AddComponent<TextMeshProUGUI>();
        objectiveText.fontSize = 14;
        objectiveText.alignment = TextAlignmentOptions.Left;
        objectiveText.text = "Descrição do objetivo";
        
        // Criar texto do progresso
        GameObject progressObj = new GameObject("ProgressText");
        progressObj.transform.SetParent(objectiveObj.transform, false);
        RectTransform progressRect = progressObj.AddComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(1, 0.5f);
        progressRect.anchorMax = new Vector2(1, 0.5f);
        progressRect.pivot = new Vector2(1, 0.5f);
        progressRect.anchoredPosition = new Vector2(-10, 0);
        progressRect.sizeDelta = new Vector2(60, 20);
        TextMeshProUGUI progressText = progressObj.AddComponent<TextMeshProUGUI>();
        progressText.fontSize = 14;
        progressText.alignment = TextAlignmentOptions.Right;
        progressText.text = "0/5";
        
        // Salvar prefab
        string prefabPath = "Assets/Resources/Quests/QuestObjective.prefab";
        PrefabUtility.SaveAsPrefabAsset(objectiveObj, prefabPath);
        GameObject.DestroyImmediate(objectiveObj);
    }
    
    /// <summary>
    /// Cria o prefab de recompensa de quest
    /// </summary>
    private static void CreateQuestRewardPrefab()
    {
        // Criar objeto base
        GameObject rewardObj = new GameObject("QuestReward");
        rewardObj.AddComponent<RectTransform>().sizeDelta = new Vector2(400, 30);
        
        // Adicionar componente de QuestRewardUI
        rewardObj.AddComponent<QuestRewardUI>();
        
        // Criar ícone da recompensa
        GameObject iconObj = new GameObject("RewardIcon");
        iconObj.transform.SetParent(rewardObj.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(15, 0);
        iconRect.sizeDelta = new Vector2(20, 20);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.white;
        
        // Criar texto do nome da recompensa
        GameObject nameObj = new GameObject("RewardName");
        nameObj.transform.SetParent(rewardObj.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 0.5f);
        nameRect.pivot = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = new Vector2(50, 0);
        nameRect.sizeDelta = new Vector2(-100, 20);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 14;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.text = "Nome da Recompensa";
        
        // Criar texto da quantidade
        GameObject amountObj = new GameObject("RewardAmount");
        amountObj.transform.SetParent(rewardObj.transform, false);
        RectTransform amountRect = amountObj.AddComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(1, 0.5f);
        amountRect.anchorMax = new Vector2(1, 0.5f);
        amountRect.pivot = new Vector2(1, 0.5f);
        amountRect.anchoredPosition = new Vector2(-10, 0);
        amountRect.sizeDelta = new Vector2(60, 20);
        TextMeshProUGUI amountText = amountObj.AddComponent<TextMeshProUGUI>();
        amountText.fontSize = 14;
        amountText.alignment = TextAlignmentOptions.Right;
        amountText.text = "x1";
        
        // Salvar prefab
        string prefabPath = "Assets/Resources/Quests/QuestReward.prefab";
        PrefabUtility.SaveAsPrefabAsset(rewardObj, prefabPath);
        GameObject.DestroyImmediate(rewardObj);
    }
    
    /// <summary>
    /// Cria o prefab do painel de quests
    /// </summary>
    private static void CreateQuestLogPrefab()
    {
        // Criar objeto base
        GameObject questLogObj = new GameObject("QuestLog");
        questLogObj.AddComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
        
        // Adicionar componente de QuestUI
        questLogObj.AddComponent<QuestUI>();
        
        // Adicionar painel de fundo
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(questLogObj.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Criar título
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(0, 40);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.fontSize = 24;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.text = "MISSÕES";
        
        // Criar painel de lista de quests
        GameObject listPanelObj = new GameObject("QuestListPanel");
        listPanelObj.transform.SetParent(panelObj.transform, false);
        RectTransform listPanelRect = listPanelObj.AddComponent<RectTransform>();
        listPanelRect.anchorMin = new Vector2(0, 0);
        listPanelRect.anchorMax = new Vector2(0.4f, 1);
        listPanelRect.pivot = new Vector2(0.5f, 0.5f);
        listPanelRect.anchoredPosition = new Vector2(0, -20);
        listPanelRect.sizeDelta = new Vector2(-20, -80);
        Image listPanelImage = listPanelObj.AddComponent<Image>();
        listPanelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        
        // Criar scroll view para lista de quests
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(listPanelObj.transform, false);
        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = Vector2.zero;
        scrollViewRect.anchorMax = Vector2.one;
        scrollViewRect.sizeDelta = new Vector2(-20, -20);
        scrollViewRect.anchoredPosition = Vector2.zero;
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        
        // Criar viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        scrollRect.viewport = viewportRect;
        
        // Criar content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 300);
        VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(5, 5, 5, 5);
        contentLayout.spacing = 5;
        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRect;
        
        // Criar painel de detalhes da quest
        GameObject detailPanelObj = new GameObject("QuestDetailPanel");
        detailPanelObj.transform.SetParent(panelObj.transform, false);
        RectTransform detailPanelRect = detailPanelObj.AddComponent<RectTransform>();
        detailPanelRect.anchorMin = new Vector2(0.4f, 0);
        detailPanelRect.anchorMax = new Vector2(1, 1);
        detailPanelRect.pivot = new Vector2(0.5f, 0.5f);
        detailPanelRect.anchoredPosition = new Vector2(0, -20);
        detailPanelRect.sizeDelta = new Vector2(-20, -80);
        Image detailPanelImage = detailPanelObj.AddComponent<Image>();
        detailPanelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        
        // Criar título da quest
        GameObject questTitleObj = new GameObject("QuestTitle");
        questTitleObj.transform.SetParent(detailPanelObj.transform, false);
        RectTransform questTitleRect = questTitleObj.AddComponent<RectTransform>();
        questTitleRect.anchorMin = new Vector2(0, 1);
        questTitleRect.anchorMax = new Vector2(1, 1);
        questTitleRect.pivot = new Vector2(0.5f, 1);
        questTitleRect.anchoredPosition = new Vector2(0, -20);
        questTitleRect.sizeDelta = new Vector2(-40, 30);
        TextMeshProUGUI questTitleText = questTitleObj.AddComponent<TextMeshProUGUI>();
        questTitleText.fontSize = 18;
        questTitleText.alignment = TextAlignmentOptions.Left;
        questTitleText.text = "Título da Quest";
        
        // Criar descrição da quest
        GameObject questDescObj = new GameObject("QuestDescription");
        questDescObj.transform.SetParent(detailPanelObj.transform, false);
        RectTransform questDescRect = questDescObj.AddComponent<RectTransform>();
        questDescRect.anchorMin = new Vector2(0, 1);
        questDescRect.anchorMax = new Vector2(1, 1);
        questDescRect.pivot = new Vector2(0.5f, 1);
        questDescRect.anchoredPosition = new Vector2(0, -70);
        questDescRect.sizeDelta = new Vector2(-40, 80);
        TextMeshProUGUI questDescText = questDescObj.AddComponent<TextMeshProUGUI>();
        questDescText.fontSize = 14;
        questDescText.alignment = TextAlignmentOptions.Left;
        questDescText.text = "Descrição detalhada da quest...";
        
        // Criar título dos objetivos
        GameObject objectivesTitleObj = new GameObject("ObjectivesTitle");
        objectivesTitleObj.transform.SetParent(detailPanelObj.transform, false);
        RectTransform objectivesTitleRect = objectivesTitleObj.AddComponent<RectTransform>();
        objectivesTitleRect.anchorMin = new Vector2(0, 1);
        objectivesTitleRect.anchorMax = new Vector2(1, 1);
        objectivesTitleRect.pivot = new Vector2(0.5f, 1);
        objectivesTitleRect.anchoredPosition = new Vector2(0, -170);
        objectivesTitleRect.sizeDelta = new Vector2(-40, 30);
        TextMeshProUGUI objectivesTitleText = objectivesTitleObj.AddComponent<TextMeshProUGUI>();
        objectivesTitleText.fontSize = 16;
        objectivesTitleText.alignment = TextAlignmentOptions.Left;
        objectivesTitleText.text = "Objetivos:";
        
        // Criar painel de objetivos
        GameObject objectivesPanelObj = new GameObject("ObjectivesPanel");
        objectivesPanelObj.transform.SetParent(detailPanelObj.transform, false);
        RectTransform objectivesPanelRect = objectivesPanelObj.AddComponent<RectTransform>();
        objectivesPanelRect.anchorMin = new Vector2(0, 1);
        objectivesPanelRect.anchorMax = new Vector2(1, 1);
        objectivesPanelRect.pivot = new Vector2(0.5f, 1);
        objectivesPanelRect.anchoredPosition = new Vector2(0, -200);
        objectivesPanelRect.sizeDelta = new Vector2(-40, 100);
        VerticalLayoutGroup objectivesLayout = objectivesPanelObj.AddComponent<VerticalLayoutGroup>();
        objectivesLayout.padding = new RectOffset(5, 5, 5, 5);
        objectivesLayout.spacing = 5;
        objectivesLayout.childAlignment = TextAnchor.UpperLeft;
        
        // Criar título das recompensas
        GameObject rewardsTitleObj = new GameObject("RewardsTitle");
        rewardsTitleObj.transform.SetParent(detailPanelObj.transform, false);
        RectTransform rewardsTitleRect = rewardsTitleObj.AddComponent<RectTransform>();
        rewardsTitleRect.anchorMin = new Vector2(0, 1);
        rewardsTitleRect.anchorMax = new Vector2(1, 1);
        rewardsTitleRect.pivot = new Vector2(0.5f, 1);
        rewardsTitleRect.anchoredPosition = new Vector2(0, -320);
        rewardsTitleRect.sizeDelta = new Vector2(-40, 30);
        TextMeshProUGUI rewardsTitleText = rewardsTitleObj.AddComponent<TextMeshProUGUI>();
        rewardsTitleText.fontSize = 16;
        rewardsTitleText.alignment = TextAlignmentOptions.Left;
        rewardsTitleText.text = "Recompensas:";
        
        // Criar painel de recompensas
        GameObject rewardsPanelObj = new GameObject("RewardsPanel");
        rewardsPanelObj.transform.SetParent(detailPanelObj.transform, false);
        RectTransform rewardsPanelRect = rewardsPanelObj.AddComponent<RectTransform>();
        rewardsPanelRect.anchorMin = new Vector2(0, 1);
        rewardsPanelRect.anchorMax = new Vector2(1, 1);
        rewardsPanelRect.pivot = new Vector2(0.5f, 1);
        rewardsPanelRect.anchoredPosition = new Vector2(0, -350);
        rewardsPanelRect.sizeDelta = new Vector2(-40, 100);
        VerticalLayoutGroup rewardsLayout = rewardsPanelObj.AddComponent<VerticalLayoutGroup>();
        rewardsLayout.padding = new RectOffset(5, 5, 5, 5);
        rewardsLayout.spacing = 5;
        rewardsLayout.childAlignment = TextAnchor.UpperLeft;
        
        // Criar botões
        GameObject buttonsObj = new GameObject("Buttons");
        buttonsObj.transform.SetParent(detailPanelObj.transform, false);
        RectTransform buttonsRect = buttonsObj.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0, 0);
        buttonsRect.anchorMax = new Vector2(1, 0);
        buttonsRect.pivot = new Vector2(0.5f, 0);
        buttonsRect.anchoredPosition = new Vector2(0, 20);
        buttonsRect.sizeDelta = new Vector2(-40, 40);
        HorizontalLayoutGroup buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
        buttonsLayout.padding = new RectOffset(5, 5, 5, 5);
        buttonsLayout.spacing = 10;
        buttonsLayout.childAlignment = TextAnchor.MiddleRight;
        
        // Botão de abandonar
        GameObject abandonButtonObj = new GameObject("AbandonButton");
        abandonButtonObj.transform.SetParent(buttonsObj.transform, false);
        RectTransform abandonButtonRect = abandonButtonObj.AddComponent<RectTransform>();
        abandonButtonRect.sizeDelta = new Vector2(120, 30);
        Image abandonButtonImage = abandonButtonObj.AddComponent<Image>();
        abandonButtonImage.color = new Color(0.7f, 0.2f, 0.2f);
        Button abandonButton = abandonButtonObj.AddComponent<Button>();
        abandonButton.targetGraphic = abandonButtonImage;
        
        // Texto do botão de abandonar
        GameObject abandonTextObj = new GameObject("Text");
        abandonTextObj.transform.SetParent(abandonButtonObj.transform, false);
        RectTransform abandonTextRect = abandonTextObj.AddComponent<RectTransform>();
        abandonTextRect.anchorMin = Vector2.zero;
        abandonTextRect.anchorMax = Vector2.one;
        abandonTextRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI abandonText = abandonTextObj.AddComponent<TextMeshProUGUI>();
        abandonText.fontSize = 14;
        abandonText.alignment = TextAlignmentOptions.Center;
        abandonText.text = "Abandonar";
        
        // Botão de completar
        GameObject completeButtonObj = new GameObject("CompleteButton");
        completeButtonObj.transform.SetParent(buttonsObj.transform, false);
        RectTransform completeButtonRect = completeButtonObj.AddComponent<RectTransform>();
        completeButtonRect.sizeDelta = new Vector2(120, 30);
        Image completeButtonImage = completeButtonObj.AddComponent<Image>();
        completeButtonImage.color = new Color(0.2f, 0.7f, 0.2f);
        Button completeButton = completeButtonObj.AddComponent<Button>();
        completeButton.targetGraphic = completeButtonImage;
        
        // Texto do botão de completar
        GameObject completeTextObj = new GameObject("Text");
        completeTextObj.transform.SetParent(completeButtonObj.transform, false);
        RectTransform completeTextRect = completeTextObj.AddComponent<RectTransform>();
        completeTextRect.anchorMin = Vector2.zero;
        completeTextRect.anchorMax = Vector2.one;
        completeTextRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI completeText = completeTextObj.AddComponent<TextMeshProUGUI>();
        completeText.fontSize = 14;
        completeText.alignment = TextAlignmentOptions.Center;
        completeText.text = "Completar";
        
        // Botão de fechar
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(panelObj.transform, false);
        RectTransform closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(1, 1);
        closeButtonRect.anchorMax = new Vector2(1, 1);
        closeButtonRect.pivot = new Vector2(1, 1);
        closeButtonRect.anchoredPosition = new Vector2(-10, -10);
        closeButtonRect.sizeDelta = new Vector2(30, 30);
        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.color = new Color(0.7f, 0.2f, 0.2f);
        Button closeButton = closeButtonObj.AddComponent<Button>();
        closeButton.targetGraphic = closeButtonImage;
        
        // Texto do botão de fechar
        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.fontSize = 14;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.text = "X";
        
        // Configurar referências no QuestUI
        QuestUI questUI = questLogObj.GetComponent<QuestUI>();
        questUI.questLogPanel = questLogObj;
        questUI.questListContent = contentRect;
        questUI.questDetailPanel = detailPanelObj;
        questUI.questTitleText = questTitleText;
        questUI.questDescriptionText = questDescText;
        questUI.objectivesListContent = objectivesPanelRect;
        questUI.rewardsListContent = rewardsPanelRect;
        questUI.abandonQuestButton = abandonButton;
        questUI.completeQuestButton = completeButton;
        
        // Salvar prefab
        string prefabPath = "Assets/Resources/Quests/QuestLog.prefab";
        PrefabUtility.SaveAsPrefabAsset(questLogObj, prefabPath);
        GameObject.DestroyImmediate(questLogObj);
    }
}
#endif
*/