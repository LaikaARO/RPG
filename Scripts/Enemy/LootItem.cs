using UnityEngine;
using System.Collections;

/// <summary>
/// Componente que representa um item coletável no mundo do jogo
/// </summary>
public class LootItem : MonoBehaviour
{
    [Header("Configurações do Item")]
    [Tooltip("Referência ao ScriptableObject do item")]
    [SerializeField] private Item item;
    
    [Tooltip("Quantidade do item")]
    [SerializeField] private int quantity = 1;
    
    [Header("Configurações Visuais")]
    [Tooltip("Velocidade de rotação do item")]
    [SerializeField] private float rotationSpeed = 45f;
    
    [Tooltip("Altura da flutuação")]
    [SerializeField] private float floatHeight = 0.3f;
    
    [Tooltip("Velocidade da flutuação")]
    [SerializeField] private float floatSpeed = 2f;
    
    [Header("Configurações de Coleta")]
    [Tooltip("Distância para coleta automática")]
    [SerializeField] private float collectRange = 2f;
    
    [Tooltip("Tempo antes do item desaparecer")]
    [SerializeField] private float disappearTime = 60f;
    
    // Variáveis privadas
    private Vector3 initialPosition;
    private Transform playerTransform;
    private bool canBeCollected = true;
    private float currentDisappearTime;
    
    // Componentes
    private Renderer itemRenderer;
    private Collider itemCollider;

    private void Start()
    {
        // Salvar posição inicial para flutuação
        initialPosition = transform.position;
        
        // Encontrar o jogador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Obter componentes
        itemRenderer = GetComponent<Renderer>();
        itemCollider = GetComponent<Collider>();
        
        // Configurar collider como trigger
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }
        
        // Aplicar cor da raridade se houver renderer
        ApplyRarityColor();
        
        // Inicializar timer de desaparecimento
        currentDisappearTime = disappearTime;
        
        // Iniciar rotina de desaparecimento
        StartCoroutine(DisappearCountdown());
    }

    private void Update()
    {
        // Rotacionar o item
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // Fazer o item flutuar
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Verificar coleta automática se o jogador estiver próximo
        if (playerTransform != null && canBeCollected)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= collectRange)
            {
                CollectItem();
            }
        }
    }

    /// <summary>
    /// Aplica a cor da raridade ao material do item
    /// </summary>
    private void ApplyRarityColor()
    {
        if (itemRenderer != null && item != null)
        {
            Material material = itemRenderer.material;
            
            // Aplicar cor de emissão baseada na raridade
            Color rarityColor = item.GetRarityColor();
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", rarityColor * 0.3f);
        }
    }

    /// <summary>
    /// Coleta o item e adiciona ao inventário do jogador
    /// </summary>
    private void CollectItem()
    {
        if (!canBeCollected || item == null) return;
        
        // Tentar adicionar ao inventário
        if (InventorySystem.Instance != null)
        {
            bool success = InventorySystem.Instance.AddItem(item, quantity);
            
            if (success)
            {
                // Item coletado com sucesso
                canBeCollected = false;
                
                // Efeito visual de coleta
                StartCoroutine(CollectionEffect());
                
                Debug.Log($"Coletou {quantity}x {item.itemName}");
            }
            else
            {
                Debug.Log("Inventário cheio! Não foi possível coletar o item.");
            }
        }
        else
        {
            Debug.LogError("InventorySystem não encontrado!");
        }
    }

    /// <summary>
    /// Efeito visual quando o item é coletado
    /// </summary>
    private IEnumerator CollectionEffect()
    {
        // Desabilitar colisões
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }
        
        // Animação de coleta - mover para cima e fazer fade
        float effectDuration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * 1.5f;
        
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / effectDuration;
            
            // Mover para cima
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            // Fade out
            if (itemRenderer != null)
            {
                Material material = itemRenderer.material;
                Color color = material.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                material.color = color;
            }
            
            yield return null;
        }
        
        // Destruir o objeto
        Destroy(gameObject);
    }

    /// <summary>
    /// Contagem regressiva para o desaparecimento do item
    /// </summary>
    private IEnumerator DisappearCountdown()
    {
        while (currentDisappearTime > 0 && canBeCollected)
        {
            currentDisappearTime -= Time.deltaTime;
            
            // Fazer o item piscar quando está próximo de desaparecer
            if (currentDisappearTime <= 10f)
            {
                float blinkSpeed = Mathf.Lerp(0.1f, 0.05f, (10f - currentDisappearTime) / 10f);
                bool shouldShow = Mathf.Sin(Time.time / blinkSpeed) > 0;
                
                if (itemRenderer != null)
                {
                    itemRenderer.enabled = shouldShow;
                }
            }
            
            yield return null;
        }
        
        // Item desapareceu por timeout
        if (canBeCollected)
        {
            Debug.Log($"{item?.itemName ?? "Item"} desapareceu");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Configura o item (chamado pelo sistema de drop)
    /// </summary>
    /// <param name="newItem">Item a ser configurado</param>
    /// <param name="newQuantity">Quantidade do item</param>
    public void SetItem(Item newItem, int newQuantity = 1)
    {
        item = newItem;
        quantity = newQuantity;
        ApplyRarityColor();
    }

    /// <summary>
    /// Trigger para coleta quando o jogador passa por cima
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canBeCollected)
        {
            CollectItem();
        }
    }

    /// <summary>
    /// Desenha o range de coleta no editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }

    // Propriedades públicas para acesso
    public Item Item => item;
    public int Quantity => quantity;
    public bool CanBeCollected => canBeCollected;
}