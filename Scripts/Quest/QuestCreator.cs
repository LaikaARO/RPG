/*using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
/// <summary>
/// Editor script para criar quests de exemplo
/// </summary>
public class QuestCreator : MonoBehaviour
{
    [MenuItem("RPG/Create Example Quests")]
    public static void CreateExampleQuests()
    {
        // Criar diretório se não existir
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Quests"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Quests");
        }
        
        // Criar quest de matar inimigos
        CreateKillQuest();
        
        // Criar quest de coletar itens
        CreateCollectQuest();
        
        // Criar quest de exploração
        CreateExploreQuest();
        
        // Criar quest de diálogo
        CreateTalkQuest();
        
        // Criar quest com múltiplos objetivos
        CreateMultiObjectiveQuest();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Quests de exemplo criadas com sucesso!");
    }
    
    /// <summary>
    /// Cria uma quest de matar inimigos
    /// </summary>
    private static void CreateKillQuest()
    {
        Quest quest = ScriptableObject.CreateInstance<Quest>();
        quest.questID = 1;
        quest.questName = "Caçada aos Goblins";
        quest.description = "Os goblins estão atacando os viajantes na estrada para o norte. Elimine alguns deles para garantir a segurança dos comerciantes.";
        quest.questType = QuestType.Kill;
        quest.level = 1;
        quest.experienceReward = 100;
        quest.goldReward = 50;
        
        // Criar objetivo de matar
        KillObjective killObjective = new KillObjective
        {
            description = "Matar Goblins",
            enemyID = 1001, // ID do Goblin
            requiredAmount = 5,
            currentAmount = 0
        };
        
        quest.objectives = new List<QuestObjective> { killObjective };
        
        // Adicionar recompensa de item
        QuestItemReward itemReward = new QuestItemReward
        {
            itemID = 10001, // ID de uma espada
            amount = 1
        };
        
        quest.itemRewards = new List<QuestItemReward> { itemReward };
        
        // Salvar quest
        AssetDatabase.CreateAsset(quest, "Assets/Resources/Quests/KillQuest.asset");
    }
    
    /// <summary>
    /// Cria uma quest de coletar itens
    /// </summary>
    private static void CreateCollectQuest()
    {
        Quest quest = ScriptableObject.CreateInstance<Quest>();
        quest.questID = 2;
        quest.questName = "Ervas Medicinais";
        quest.description = "O curandeiro da vila precisa de ervas medicinais para preparar poções. Colete algumas ervas na floresta.";
        quest.questType = QuestType.Collect;
        quest.level = 2;
        quest.experienceReward = 150;
        quest.goldReward = 75;
        
        // Criar objetivo de coletar
        CollectObjective collectObjective = new CollectObjective
        {
            description = "Coletar Ervas Medicinais",
            itemID = 10002, // ID das ervas
            requiredAmount = 10,
            currentAmount = 0
        };
        
        quest.objectives = new List<QuestObjective> { collectObjective };
        
        // Adicionar recompensa de item
        QuestItemReward itemReward = new QuestItemReward
        {
            itemID = 10003, // ID de uma poção
            amount = 3
        };
        
        quest.itemRewards = new List<QuestItemReward> { itemReward };
        
        // Salvar quest
        AssetDatabase.CreateAsset(quest, "Assets/Resources/Quests/CollectQuest.asset");
    }
    
    /// <summary>
    /// Cria uma quest de exploração
    /// </summary>
    private static void CreateExploreQuest()
    {
        Quest quest = ScriptableObject.CreateInstance<Quest>();
        quest.questID = 3;
        quest.questName = "Explorador das Ruínas";
        quest.description = "Antigas ruínas foram descobertas ao leste. Explore-as e descubra seus segredos.";
        quest.questType = QuestType.Explore;
        quest.level = 3;
        quest.experienceReward = 200;
        quest.goldReward = 100;
        
        // Criar objetivo de explorar
        ExploreObjective exploreObjective = new ExploreObjective
        {
            description = "Explorar as Ruínas Antigas",
            areaID = 1, // ID da área das ruínas
            areaName = "Ruínas Antigas",
            isExplored = false
        };
        
        quest.objectives = new List<QuestObjective> { exploreObjective };
        
        // Adicionar recompensa de item
        QuestItemReward itemReward = new QuestItemReward
        {
            itemID = 10004, // ID de um artefato antigo
            amount = 1
        };
        
        quest.itemRewards = new List<QuestItemReward> { itemReward };
        
        // Salvar quest
        AssetDatabase.CreateAsset(quest, "Assets/Resources/Quests/ExploreQuest.asset");
    }
    
    /// <summary>
    /// Cria uma quest de diálogo
    /// </summary>
    private static void CreateTalkQuest()
    {
        Quest quest = ScriptableObject.CreateInstance<Quest>();
        quest.questID = 4;
        quest.questName = "Mensageiro Real";
        quest.description = "O rei precisa enviar uma mensagem importante ao líder da vila vizinha. Entregue a mensagem e traga a resposta.";
        quest.questType = QuestType.Talk;
        quest.level = 2;
        quest.experienceReward = 120;
        quest.goldReward = 60;
        
        // Criar objetivo de falar
        TalkObjective talkObjective = new TalkObjective
        {
            description = "Falar com o Líder da Vila",
            npcID = 101, // ID do líder da vila
            npcName = "Aldric",
            isCompleted = false
        };
        
        quest.objectives = new List<QuestObjective> { talkObjective };
        
        // Adicionar recompensa de item
        QuestItemReward itemReward = new QuestItemReward
        {
            itemID = 10005, // ID de um anel
            amount = 1
        };
        
        quest.itemRewards = new List<QuestItemReward> { itemReward };
        
        // Salvar quest
        AssetDatabase.CreateAsset(quest, "Assets/Resources/Quests/TalkQuest.asset");
    }
    
    /// <summary>
    /// Cria uma quest com múltiplos objetivos
    /// </summary>
    private static void CreateMultiObjectiveQuest()
    {
        Quest quest = ScriptableObject.CreateInstance<Quest>();
        quest.questID = 5;
        quest.questName = "A Ameaça dos Bandidos";
        quest.description = "Um grupo de bandidos está aterrorizando a região. Elimine seu líder, recupere os itens roubados e informe o capitão da guarda sobre o sucesso da missão.";
        quest.questType = QuestType.Mixed;
        quest.level = 5;
        quest.experienceReward = 500;
        quest.goldReward = 250;
        
        // Criar objetivos
        List<QuestObjective> objectives = new List<QuestObjective>();
        
        // Objetivo 1: Matar o líder dos bandidos
        KillObjective killObjective = new KillObjective
        {
            description = "Eliminar o Líder dos Bandidos",
            enemyID = 1005, // ID do líder dos bandidos
            requiredAmount = 1,
            currentAmount = 0
        };
        objectives.Add(killObjective);
        
        // Objetivo 2: Recuperar itens roubados
        CollectObjective collectObjective = new CollectObjective
        {
            description = "Recuperar Itens Roubados",
            itemID = 10010, // ID dos itens roubados
            requiredAmount = 1,
            currentAmount = 0
        };
        objectives.Add(collectObjective);
        
        // Objetivo 3: Falar com o capitão da guarda
        TalkObjective talkObjective = new TalkObjective
        {
            description = "Informar o Capitão da Guarda",
            npcID = 102, // ID do capitão da guarda
            npcName = "Capitão Thorne",
            isCompleted = false
        };
        objectives.Add(talkObjective);
        
        quest.objectives = objectives;
        
        // Adicionar recompensas de item
        List<QuestItemReward> itemRewards = new List<QuestItemReward>();
        
        QuestItemReward itemReward1 = new QuestItemReward
        {
            itemID = 10006, // ID de uma armadura
            amount = 1
        };
        itemRewards.Add(itemReward1);
        
        QuestItemReward itemReward2 = new QuestItemReward
        {
            itemID = 10007, // ID de uma poção rara
            amount = 2
        };
        itemRewards.Add(itemReward2);
        
        quest.itemRewards = itemRewards;
        
        // Salvar quest
        AssetDatabase.CreateAsset(quest, "Assets/Resources/Quests/MultiObjectiveQuest.asset");
    }
}
#endif
*/