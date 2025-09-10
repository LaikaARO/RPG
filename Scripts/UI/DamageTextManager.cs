using UnityEngine;

/// <summary>
/// Gerencia a criação e exibição de textos de dano no mundo do jogo.
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Prefab do texto de dano")]
    [SerializeField] private GameObject damageTextPrefab;
    
    [Tooltip("Canvas onde os textos de dano serão criados")]
    [SerializeField] private Canvas worldSpaceCanvas;
    
    [Header("Configurações")]
    [Tooltip("Altura acima do alvo onde o texto aparecerá")]
    [SerializeField] private float heightOffset = 1.5f;
    
    // Singleton para acesso fácil
    private static DamageTextManager _instance;
    public static DamageTextManager Instance { get { return _instance; } }
    
    private void Awake()
    {
        // Configurar singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // Verificar se temos as referências necessárias
        if (damageTextPrefab == null)
        {
            Debug.LogError("DamageTextManager: Prefab de texto de dano não atribuído!");
        }
        
        if (worldSpaceCanvas == null)
        {
            Debug.LogError("DamageTextManager: Canvas não atribuído!");
        }
        else if (worldSpaceCanvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogWarning("DamageTextManager: O Canvas deve estar no modo World Space para funcionar corretamente!");
        }
    }
    
    /// <summary>
    /// Cria um texto de dano na posição do alvo.
    /// </summary>
    /// <param name="targetPosition">Posição do alvo que recebeu o dano</param>
    /// <param name="damageAmount">Quantidade de dano a ser exibida</param>
    /// <param name="isCritical">Se o dano foi crítico</param>
    public void ShowDamageText(Vector3 targetPosition, int damageAmount, bool isCritical = false)
    {
        if (damageTextPrefab == null || worldSpaceCanvas == null) return;
        
        // Calcular a posição onde o texto deve aparecer
        Vector3 spawnPosition = targetPosition + Vector3.up * heightOffset;
        
        // Criar o texto de dano como filho do canvas
        GameObject damageTextInstance = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, worldSpaceCanvas.transform);
        
        // Configurar o componente DamageTextUI
        DamageTextUI damageTextUI = damageTextInstance.GetComponent<DamageTextUI>();
        if (damageTextUI != null)
        {
            damageTextUI.Initialize(damageAmount, isCritical);
        }
        else
        {
            Debug.LogError("DamageTextManager: O prefab não contém o componente DamageTextUI!");
            Destroy(damageTextInstance);
        }
    }
}