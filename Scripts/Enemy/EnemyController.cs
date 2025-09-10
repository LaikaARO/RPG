using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controla o comportamento básico de inimigos, incluindo detecção de jogador, movimento e ataque.
/// </summary>
public class EnemyController : MonoBehaviour
{
    // Evento estático para quando um novo inimigo é criado
    public static event Action<EnemyController> OnEnemyCreated;
    
    // ID do inimigo para o sistema de quests (1000-9999)
    [Header("Quest System")]
    [Tooltip("ID único do inimigo para o sistema de quests (1000-9999)")]
    public int enemyID = 1000;
    [Header("Configurações de Detecção")]
    [Tooltip("Raio de detecção do jogador")]
    [SerializeField] private float detectionRadius = 10f;
    
    [Tooltip("Raio de ataque")]
    [SerializeField] private float attackRange = 1.5f;
    
    [Tooltip("Intervalo entre ataques em segundos")]
    [SerializeField] private float attackCooldown = 2f;
    
    [Tooltip("Camada do jogador para detecção")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Configurações de Movimento")]
    [Tooltip("Velocidade de movimento")]
    [SerializeField] private float moveSpeed = 3.5f;
    
    [Tooltip("Velocidade de rotação")]
    [SerializeField] private float rotationSpeed = 5f;

    // Referências a componentes
    private NavMeshAgent navAgent;
    private Animator animator;
    private CharacterStats enemyStats;
    
    // Referência ao jogador
    private Transform playerTransform;
    private CharacterStats playerStats;
    
    // Eventos
    public event Action<int> OnDamageDealt;
    public event Action OnAttackStarted;
    public event Action OnAttackFinished;
    public event Action<EnemyController> OnDeath; // Evento disparado quando o inimigo morre
    
    // Variáveis de estado
    private bool isAttacking = false;
    private bool canAttack = true;
    private float distanceToPlayer;
    
    // Estados do inimigo
    private enum EnemyState { Idle, Chasing, Attacking }
    private EnemyState currentState = EnemyState.Idle;
    
    // Constantes para animação
    private const string ANIM_SPEED = "Speed";
    private const string ANIM_ATTACK = "Attack";
    private const string ANIM_IS_ATTACKING = "IsAttacking";

    private void Awake()
    {
        InitializeComponents();
    }
    
    /// <summary>
    /// Inicializa todos os componentes necessários
    /// </summary>
    private void InitializeComponents()
    {
        // Obter componentes necessários
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyStats = GetComponent<CharacterStats>();
        
        if (navAgent == null)
        {
            Debug.LogError($"{gameObject.name}: EnemyController requer um componente NavMeshAgent.");
            return;
        }
        
        if (enemyStats == null)
        {
            Debug.LogError($"{gameObject.name}: EnemyController requer um componente CharacterStats.");
            return;
        }
        
        // Configurar o NavMeshAgent
        navAgent.speed = moveSpeed;
        navAgent.stoppingDistance = attackRange * 0.8f;
    }

    private void Start()
    {
        // Verificar se os componentes foram inicializados corretamente
        if (navAgent == null || enemyStats == null)
        {
            Debug.LogError($"{gameObject.name}: Componentes necessários não foram inicializados. Desativando EnemyController.");
            enabled = false;
            return;
        }
        
        // Registrar para o evento de morte do CharacterStats
        if (enemyStats != null)
        {
            enemyStats.OnHealthReachedZero.AddListener(HandleDeath);
        }
        
        // Iniciar a rotina de busca por jogador
        StartCoroutine(ScanForPlayer());
        
        // Notificar que um novo inimigo foi criado
        if (OnEnemyCreated != null)
        {
            OnEnemyCreated(this);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;
        
        // Calcular distância até o jogador
        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Atualizar estado com base na distância
        UpdateState();
        
        // Executar comportamento baseado no estado atual
        ExecuteCurrentState();
        
        // Atualizar animações
        UpdateAnimations();
    }
    
    /// <summary>
    /// Executa o comportamento baseado no estado atual
    /// </summary>
    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
                
            case EnemyState.Chasing:
                HandleChasingState();
                break;
                
            case EnemyState.Attacking:
                HandleAttackingState();
                break;
        }
    }

