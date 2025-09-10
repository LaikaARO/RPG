using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Controla a exibição de texto de dano flutuante que aparece quando um personagem recebe dano.
/// </summary>
public class DamageTextUI : MonoBehaviour
{
    [Header("Configurações de Aparência")]
    [Tooltip("Componente de texto que exibirá o valor do dano")]
    [SerializeField] private TextMeshProUGUI damageText;
    
    [Tooltip("Cor para dano normal")]
    [SerializeField] private Color normalDamageColor = Color.white;
    
    [Tooltip("Cor para dano crítico")]
    [SerializeField] private Color criticalDamageColor = Color.red;
    
    [Header("Configurações de Animação")]
    [Tooltip("Velocidade de movimento para cima")]
    [SerializeField] private float floatSpeed = 1.0f;
    
    [Tooltip("Duração total da animação em segundos")]
    [SerializeField] private float duration = 1.5f;
    
    [Tooltip("Escala inicial do texto")]
    [SerializeField] private float initialScale = 0.5f;
    
    [Tooltip("Escala máxima do texto")]
    [SerializeField] private float maxScale = 1.2f;
    
    [Tooltip("Velocidade de escala")]
    [SerializeField] private float scaleSpeed = 5.0f;
    
    // Variáveis privadas para controle da animação
    private float currentDuration;
    private Vector3 randomOffset;
    private bool isCritical;
    
    /// <summary>
    /// Inicializa o texto de dano com o valor especificado.
    /// </summary>
    /// <param name="damageAmount">Quantidade de dano a ser exibida</param>
    /// <param name="isCriticalHit">Se o dano foi um acerto crítico</param>
    public void Initialize(int damageAmount, bool isCriticalHit = false)
    {
        // Configurar o texto e a cor com base no tipo de dano
        isCritical = isCriticalHit;
        damageText.text = damageAmount.ToString();
        damageText.color = isCriticalHit ? criticalDamageColor : normalDamageColor;
        
        // Configurar a escala inicial
        transform.localScale = Vector3.one * initialScale;
        
        // Adicionar um pequeno deslocamento aleatório para evitar sobreposição
        randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(0.1f, 0.3f),
            0
        );
        
        // Iniciar a animação
        currentDuration = 0f;
        StartCoroutine(AnimateText());
    }
    
    /// <summary>
    /// Corrotina que anima o texto de dano (movimento, escala e desaparecimento).
    /// </summary>
    private IEnumerator AnimateText()
    {
        // Fase 1: Aumentar a escala rapidamente
        float currentScale = initialScale;
        while (currentScale < maxScale)
        {
            currentScale += scaleSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Min(currentScale, maxScale);
            
            // Também move um pouco para cima durante esta fase
            transform.position += Vector3.up * (floatSpeed * 0.5f) * Time.deltaTime;
            
            yield return null;
        }
        
        // Fase 2: Flutuar para cima e diminuir gradualmente
        while (currentDuration < duration)
        {
            // Atualizar posição (movimento para cima + offset aleatório)
            transform.position += (Vector3.up + randomOffset) * floatSpeed * Time.deltaTime;
            
            // Atualizar escala (diminuir gradualmente)
            float scaleProgress = 1.0f - (currentDuration / duration);
            transform.localScale = Vector3.one * Mathf.Lerp(initialScale, maxScale, scaleProgress);
            
            // Atualizar transparência (fade out)
            Color textColor = damageText.color;
            textColor.a = Mathf.Lerp(0, 1, 1.0f - (currentDuration / duration));
            damageText.color = textColor;
            
            // Se for crítico, adicionar um efeito de pulso
            if (isCritical)
            {
                float pulse = 1.0f + 0.2f * Mathf.Sin(currentDuration * 10f);
                transform.localScale = transform.localScale * pulse;
            }
            
            currentDuration += Time.deltaTime;
            yield return null;
        }
        
        // Destruir o objeto quando a animação terminar
        Destroy(gameObject);
    }
}