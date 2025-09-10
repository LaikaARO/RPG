using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Sistema de combate que gerencia ataques, dano e interações de combate entre personagens.
/// </summary>
public class CombatSystem : MonoBehaviour
{
    [Header("Configurações de Combate")]
    [Tooltip("Alcance do ataque")]
    [SerializeField] private float attackRange = 2f;
    
    [Tooltip("Tempo entre ataques em segundos")]
    [SerializeField] private float attackCooldown = 1.5f;
    
    [Tooltip("Camadas que podem ser atacadas")]
    [SerializeField] private LayerMask attackableLayers;

    // Referências a componentes
    private CharacterStats characterStats;
    private Animator animator;
    private PlayerMovement playerMovement;
    private LevelSystem levelSystem;

    // Variáveis de estado
    private bool canAttack = true;
    private Transform currentTarget;
    private bool isAttacking = false;
    
    // Cache para cálculos
    private Dictionary<string, float> damageCache = new Dictionary<string, float>();
    
    // Eventos
    public event Action<int> OnDamageDealt;
    public event Action<Transform> OnTargetChanged;
    public event Action OnAttackStarted;
    public event Action OnAttackFinished;
    public event Action<bool> OnCriticalHit;

    // Constantes para animação
    private const string ANIM_ATTACK_TRIGGER = "Attack";
    private const string ANIM_IS_ATTACKING = "IsAttacking";

    private void Awake()
    {
        InitializeComponents();
        InitializeDamageCache();
    }
    
    /// <summary>
    /// Inicializa e valida todos os componentes necessários
    /// </summary>
    private void InitializeComponents()
    {
        try
        {
            // Obter componentes necessários
            characterStats = GetComponent<CharacterStats>();
            animator = GetComponent<Animator>();
            playerMovement = GetComponent<PlayerMovement>();
            levelSystem = GetComponent<LevelSystem>();
            
            // Validar componentes críticos
            if (characterStats == null)
            {
                Debug.LogError("CombatSystem requer um componente CharacterStats no mesmo GameObject.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao inicializar componentes: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Inicializa o cache para cálculos de dano
    /// </summary>
    private void InitializeDamageCache()
    {
        // Pré-calcular valores comuns para melhorar performance
        damageCache["baseCritMultiplier"] = 1.5f;
    }

    private void Update()
    {
        // Verificar se o jogador clicou em um inimigo ou em outro lugar do mapa
        if (Input.GetMouseButtonDown(0))
        {
            CheckForTargetUnderMouse();
        }

        // Verificar se deve atacar o alvo atual
        if (currentTarget != null && canAttack && !isAttacking)
        {
            // Verificar se está dentro do alcance
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget <= attackRange)
            {
                StartAttack();
            }
        }
    }

    /// <summary>
    /// Verifica se o mouse está sobre um alvo atacável ou em outro lugar do mapa
    /// </summary>
    private void CheckForTargetUnderMouse()
    {
        try
        {
            // Verificar se o clique foi em um elemento da UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Se o clique foi na UI, não processamos o ataque
                return;
            }
            
            // Verificar se a câmera principal existe
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Camera.main não encontrada. Verifique se existe uma câmera com tag MainCamera.");
                return;
            }
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                // Verificar se o que foi atingido é um inimigo
                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    // Se estiver atacando, permitir mudar de alvo
                    if (isAttacking)
                    {
                        FinishAttack();
                    }
                    
                    // Definir o alvo atual
                    SetTarget(hit.transform);
                }
                else
                {
                    // Clicou em outro lugar que não é um inimigo, limpar o alvo atual
                    ClearTarget();
                }
            }
            else
            {
                // Não atingiu nada, limpar o alvo atual
                ClearTarget();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao verificar alvo sob o mouse: {ex.Message}");
        }
    }

