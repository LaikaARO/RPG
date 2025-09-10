using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema que gerencia a morte de personagens e distribui recompensas.
/// Versão atualizada com sistema de drop de itens.
/// </summary>
public class DeathSystem : MonoBehaviour
{
    [Header("Configurações de Morte")]
    [Tooltip("Tempo em segundos antes do corpo desaparecer")]
    [SerializeField] private float corpseDisappearTime = 3f;
    
    [Tooltip("Efeito visual para spawnar na morte")]
    [SerializeField] private GameObject deathEffectPrefab;

    [Header("Configurações de Recompensa")]
    [Tooltip("Quantidade de XP concedida ao morrer (apenas para inimigos)")]
    [SerializeField] private int experienceReward = 10;

    [Header("Sistema de Drop de Itens")]
    [Tooltip("Prefab do item coletável")]
    [SerializeField] private GameObject lootItemPrefab;
    
    [Tooltip("Lista de itens que podem ser dropados com suas chances")]
    [SerializeField] private LootDrop[] possibleLoots;

    // Referências a componentes
    private CharacterStats characterStats;
    
    // Eventos
    public UnityEvent OnDeath;
    public UnityEvent<int> OnExperienceRewarded;
    
    // Evento para notificar outros sistemas sobre a morte
    public event Action<GameObject> OnEntityDied;

    private void Awake()
    {
        InitializeComponents();
    }
    
    /// <summary>
    /// Inicializa os componentes e inscreve nos eventos necessários
    /// </summary>
    private void InitializeComponents()
    {
        // Obter componentes necessários
        characterStats = GetComponent<CharacterStats>();
        
        if (characterStats == null)
        {
            Debug.LogError("DeathSystem requer um componente CharacterStats no mesmo GameObject.");
            return;
        }
        
        // Garantir que o evento OnDeath esteja inicializado
        if (OnDeath == null)
        {
            OnDeath = new UnityEvent();
        }
        
        // Garantir que o evento OnExperienceRewarded esteja inicializado
        if (OnExperienceRewarded == null)
        {
            OnExperienceRewarded = new UnityEvent<int>();
        }
        
        // Inscrever-se no evento de saúde zero
        characterStats.OnHealthReachedZero.AddListener(HandleDeath);
    }

    private void OnDestroy()
    {
        // Cancelar inscrição para evitar vazamentos de memória
        if (characterStats != null)
        {
            characterStats.OnHealthReachedZero.RemoveListener(HandleDeath);
        }
        
        // Limpar eventos para evitar memory leaks
        if (OnDeath != null)
        {
            OnDeath.RemoveAllListeners();
        }
        
        if (OnExperienceRewarded != null)
        {
            OnExperienceRewarded.RemoveAllListeners();
        }
        
        // Limpar eventos de delegate
        OnEntityDied = null;
    }

