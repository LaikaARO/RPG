using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Representa um slot individual na interface do inventário
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Componentes da UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image borderImage;
    
    [Header("Cores")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color hoverColor = Color.gray;
    
    // Variáveis privadas
    private InventorySlot currentSlot;
    private int slotIndex;
    private InventoryUI inventoryUI;
    private bool isSelected = false;

    /// <summary>
    /// Inicializa o slot com seu índice e referência à UI do inventário
    /// </summary>
    /// <param name="index">Índice do slot</param>
    /// <param name="ui">Referência à UI do inventário</param>
    public void Initialize(int index, InventoryUI ui)
    {
        slotIndex = index;
        inventoryUI = ui;
        
        // Configurar componentes se não foram atribuídos
        if (itemIcon == null)
            itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        
        if (quantityText == null)
            quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        if (borderImage == null)
            borderImage = transform.Find("Border")?.GetComponent<Image>();
        
        // Limpar o slot inicialmente
        ClearSlot();
    }

    /// <summary>
    /// Atualiza o slot com as informações do inventário
    /// </summary>
    /// <param name="slot">Dados do slot do inventário</param>
    public void UpdateSlot(InventorySlot slot)
    {
        currentSlot = slot;
        
        if (slot == null || slot.IsEmpty())
        {
            ClearSlot();
        }
        else
        {
            DisplayItem(slot);
        }
    }

    /// <summary>
    /// Exibe um item no slot
    /// </summary>
    /// <param name="slot">Dados do slot</param>
    private void DisplayItem(InventorySlot slot)
    {
        // Mostrar ícone do item
        if (itemIcon != null && slot.item != null)
        {
            itemIcon.sprite = slot.item.icon;
            itemIcon.enabled = true;
            
            // Aplicar cor da raridade
            if (slot.item != null)
            {
                itemIcon.color = slot.item.GetRarityColor();
            }
        }
        
        // Mostrar quantidade se maior que 1
        if (quantityText != null)
        {
            if (slot.quantity > 1)
            {
                quantityText.text = slot.quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.enabled = false;
            }
        }
        
        // Atualizar cor de fundo baseada na raridade
        if (backgroundImage != null && slot.item != null)
        {
            Color rarityColor = slot.item.GetRarityColor();
            rarityColor.a = 0.3f; // Tornar transparente
            backgroundImage.color = rarityColor;
        }
    }

    /// <summary>
    /// Limpa o slot
    /// </summary>
    private void ClearSlot()
    {
        // Esconder ícone
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
        
        // Esconder texto de quantidade
        if (quantityText != null)
        {
            quantityText.enabled = false;
        }
        
        // Restaurar cor normal do fundo
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }

    /// <summary>
    /// Implementação do clique no slot
    /// </summary>
    /// <param name="eventData">Dados do evento</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            // Duplo clique para usar o item
            if (eventData.clickCount == 2 && currentSlot != null && !currentSlot.IsEmpty())
            {
                if (currentSlot.item != null && currentSlot.item.itemType == ItemType.Consumable)
                {
                    if (InventorySystem.Instance != null)
                    {
                        InventorySystem.Instance.UseItem(slotIndex);
                    }
                    return;
                }
            }
            
            // Clique simples para selecionar
            inventoryUI.SelectSlot(this);
            SetSelected(true);
        }
    }

    /// <summary>
    /// Implementação do hover no slot
    /// </summary>
    /// <param name="eventData">Dados do evento</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && borderImage != null)
        {
            borderImage.color = hoverColor;
        }
    }

    /// <summary>
    /// Implementação da saída do hover
    /// </summary>
    /// <param name="eventData">Dados do evento</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected && borderImage != null)
        {
            borderImage.color = normalColor;
        }
    }

    /// <summary>
    /// Define se o slot está selecionado
    /// </summary>
    /// <param name="selected">Estado de seleção</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (borderImage != null)
        {
            borderImage.color = selected ? selectedColor : normalColor;
        }
        
        // Desmarcar outros slots
        if (selected && inventoryUI != null)
        {
            // Encontrar todos os outros slots e desmarcá-los
            InventorySlotUI[] allSlots = FindObjectsOfType<InventorySlotUI>();
            foreach (var slot in allSlots)
            {
                if (slot != this)
                {
                    slot.SetSelected(false);
                }
            }
        }
    }

    /// <summary>
    /// Retorna o slot atual
    /// </summary>
    /// <returns>Dados do slot</returns>
    public InventorySlot GetSlot()
    {
        return currentSlot;
    }

    /// <summary>
    /// Retorna o índice do slot
    /// </summary>
    public int SlotIndex => slotIndex;
}