    /// <summary>
    /// Define um alvo para atacar
    /// </summary>
    /// <param name="target">O transform do alvo</param>
    /// <returns>True se o alvo foi definido com sucesso</returns>
    public bool SetTarget(Transform target)
    {
        if (target == null) return false;
        
        try
        {    
            // Se o alvo mudou, notificar
            if (currentTarget != target)
            {
                currentTarget = target;
                OnTargetChanged?.Invoke(target);
            }
            
            // Notificar o sistema de movimento para se aproximar do alvo
            if (playerMovement != null && !isAttacking)
            {
                // Mover para uma posição próxima ao alvo
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - directionToTarget * (attackRange * 0.8f);
                playerMovement.MoveTo(targetPosition);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao definir alvo: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Limpa o alvo atual e para o ataque
    /// </summary>
    public void ClearTarget()
    {
        try
        {
            // Se não há alvo ou não está atacando, não precisa fazer nada
            if (currentTarget == null)
            {
                return;
            }
            
            // Se estiver atacando, finalizar o ataque
            if (isAttacking)
            {
                FinishAttack();
            }
            
            // Limpar o alvo atual
            Transform oldTarget = currentTarget;
            currentTarget = null;
            
            // Notificar mudança de alvo
            OnTargetChanged?.Invoke(null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao limpar alvo: {ex.Message}");
        }
    }

    /// <summary>
    /// Inicia o ataque ao alvo atual
    /// </summary>
    private void StartAttack()
    {
        if (!canAttack || currentTarget == null) return;

        // Iniciar o ataque
        isAttacking = true;
        canAttack = false;
        
        // Notificar que o ataque começou
        OnAttackStarted?.Invoke();
        
        // Parar o movimento do jogador durante o ataque
        if (playerMovement != null)
        {
            playerMovement.StopMoving();
        }

        // Rotacionar para olhar para o alvo
        RotateTowardsTarget();

        // Disparar animação de ataque
        PlayAttackAnimation();

        // Aplicar dano após um pequeno delay (sincronizado com a animação)
        StartCoroutine(ApplyDamageWithDelay(0.5f));

        // Iniciar cooldown de ataque
        StartCoroutine(AttackCooldown());
    }
    
    /// <summary>
    /// Rotaciona o personagem para olhar para o alvo atual
    /// </summary>
    private void RotateTowardsTarget()
    {
        if (currentTarget == null)
            return;
            
        Vector3 lookDirection = currentTarget.position - transform.position;
        lookDirection.y = 0; // Manter no plano horizontal
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
    
    /// <summary>
    /// Reproduz a animação de ataque
    /// </summary>
    private void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(ANIM_IS_ATTACKING, true);
            animator.SetTrigger(ANIM_ATTACK_TRIGGER);
        }
    }

    /// <summary>
    /// Aplica dano ao alvo após um delay
    /// </summary>
    private IEnumerator ApplyDamageWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        try
        {
            if (currentTarget == null) 
            {
                FinishAttack();
                yield break;
            }
            
            // Calcular dano com base nos atributos
            DamageResult damageResult = CalculateDamage();

            // Aplicar dano ao alvo
            CharacterStats targetStats = currentTarget.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                // Definir a flag para indicar que o dano vem do sistema de combate
                CharacterStats.SetDamageFromCombatSystem(true);
                
                // Aplicar o dano
                targetStats.ModifyHealth(-damageResult.damage);
                
                // Resetar a flag após aplicar o dano
                CharacterStats.SetDamageFromCombatSystem(false);
                
                // Notificar sobre o dano causado
                OnDamageDealt?.Invoke(damageResult.damage);
                
                // Notificar sobre crítico
                if (damageResult.isCritical)
                {
                    OnCriticalHit?.Invoke(true);
                }
                
                // Exibir o texto de dano flutuante
                if (DamageTextManager.Instance != null)
                {
                    DamageTextManager.Instance.ShowDamageText(currentTarget.position, damageResult.damage, damageResult.isCritical);
                }
                
                // Log para debug
                string criticalText = damageResult.isCritical ? " (CRÍTICO!)" : "";
                Debug.Log($"Causou {damageResult.damage} de dano ao {currentTarget.name}{criticalText}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao aplicar dano: {ex.Message}");
        }
        finally
        {
            // Finalizar o estado de ataque
            FinishAttack();
        }
    }
    
    /// <summary>
    /// Finaliza o estado de ataque
    /// </summary>
    private void FinishAttack()
    {
        isAttacking = false;
        
        // Atualizar animação
        if (animator != null)
        {
            animator.SetBool(ANIM_IS_ATTACKING, false);
        }
        
        // Notificar que o ataque terminou
        OnAttackFinished?.Invoke();
        
        // Permitir que o jogador se mova novamente após o ataque
        // Não movemos automaticamente para não interferir com o controle do jogador
    }

    /// <summary>
    /// Gerencia o cooldown entre ataques
    /// </summary>
    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    /// <summary>
    /// Estrutura para armazenar o resultado do cálculo de dano
    /// </summary>
    private struct DamageResult
    {
        public int damage;
        public bool isCritical;
    }
    
    /// <summary>
    /// Calcula o dano com base nos atributos do personagem
    /// </summary>
    private DamageResult CalculateDamage()
    {
        DamageResult result = new DamageResult
        {
            damage = 5, // Valor padrão
            isCritical = false
        };
        
        if (characterStats == null) return result;

        try
        {
            // Fórmula básica de dano: Força * 2 + nível
            float baseDamage = characterStats.Strength * 2;
            
            // Adicionar bônus de nível se disponível
            if (levelSystem != null)
            {
                baseDamage += levelSystem.Level;
            }

            // Verificar chance de crítico
            float critChance = CalculateCriticalChance();
            result.isCritical = UnityEngine.Random.Range(0f, 100f) <= critChance;

            // Aplicar dano crítico se necessário
            if (result.isCritical)
            {
                float critMultiplier = CalculateCriticalMultiplier();
                baseDamage *= critMultiplier;
            }

            // Adicionar variação aleatória pequena
            float randomVariation = UnityEngine.Random.Range(0.9f, 1.1f);
            baseDamage *= randomVariation;

            result.damage = Mathf.RoundToInt(baseDamage);
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao calcular dano: {ex.Message}");
            return result; // Valor padrão em caso de erro
        }
    }
    
    /// <summary>
    /// Calcula a chance de acerto crítico
    /// </summary>
    private float CalculateCriticalChance()
    {
        if (characterStats == null) return 5f;
        
        // Chance de crítico baseada em destreza e inteligência
        return characterStats.Dexterity * 0.1f + characterStats.Intelligence * 0.05f;
    }
    
    /// <summary>
    /// Calcula o multiplicador de dano crítico
    /// </summary>
    private float CalculateCriticalMultiplier()
    {
        if (characterStats == null) return damageCache["baseCritMultiplier"];
        
        // Multiplicador base + bônus de força
        return damageCache["baseCritMultiplier"] + (characterStats.Strength * 0.01f);
    }

    /// <summary>
    /// Verifica se o personagem está atacando
    /// </summary>
    public bool IsAttacking()
    {
        return isAttacking;
    }

    /// <summary>
    /// Cancela o ataque atual
    /// </summary>
    /// <returns>True se o ataque foi cancelado com sucesso</returns>
    public bool CancelAttack()
    {
        if (!isAttacking) return false;
        
        try
        {    
            isAttacking = false;
            Transform oldTarget = currentTarget;
            currentTarget = null;
            
            // Notificar mudança de alvo
            if (oldTarget != null)
            {
                OnTargetChanged?.Invoke(null);
            }
            
            // Atualizar animação
            if (animator != null)
            {
                animator.SetBool(ANIM_IS_ATTACKING, false);
            }
            
            // Notificar que o ataque terminou
            OnAttackFinished?.Invoke();
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao cancelar ataque: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Desenha o alcance de ataque no editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    /// <summary>
    /// Limpa os eventos ao destruir o objeto para evitar memory leaks
    /// </summary>
    private void OnDestroy()
    {
        OnDamageDealt = null;
        OnTargetChanged = null;
        OnAttackStarted = null;
        OnAttackFinished = null;
        OnCriticalHit = null;
    }
}