    /// <summary>
    /// Gerencia o processo de morte do personagem
    /// </summary>
    private void HandleDeath()
    {
        // Verificar se o objeto já foi destruído
        if (this == null || !gameObject.activeInHierarchy) return;
        
        // Verificar se é o jogador ou um inimigo
        bool isPlayer = CompareTag("Player");
        
        try
        {
            // Desativar componentes de movimento e combate
            DisableComponents();
            
            // Disparar animação de morte se houver um Animator
            PlayDeathAnimation();
            
            // Spawnar efeito visual de morte
            SpawnDeathEffect();
            
            // Invocar evento de morte
            OnDeath?.Invoke();
            
            // Notificar outros sistemas sobre a morte
            OnEntityDied?.Invoke(gameObject);
            
            // Processar recompensas se for um inimigo
            if (!isPlayer)
            {
                // Conceder XP ao jogador
                RewardExperience();
                
                // Dropar itens
                DropLoot();
                
                // Iniciar processo de desaparecimento do corpo
                StartCoroutine(DisappearCorpse());
            }
            else
            {
                // Lógica para morte do jogador
                Debug.Log("O jogador morreu!");
                // Aqui poderia ser implementado um game over ou respawn
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao processar morte: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Reproduz a animação de morte
    /// </summary>
    private void PlayDeathAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }
    
    /// <summary>
    /// Spawna o efeito visual de morte
    /// </summary>
    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }

    /// <summary>
    /// Desativa componentes relacionados a movimento e combate
    /// </summary>
    private void DisableComponents()
    {
        try
        {
            // Desativar NavMeshAgent se for um inimigo
            DisableNavMeshAgent();
            
            // Desativar controlador de inimigo
            DisableEnemyController();
            
            // Desativar sistema de combate
            DisableCombatSystem();
            
            // Desativar movimento do jogador
            DisablePlayerMovement();
            
            // Desativar colliders para evitar mais interações
            DisableColliders();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao desativar componentes: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Desativa o NavMeshAgent
    /// </summary>
    private void DisableNavMeshAgent()
    {
        var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
    }
    
    /// <summary>
    /// Desativa o controlador de inimigo
    /// </summary>
    private void DisableEnemyController()
    {
        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }
    }
    
    /// <summary>
    /// Desativa o sistema de combate
    /// </summary>
    private void DisableCombatSystem()
    {
        var combatSystem = GetComponent<CombatSystem>();
        if (combatSystem != null)
        {
            combatSystem.enabled = false;
        }
    }
    
    /// <summary>
    /// Desativa o movimento do jogador
    /// </summary>
    private void DisablePlayerMovement()
    {
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }
    
    /// <summary>
    /// Desativa os colliders
    /// </summary>
    private void DisableColliders()
    {
        var colliders = GetComponents<Collider>();
        if (colliders != null && colliders.Length > 0)
        {
            foreach (var col in colliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Concede experiência ao jogador quando um inimigo morre
    /// </summary>
    private void RewardExperience()
    {
        try
        {
            // Encontrar o jogador - cache para melhor performance
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            // Calcular XP com base no nível do inimigo, se disponível
            int xpAmount = CalculateExperienceReward();
            
            // Conceder XP ao sistema de nível do jogador
            LevelSystem playerLevelSystem = player.GetComponent<LevelSystem>();
            if (playerLevelSystem != null)
            {
                playerLevelSystem.AddExperience(xpAmount);
                
                // Invocar evento para UI ou outros sistemas
                OnExperienceRewarded?.Invoke(xpAmount);
                
                Debug.Log($"Jogador ganhou {xpAmount} de experiência!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao recompensar experiência: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Calcula a quantidade de experiência a ser concedida
    /// </summary>
    private int CalculateExperienceReward()
    {
        int xpAmount = experienceReward;
        
        LevelSystem enemyLevelSystem = GetComponent<LevelSystem>();
        if (enemyLevelSystem != null)
        {
            // Bônus de XP baseado no nível do inimigo
            xpAmount += enemyLevelSystem.Level * 5;
        }
        
        return xpAmount;
    }

    /// <summary>
    /// Dropa itens baseado na tabela de loot
    /// </summary>
    private void DropLoot()
    {
        if (possibleLoots == null || possibleLoots.Length == 0 || lootItemPrefab == null)
        {
            return;
        }

        try
        {
            foreach (var lootDrop in possibleLoots)
            {
                if (lootDrop.item == null) continue;
    
                // Verificar chance de drop
                if (ShouldDropItem(lootDrop.dropChance))
                {
                    // Calcular quantidade a dropar
                    int quantity = CalculateDropQuantity(lootDrop.minQuantity, lootDrop.maxQuantity);
                    
                    // Spawnar o item
                    SpawnLootItem(lootDrop.item, quantity);
                    
                    Debug.Log($"Inimigo dropou {quantity}x {lootDrop.item.itemName}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao dropar itens: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Verifica se um item deve ser dropado com base na chance
    /// </summary>
    private bool ShouldDropItem(float dropChance)
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        return randomValue <= dropChance;
    }
    
    /// <summary>
    /// Calcula a quantidade de itens a serem dropados
    /// </summary>
    private int CalculateDropQuantity(int minQuantity, int maxQuantity)
    {
        return UnityEngine.Random.Range(minQuantity, maxQuantity + 1);
    }

    /// <summary>
    /// Spawna um item coletável no mundo
    /// </summary>
    /// <param name="item">Item a ser spawnado</param>
    /// <param name="quantity">Quantidade do item</param>
    private void SpawnLootItem(Item item, int quantity)
    {
        if (lootItemPrefab == null || item == null) return;

        try
        {
            // Calcular posição de spawn (um pouco acima do chão e com offset aleatório)
            Vector3 spawnPosition = CalculateLootSpawnPosition();
    
            // Instanciar o prefab do item
            GameObject lootObject = Instantiate(lootItemPrefab, spawnPosition, Quaternion.identity);
            if (lootObject == null) return;
            
            // Configurar o componente LootItem
            ConfigureLootItem(lootObject, item, quantity);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao spawnar item: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Calcula a posição de spawn para o item de loot
    /// </summary>
    private Vector3 CalculateLootSpawnPosition()
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
        spawnPosition += new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            0,
            UnityEngine.Random.Range(-1f, 1f)
        );
        
        return spawnPosition;
    }
    
    /// <summary>
    /// Configura o componente LootItem no objeto instanciado
    /// </summary>
    private void ConfigureLootItem(GameObject lootObject, Item item, int quantity)
    {
        // Configurar o componente LootItem (usando GetComponent genérico para evitar erro de tipo)
        var lootItemComponent = lootObject.GetComponent<MonoBehaviour>();
        if (lootItemComponent != null && lootItemComponent.GetType().Name == "LootItem")
        {
            // Usar reflection para chamar SetItem se o componente existir
            var setItemMethod = lootItemComponent.GetType().GetMethod("SetItem");
            if (setItemMethod != null)
            {
                setItemMethod.Invoke(lootItemComponent, new object[] { item, quantity });
            }
        }
        else
        {
            Debug.LogError("Prefab de loot não possui componente LootItem!");
        }
    }

    /// <summary>
    /// Faz o corpo desaparecer após um tempo
    /// </summary>
    private IEnumerator DisappearCorpse()
    {
        if (corpseDisappearTime <= 0)
        {
            corpseDisappearTime = 0.1f; // Valor mínimo para evitar problemas
        }
        
        yield return new WaitForSeconds(corpseDisappearTime);
        
        // Verificar se o objeto ainda existe
        if (this == null || !gameObject) yield break;
        
        try
        {
            // Efeito de desaparecimento
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Destruir o objeto
            Destroy(gameObject);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao fazer o corpo desaparecer: {ex.Message}");
        }
    }

    /// <summary>
    /// Adiciona um item à tabela de loot
    /// </summary>
    /// <param name="item">Item a ser adicionado</param>
    /// <param name="dropChance">Chance de drop (0-100)</param>
    /// <param name="minQuantity">Quantidade mínima</param>
    /// <param name="maxQuantity">Quantidade máxima</param>
    public void AddLootItem(Item item, float dropChance, int minQuantity = 1, int maxQuantity = 1)
    {
        if (item == null) return;

        // Expandir o array se necessário
        LootDrop[] newLoots = new LootDrop[possibleLoots.Length + 1];
        for (int i = 0; i < possibleLoots.Length; i++)
        {
            newLoots[i] = possibleLoots[i];
        }
        
        // Adicionar novo item
        newLoots[possibleLoots.Length] = new LootDrop
        {
            item = item,
            dropChance = dropChance,
            minQuantity = minQuantity,
            maxQuantity = maxQuantity
        };
        
        possibleLoots = newLoots;
    }
}

/// <summary>
/// Estrutura para definir um item de loot com suas propriedades
/// </summary>
[System.Serializable]
public class LootDrop
{
    [Tooltip("Item que pode ser dropado")]
    public Item item;
    
    [Tooltip("Chance de drop em porcentagem (0-100)")]
    [Range(0f, 100f)]
    public float dropChance = 50f;
    
    [Tooltip("Quantidade mínima que pode ser dropada")]
    public int minQuantity = 1;
    
    [Tooltip("Quantidade máxima que pode ser dropada")]
    public int maxQuantity = 1;
}