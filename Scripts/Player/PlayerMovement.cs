using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Controla o movimento do personagem usando o sistema de navegação NavMesh da Unity.
/// Permite movimento point & click com o botão esquerdo do mouse.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [Tooltip("Velocidade de movimento do personagem")]
    [SerializeField] private float moveSpeed = 3.5f;
    
    [Tooltip("Distância mínima para considerar que o personagem chegou ao destino")]
    [SerializeField] private float stoppingDistance = 0.2f;
    
    [Tooltip("Velocidade de rotação do personagem")]
    [SerializeField] private float rotationSpeed = 10f;

    // Componentes
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    
    // Variáveis de estado
    private bool isMoving = false;
    private Vector3 targetPosition;
    
    // Eventos
    public event Action OnMovementStarted;
    public event Action OnMovementStopped;

    // Constantes para animação
    private const string ANIM_IS_MOVING = "IsMoving";

    private void Awake()
    {
        InitializeComponents();
    }
    
    /// <summary>
    /// Inicializa e valida todos os componentes necessários
    /// </summary>
    private void InitializeComponents()
    {
        // Obter componentes necessários
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Configurar o NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
            navMeshAgent.updateRotation = false; // Vamos controlar a rotação manualmente
        }
        else
        {
            Debug.LogError("NavMeshAgent não encontrado no objeto do jogador. Adicione um componente NavMeshAgent.");
        }
    }

    private void Update()
    {
        // Verificar clique do mouse para movimento
        if (Input.GetMouseButtonDown(0))
        {
            MoveToMousePosition();
        }

        // Atualizar estado de movimento
        UpdateMovementState();
        
        // Rotacionar o personagem na direção do movimento
        RotateTowardsMovementDirection();
    }

    /// <summary>
    /// Move o personagem para a posição do mouse quando clicado
    /// </summary>
    private void MoveToMousePosition()
    {
        // Verificar se o clique foi em um elemento da UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Se o clique foi na UI, não movemos o personagem
            return;
        }
        
        // Verificar se a câmera principal existe
        if (Camera.main == null)
        {
            Debug.LogWarning("Camera.main não encontrada. Verifique se existe uma câmera com tag MainCamera.");
            return;
        }
        
        // Criar um raio a partir da posição do mouse na tela
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Verificar se o raio atingiu algo
        if (Physics.Raycast(ray, out hit))
        {
            // Verificar se o que foi atingido é um inimigo
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                // Lógica para atacar inimigo será implementada no sistema de combate
                return;
            }
            
            // Verificar se o NavMeshAgent está ativo e funcionando
            if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh)
            {
                Debug.LogWarning("NavMeshAgent não está disponível ou não está em um NavMesh válido.");
                return;
            }
            
            // Definir o destino do NavMeshAgent para a posição do clique
            if (navMeshAgent.SetDestination(hit.point))
            {
                targetPosition = hit.point;
                SetMovingState(true);
            }
        }
    }

    /// <summary>
    /// Atualiza o estado de movimento e as animações
    /// </summary>
    private void UpdateMovementState()
    {
        // Verificar se o NavMeshAgent está disponível
        if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled)
            return;
            
        // Verificar se o personagem chegou ao destino
        if (isMoving && navMeshAgent.pathStatus != NavMeshPathStatus.PathInvalid && 
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            SetMovingState(false);
        }

        // Atualizar animação se o Animator existir
        UpdateAnimationState();
    }

    /// <summary>
    /// Rotaciona o personagem na direção do movimento
    /// </summary>
    private void RotateTowardsMovementDirection()
    {
        if (navMeshAgent.velocity.magnitude > 0.1f)
        {
            // Obter a direção do movimento
            Vector3 direction = navMeshAgent.velocity.normalized;
            
            // Calcular a rotação desejada
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Aplicar a rotação suavemente
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Atualiza o estado das animações baseado no movimento
    /// </summary>
    private void UpdateAnimationState()
    {
        if (animator != null)
        {
            animator.SetBool(ANIM_IS_MOVING, isMoving);
        }
    }
    
    /// <summary>
    /// Define o estado de movimento e dispara eventos apropriados
    /// </summary>
    private void SetMovingState(bool moving)
    {
        // Se o estado não mudou, não fazemos nada
        if (isMoving == moving)
            return;
            
        isMoving = moving;
        
        // Disparar eventos apropriados
        if (isMoving)
        {
            OnMovementStarted?.Invoke();
        }
        else
        {
            OnMovementStopped?.Invoke();
        }
    }
    
    /// <summary>
    /// Método público para mover o personagem para uma posição específica
    /// </summary>
    public bool MoveTo(Vector3 position)
    {
        // Verificar se o NavMeshAgent está disponível
        if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("Tentativa de mover o personagem com NavMeshAgent inválido.");
            return false;
        }
        
        // Tentar definir o destino
        if (navMeshAgent.SetDestination(position))
        {
            targetPosition = position;
            SetMovingState(true);
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Método público para parar o movimento do personagem
    /// </summary>
    public void StopMoving()
    {
        // Verificar se o NavMeshAgent está disponível
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.ResetPath();
        }
        
        SetMovingState(false);
        UpdateAnimationState();
    }

    /// <summary>
    /// Retorna se o personagem está se movendo
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }
}