    /// <summary>
    /// Rotina que verifica periodicamente a presença do jogador
    /// </summary>
    private IEnumerator ScanForPlayer()
    {
        WaitForSeconds scanInterval = new WaitForSeconds(0.5f);
        
        while (true)
        {
            // Procurar por jogador dentro do raio de detecção
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
            
            if (hitColliders.Length > 0)
            {
                // Jogador detectado
                GameObject player = hitColliders[0].gameObject;
                if (player != null)
                {
                    playerTransform = player.transform;
                    playerStats = player.GetComponent<CharacterStats>();
                    
                    // Mudar para estado de perseguição
                    if (currentState == EnemyState.Idle)
                    {
                        currentState = EnemyState.Chasing;
                    }
                }
            }
            else if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) > detectionRadius * 1.5f)
            {
                // Jogador saiu do alcance de detecção expandido
                playerTransform = null;
                playerStats = null;
                currentState = EnemyState.Idle;
            }
            
            yield return scanInterval;
        }
    }

    /// <summary>
    /// Atualiza o estado do inimigo com base na distância ao jogador
    /// </summary>
    private void UpdateState()
    {
        if (playerTransform == null)
        {
            currentState = EnemyState.Idle;
            return;
        }
        
        // Verificar se o jogador está dentro do alcance de ataque
        if (distanceToPlayer <= attackRange && canAttack)
        {
            currentState = EnemyState.Attacking;
        }
        // Verificar se o jogador está dentro do raio de detecção
        else if (distanceToPlayer <= detectionRadius)
        {
            currentState = EnemyState.Chasing;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    /// <summary>
    /// Comportamento no estado ocioso
    /// </summary>
    private void HandleIdleState()
    {
        if (navAgent != null && navAgent.hasPath)
        {
            navAgent.ResetPath();
        }
    }

    /// <summary>
    /// Comportamento no estado de perseguição
    /// </summary>
    private void HandleChasingState()
    {
        if (navAgent == null || playerTransform == null || isAttacking) return;
        
        if (navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(playerTransform.position);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: NavMeshAgent não está em uma NavMesh válida.");
        }
    }

    /// <summary>
    /// Comportamento no estado de ataque
    /// </summary>
    private void HandleAttackingState()
    {
        if (navAgent != null && !isAttacking)
        {
            if (navAgent.isOnNavMesh)
            {
                navAgent.ResetPath();
                navAgent.velocity = Vector3.zero;
            }
        }
        
        // Rotacionar para olhar para o jogador
        RotateTowardsPlayer();
        
        // Iniciar ataque se não estiver já atacando
        if (!isAttacking && canAttack)
        {
            StartCoroutine(PerformAttack());
        }
    }
    
    /// <summary>
    /// Rotaciona o inimigo para olhar para o jogador
    /// </summary>
    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;
        
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0; // Manter no plano horizontal
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    /// <summary>
    /// Executa o ataque ao jogador
    /// </summary>
    private IEnumerator PerformAttack()
    {
        // Iniciar ataque
        isAttacking = true;
        canAttack = false;
        
        // Notificar início do ataque
        OnAttackStarted?.Invoke();
        
        // Garantir que o NavMeshAgent pare durante o ataque
        StopNavMeshAgent();
        
        // Disparar animação de ataque
        PlayAttackAnimation();
        
        // Esperar pelo momento do impacto na animação
        WaitForSeconds impactDelay = new WaitForSeconds(0.5f);
        yield return impactDelay;
        
        // Aplicar dano ao jogador
        ApplyDamageToPlayer();
        
        // Finalizar animação de ataque
        StopAttackAnimation();
        
        // Esperar o cooldown de ataque
        WaitForSeconds cooldownDelay = new WaitForSeconds(attackCooldown);
        yield return cooldownDelay;
        
        // Finalizar ataque
        FinishAttack();
    }
    
    /// <summary>
    /// Para o NavMeshAgent durante o ataque
    /// </summary>
    private void StopNavMeshAgent()
    {
        if (navAgent == null) return;
        
        if (navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
        }
    }
    
    /// <summary>
    /// Inicia a animação de ataque
    /// </summary>
    private void PlayAttackAnimation()
    {
        if (animator == null) return;
        
        animator.SetBool(ANIM_IS_ATTACKING, true);
        animator.SetTrigger(ANIM_ATTACK);
    }
    
    /// <summary>
    /// Finaliza a animação de ataque
    /// </summary>
    private void StopAttackAnimation()
    {
        if (animator == null) return;
        
        animator.SetBool(ANIM_IS_ATTACKING, false);
    }
    
    /// <summary>
    /// Aplica dano ao jogador se estiver ao alcance
    /// </summary>
    private void ApplyDamageToPlayer()
    {
        if (playerTransform == null || playerStats == null) return;
        
        // Verificar se o jogador ainda está ao alcance
        if (distanceToPlayer <= attackRange * 1.2f)
        {
            // Calcular e aplicar dano
            int damage = CalculateDamage();
            playerStats.ModifyHealth(-damage);
            
            // Notificar sobre o dano causado
            OnDamageDealt?.Invoke(damage);
            
            if (Debug.isDebugBuild)
            {
                Debug.Log($"{gameObject.name} causou {damage} de dano ao jogador");
            }
        }
    }
    
    /// <summary>
    /// Finaliza o estado de ataque
    /// </summary>
    private void FinishAttack()
    {
        // Permitir novo ataque
        isAttacking = false;
        canAttack = true;
        
        // Reativar o NavMeshAgent
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
        }
        
        // Notificar fim do ataque
        OnAttackFinished?.Invoke();
    }

    /// <summary>
    /// Calcula o dano com base nos atributos do inimigo
    /// </summary>
    private int CalculateDamage()
    {
        const int DEFAULT_DAMAGE = 3;
        const float STRENGTH_MULTIPLIER = 1.5f;
        const float MIN_VARIATION = 0.9f;
        const float MAX_VARIATION = 1.1f;
        
        if (enemyStats == null) return DEFAULT_DAMAGE; // Valor padrão se não houver estatísticas

        // Fórmula básica de dano: Força * 1.5
        float baseDamage = enemyStats.Strength * STRENGTH_MULTIPLIER;
        
        // Adicionar variação aleatória pequena
        float randomVariation = UnityEngine.Random.Range(MIN_VARIATION, MAX_VARIATION);
        baseDamage *= randomVariation;

        return Mathf.RoundToInt(baseDamage);
    }

    /// <summary>
    /// Atualiza as animações com base no estado atual
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Atualizar velocidade para animação
        float currentSpeed = 0f;
        
        if (navAgent != null && navAgent.hasPath)
        {
            currentSpeed = navAgent.velocity.magnitude / moveSpeed;
        }
        
        animator.SetFloat(ANIM_SPEED, currentSpeed);
    }
    
    /// <summary>
    /// Manipula a morte do inimigo
    /// </summary>
    private void HandleDeath()
    {
        // Desativar componentes de movimento e colisão
        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }
        
        // Desativar o controlador
        enabled = false;
        
        // Disparar evento de morte
        if (OnDeath != null)
        {
            OnDeath(this);
        }
        
        // Iniciar animação de morte se disponível
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // Destruir o objeto após um tempo (ou iniciar sistema de loot)
        StartCoroutine(DestroyAfterDelay(3f));
    }
    
    /// <summary>
    /// Destrói o objeto após um atraso
    /// </summary>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    /// <summary>
    /// Desenha gizmos para visualização no editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Desenhar raio de detecção
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Desenhar raio de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    private void OnDestroy()
    {
        // Limpar coroutines e listeners de eventos
        StopAllCoroutines();
        
        // Remover listener do evento de morte
        if (enemyStats != null)
        {
            enemyStats.OnHealthReachedZero.RemoveListener(HandleDeath);
        }
    }
}