using UnityEngine;

/// <summary>
/// Classe de extensão que fornece métodos utilitários para trabalhar com elementos de UI arrastáveis.
/// </summary>
public static class DraggableUIExtension
{
    /// <summary>
    /// Torna um GameObject com RectTransform arrastável adicionando o componente DraggableUI.
    /// </summary>
    /// <param name="uiElement">O GameObject que contém um RectTransform</param>
    /// <param name="returnToOriginalPosition">Se o elemento deve retornar à posição original quando solto</param>
    /// <param name="keepInParentBounds">Se o elemento deve ficar restrito aos limites do pai</param>
    /// <param name="dragSpeed">Velocidade de arrasto (1 = normal)</param>
    /// <returns>O componente DraggableUI adicionado</returns>
    public static DraggableUI MakeDraggable(this GameObject uiElement, 
                                          bool returnToOriginalPosition = false, 
                                          bool keepInParentBounds = true, 
                                          float dragSpeed = 1.0f)
    {
        // Verificar se o GameObject tem um RectTransform
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning($"O objeto {uiElement.name} não possui um RectTransform. Adicionando um RectTransform antes de torná-lo arrastável.");
            rectTransform = uiElement.AddComponent<RectTransform>();
        }
        
        // Verificar se o GameObject já tem um componente DraggableUI
        DraggableUI draggable = uiElement.GetComponent<DraggableUI>();
        
        // Se não tiver, adicionar um novo
        if (draggable == null)
        {
            draggable = uiElement.AddComponent<DraggableUI>();
        }
        
        // Configurar as propriedades via reflexão (já que são SerializeField privados)
        var returnField = typeof(DraggableUI).GetField("returnToOriginalPosition", 
                                                    System.Reflection.BindingFlags.NonPublic | 
                                                    System.Reflection.BindingFlags.Instance);
        
        var boundsField = typeof(DraggableUI).GetField("keepInParentBounds", 
                                                    System.Reflection.BindingFlags.NonPublic | 
                                                    System.Reflection.BindingFlags.Instance);
        
        var speedField = typeof(DraggableUI).GetField("dragSpeed", 
                                                   System.Reflection.BindingFlags.NonPublic | 
                                                   System.Reflection.BindingFlags.Instance);
        
        if (returnField != null) returnField.SetValue(draggable, returnToOriginalPosition);
        if (boundsField != null) boundsField.SetValue(draggable, keepInParentBounds);
        if (speedField != null) speedField.SetValue(draggable, dragSpeed);
        
        return draggable;
    }
    
    /// <summary>
    /// Verifica se um GameObject tem o componente DraggableUI.
    /// </summary>
    /// <param name="uiElement">O GameObject a verificar</param>
    /// <returns>True se o elemento for arrastável, false caso contrário</returns>
    public static bool IsDraggable(this GameObject uiElement)
    {
        return uiElement.GetComponent<DraggableUI>() != null;
    }
    
    /// <summary>
    /// Remove a funcionalidade de arrasto de um elemento de UI.
    /// </summary>
    /// <param name="uiElement">O GameObject do qual remover a funcionalidade de arrasto</param>
    public static void RemoveDraggable(this GameObject uiElement)
    {
        DraggableUI draggable = uiElement.GetComponent<DraggableUI>();
        if (draggable != null)
        {
            Object.Destroy(draggable);
        }
    }
}