using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gerenciador que aplica automaticamente o componente DraggableUI a elementos de UI específicos.
/// Este script deve ser adicionado a um GameObject no Canvas principal.
/// </summary>
public class DraggableUIManager : MonoBehaviour
{
    [System.Serializable]
    public class DraggableUISettings
    {
        public string uiTag = "DraggableUI";
        public bool returnToOriginalPosition = false;
        public bool keepInParentBounds = true;
        public float dragSpeed = 1.0f;
    }
    
    [Tooltip("Configurações para elementos de UI arrastáveis")]
    [SerializeField] private DraggableUISettings settings = new DraggableUISettings();
    
    [Tooltip("Se verdadeiro, procura automaticamente por elementos com a tag especificada ao iniciar")]
    [SerializeField] private bool autoFindElements = true;
    
    [Tooltip("Elementos específicos para tornar arrastáveis (além dos encontrados pela tag)")]
    [SerializeField] private GameObject[] specificElements;
    
    // Lista de todos os elementos arrastáveis gerenciados
    private List<GameObject> managedElements = new List<GameObject>();
    
    private void Start()
    {
        if (autoFindElements)
        {
            FindAndApplyDraggable();
        }
        
        // Aplicar aos elementos específicos
        ApplyToSpecificElements();
    }
    
    /// <summary>
    /// Encontra todos os elementos com a tag especificada e aplica o componente DraggableUI
    /// </summary>
    public void FindAndApplyDraggable()
    {
        GameObject[] taggedElements = GameObject.FindGameObjectsWithTag(settings.uiTag);
        
        foreach (GameObject element in taggedElements)
        {
            ApplyDraggable(element);
        }
        
        Debug.Log($"DraggableUIManager: {taggedElements.Length} elementos encontrados com a tag '{settings.uiTag}' e tornados arrastáveis.");
    }
    
    /// <summary>
    /// Aplica o componente DraggableUI aos elementos específicos
    /// </summary>
    private void ApplyToSpecificElements()
    {
        if (specificElements == null) return;
        
        foreach (GameObject element in specificElements)
        {
            if (element != null)
            {
                ApplyDraggable(element);
            }
        }
    }
    
    /// <summary>
    /// Aplica o componente DraggableUI a um elemento específico
    /// </summary>
    /// <param name="element">O elemento a tornar arrastável</param>
    public void ApplyDraggable(GameObject element)
    {
        if (element == null) return;
        
        // Verificar se o elemento já está sendo gerenciado
        if (!managedElements.Contains(element))
        {
            // Verificar se o elemento é um objeto de UI (deve ter ou poder receber um RectTransform)
            if (element.transform is RectTransform || element.GetComponent<RectTransform>() != null)
            {
                // Tornar o elemento arrastável com as configurações especificadas
                element.MakeDraggable(
                    settings.returnToOriginalPosition,
                    settings.keepInParentBounds,
                    settings.dragSpeed
                );
                
                // Adicionar à lista de elementos gerenciados
                managedElements.Add(element);
                
                Debug.Log($"DraggableUIManager: Elemento '{element.name}' agora é arrastável.");
            }
            else
            {
                Debug.LogWarning($"DraggableUIManager: O elemento '{element.name}' não é um objeto de UI válido. Certifique-se de que ele esteja dentro de um Canvas.");
            }
        }
    }
    
    /// <summary>
    /// Remove a funcionalidade de arrasto de todos os elementos gerenciados
    /// </summary>
    public void RemoveAllDraggable()
    {
        foreach (GameObject element in managedElements)
        {
            if (element != null)
            {
                element.RemoveDraggable();
            }
        }
        
        managedElements.Clear();
        Debug.Log("DraggableUIManager: Funcionalidade de arrasto removida de todos os elementos.");
    }
    
    /// <summary>
    /// Reseta a posição de todos os elementos arrastáveis para suas posições originais
    /// </summary>
    public void ResetAllPositions()
    {
        foreach (GameObject element in managedElements)
        {
            if (element != null && element.IsDraggable())
            {
                DraggableUI draggable = element.GetComponent<DraggableUI>();
                if (draggable != null)
                {
                    draggable.ResetPosition();
                }
            }
        }
        
        Debug.Log("DraggableUIManager: Posições de todos os elementos arrastáveis resetadas.");
    }
}