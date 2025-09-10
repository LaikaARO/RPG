/*using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Classe responsável por integrar o sistema de quests com outros sistemas do jogo
/// </summary>
public class QuestIntegration : MonoBehaviour
{
    private static QuestIntegration _instance;
    public static QuestIntegration Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("QuestIntegration");
                _instance = obj.AddComponent<QuestIntegration>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Inicializar
        InitializeIntegrations();
    }

    /// <summary>
    /// Inicializa as integrações com outros sistemas
    /// </summary>
    private void InitializeIntegrations()
    {
        // Integração com sistema de combate
        IntegrateWithCombatSystem();
        
        // Integração com sistema de inventário
        IntegrateWithInventorySystem();
        
        // Integração com sistema de diálogo
        IntegrateWithDialogueSystem();
        
        // Integração com sistema de movimento
        IntegrateWithMovementSystem();
    }

    /// <summary>
    /// Integra o sistema de quests com o sistema de combate
    /// </summary>
    private void IntegrateWithCombatSystem()
    {
        // Encontrar todos os EnemyControllers na cena
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            // Adicionar listener para quando o inimigo morrer
            enemy.OnDeath += HandleEnemyDeath;
        }
        
        // Adicionar listener para quando novos inimigos forem criados
        EnemyController.OnEnemyCreated += RegisterEnemyEvents;
    }
    
    /// <summary>
    /// Registra eventos para um novo inimigo
    /// </summary>
    private void RegisterEnemyEvents(EnemyController enemy)
    {
        enemy.OnDeath += HandleEnemyDeath;
    }
    
    /// <summary>
    /// Manipula o evento de morte de um inimigo
    /// </summary>
    private void HandleEnemyDeath(EnemyController enemy)
    {
        // Obter o ID do inimigo (assumindo que existe um campo enemyID)
        int enemyID = enemy.enemyID;
        
        // Notificar o QuestManager sobre a morte do inimigo
        QuestManager.Instance.OnEnemyKilled(enemyID);
    }

    /// <summary>
    /// Integra o sistema de quests com o sistema de inventário
    /// </summary>
    private void IntegrateWithInventorySystem()
    {
        // Encontrar o gerenciador de inventário
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            // Adicionar listener para quando um item for adicionado ao inventário
            inventory.OnItemAdded += HandleItemAdded;
            
            // Adicionar listener para quando um item for removido do inventário
            inventory.OnItemRemoved += HandleItemRemoved;
        }
    }
    
    /// <summary>
    /// Manipula o evento de adição de item ao inventário
    /// </summary>
    private void HandleItemAdded(Item item, int quantity)
    {
        // Obter o ID do item
        int itemID = item.itemID;
        
        // Notificar o QuestManager sobre a coleta do item
        QuestManager.Instance.OnItemCollected(itemID, quantity);
    }
    
    /// <summary>
    /// Manipula o evento de remoção de item do inventário
    /// </summary>
    private void HandleItemRemoved(Item item, int quantity)
    {
        // Obter o ID do item
        int itemID = item.itemID;
        
        // Notificar o QuestManager sobre a perda do item
        QuestManager.Instance.OnItemLost(itemID, quantity);
    }

    /// <summary>
    /// Integra o sistema de quests com o sistema de diálogo
    /// </summary>
    private void IntegrateWithDialogueSystem()
    {
        // Implementar quando o sistema de diálogo estiver disponível
        // Por exemplo:
        // DialogueManager.Instance.OnDialogueCompleted += HandleDialogueCompleted;
    }
    
    /// <summary>
    /// Manipula o evento de conclusão de diálogo
    /// </summary>
    private void HandleDialogueCompleted(int npcID)
    {
        // Notificar o QuestManager sobre a conversa com o NPC
        QuestManager.Instance.OnNPCTalked(npcID);
    }

    /// <summary>
    /// Integra o sistema de quests com o sistema de movimento
    /// </summary>
    private void IntegrateWithMovementSystem()
    {
        // Encontrar o controlador do jogador
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // Adicionar componente para rastrear a posição do jogador
            PlayerPositionTracker tracker = playerController.gameObject.AddComponent<PlayerPositionTracker>();
            tracker.Initialize();
        }
    }
}

/// <summary>
/// Componente para rastrear a posição do jogador para objetivos de exploração
/// </summary>
public class PlayerPositionTracker : MonoBehaviour
{
    // Lista de áreas de interesse para quests
    private List<QuestArea> questAreas = new List<QuestArea>();
    
    // Intervalo de verificação em segundos
    private float checkInterval = 1.0f;
    private float timer = 0f;
    
    /// <summary>
    /// Inicializa o rastreador
    /// </summary>
    public void Initialize()
    {
        // Encontrar todas as áreas de quest na cena
        QuestArea[] areas = FindObjectsOfType<QuestArea>();
        questAreas.AddRange(areas);
        
        // Adicionar listener para quando novas áreas forem criadas
        QuestArea.OnQuestAreaCreated += RegisterQuestArea;
    }
    
    /// <summary>
    /// Registra uma nova área de quest
    /// </summary>
    private void RegisterQuestArea(QuestArea area)
    {
        if (!questAreas.Contains(area))
        {
            questAreas.Add(area);
        }
    }
    
    private void Update()
    {
        // Verificar a cada intervalo definido
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckQuestAreas();
        }
    }
    
    /// <summary>
    /// Verifica se o jogador está em alguma área de quest
    /// </summary>
    private void CheckQuestAreas()
    {
        Vector3 playerPosition = transform.position;
        
        foreach (QuestArea area in questAreas)
        {
            if (area.IsPlayerInArea(playerPosition))
            {
                // Notificar o QuestManager sobre a exploração da área
                QuestManager.Instance.OnAreaExplored(area.areaID);
            }
        }
    }
}

/// <summary>
/// Componente que define uma área para objetivos de exploração
/// </summary>
public class QuestArea : MonoBehaviour
{
    // Evento disparado quando uma nova área é criada
    public static event System.Action<QuestArea> OnQuestAreaCreated;
    
    // ID da área para referência nas quests
    public int areaID;
    
    // Tipo de área (esfera, cubo, etc)
    public enum AreaType { Sphere, Box, Polygon }
    public AreaType areaType = AreaType.Sphere;
    
    // Tamanho da área
    public float radius = 5f;
    public Vector3 boxSize = new Vector3(10f, 5f, 10f);
    public Vector2[] polygonPoints;
    
    // Gizmos para visualização na cena
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        
        switch (areaType)
        {
            case AreaType.Sphere:
                Gizmos.DrawSphere(transform.position, radius);
                break;
                
            case AreaType.Box:
                Gizmos.DrawCube(transform.position, boxSize);
                break;
                
            case AreaType.Polygon:
                DrawPolygon();
                break;
        }
    }
    
    /// <summary>
    /// Desenha um polígono 2D no plano XZ
    /// </summary>
    private void DrawPolygon()
    {
        if (polygonPoints == null || polygonPoints.Length < 3)
            return;
            
        for (int i = 0; i < polygonPoints.Length; i++)
        {
            Vector3 current = transform.TransformPoint(new Vector3(polygonPoints[i].x, 0, polygonPoints[i].y));
            Vector3 next = transform.TransformPoint(new Vector3(
                polygonPoints[(i + 1) % polygonPoints.Length].x, 
                0, 
                polygonPoints[(i + 1) % polygonPoints.Length].y));
                
            Gizmos.DrawLine(current, next);
        }
    }
    
    /// <summary>
    /// Verifica se o jogador está dentro da área
    /// </summary>
    public bool IsPlayerInArea(Vector3 playerPosition)
    {
        switch (areaType)
        {
            case AreaType.Sphere:
                return Vector3.Distance(transform.position, playerPosition) <= radius;
                
            case AreaType.Box:
                Vector3 localPos = transform.InverseTransformPoint(playerPosition);
                return Mathf.Abs(localPos.x) <= boxSize.x / 2 && 
                       Mathf.Abs(localPos.y) <= boxSize.y / 2 && 
                       Mathf.Abs(localPos.z) <= boxSize.z / 2;
                
            case AreaType.Polygon:
                return IsPointInPolygon(playerPosition);
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Verifica se um ponto está dentro de um polígono 2D (no plano XZ)
    /// </summary>
    private bool IsPointInPolygon(Vector3 point)
    {
        if (polygonPoints == null || polygonPoints.Length < 3)
            return false;
            
        // Converter para espaço local
        Vector3 localPoint = transform.InverseTransformPoint(point);
        Vector2 point2D = new Vector2(localPoint.x, localPoint.z);
        
        bool inside = false;
        for (int i = 0, j = polygonPoints.Length - 1; i < polygonPoints.Length; j = i++)
        {
            if (((polygonPoints[i].y > point2D.y) != (polygonPoints[j].y > point2D.y)) &&
                (point2D.x < (polygonPoints[j].x - polygonPoints[i].x) * (point2D.y - polygonPoints[i].y) / 
                (polygonPoints[j].y - polygonPoints[i].y) + polygonPoints[i].x))
            {
                inside = !inside;
            }
        }
        
        return inside;
    }
    
    private void Awake()
    {
        // Notificar que uma nova área foi criada
        if (OnQuestAreaCreated != null)
        {
            OnQuestAreaCreated(this);
        }
    }
}*/