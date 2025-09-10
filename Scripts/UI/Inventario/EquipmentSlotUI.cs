using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Representa um slot individual de equipamento na UI
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Componentes da UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image slotTypeIcon; // Ícone que mostra o tipo do slot (espada, capacete, etc.)
    
    [Header("Cores")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color hoverColor = Color.gray;
    [SerializeField] private Color emptySlotColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    // Variáveis privadas
    private EquipmentSlot equipmentSlot;
    private EquipmentItem currentItem;
    private EquipmentUI equipmentUI;
    private bool isSelected = false;

    /// <summary>
    /// Inicializa o slot com seu tipo e referência à UI
    /// </summary>
    /// <param name="slot">Tipo do slot</param>
    /// <param name="ui">Referência à UI de equipamentos</param>
    public void Initialize(EquipmentSlot slot, EquipmentUI ui)
    {
        equipmentSlot = slot;
        equipmentUI = ui;
        
        // Configurar componentes se não foram atribuídos
        if (itemIcon == null)
            itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        if (borderImage == null)
            borderImage = transform.Find("Border")?.GetComponent<Image>();
        
        if (slotTypeIcon == null)
            slotTypeIcon = transform.Find("SlotTypeIcon")?.GetComponent<Image>();
        
        // Configurar o ícone do tipo de slot
        SetupSlotTypeIcon();
        
        // Limpar o slot inicialmente
        ClearSlot();
    }

    /// <summary>
    /// Configura o ícone do tipo de slot
    /// </summary>
    private void SetupSlotTypeIcon()
    {
        if (slotTypeIcon == null) return;
        
        // Aqui você pode configurar diferentes ícones para cada tipo de slot
        // Por exemplo, carregar sprites específicos de Resources ou usar um ScriptableObject
        
        // Para simplicidade, vamos apenas definir uma cor base para cada tipo
        Color slotColor = GetSlotTypeColor();
        slotTypeIcon.color = slotColor;
        
        // Se você tiver sprites específicos para cada tipo de slot, pode configurar aqui:
        // slotTypeIcon.sprite = GetSlotTypeSprite(equipmentSlot);
    }

    /// <summary>
    /// Retorna uma cor baseada no tipo de slot
    /// </summary>
    private Color GetSlotTypeColor()
    {
        switch (equipmentSlot)
        {
            case EquipmentSlot.MainHand:
            case EquipmentSlot.OffHand:
                return new Color(1f, 0.5f, 0f); // Laranja para armas
            
            case EquipmentSlot.Helmet:
            case EquipmentSlot.Chest:
            case EquipmentSlot.Legs:
            case EquipmentSlot.Boots:
            case EquipmentSlot.Gloves:
                return new Color(0.7f, 0.7f, 0.7f); // Cinza para armaduras
            
            case EquipmentSlot.Ring1:
            case EquipmentSlot.Ring2:
            case EquipmentSlot.Necklace:
            case EquipmentSlot.Earring1:
            case EquipmentSlot.Earring2:
                return new Color(1f, 1f, 0f); // Amarelo para acessórios
            
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Atualiza o slot com um item
    /// </summary>
    /// <param name="item">Item a ser exibido (null para limpar)</param>
    public void UpdateSlot(EquipmentItem item)
    {
        currentItem = item;
        
        if (item == null)
        {
            ClearSlot();
        }
        else
        {
            DisplayItem(item);
        }
    }

    /// <summary>
    /// Exibe um item no slot
    /// </summary>
    /// <param name="item">Item a ser exibido</param>
    private void DisplayItem(EquipmentItem item)
    {
        // Mostrar ícone do item
        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
            itemIcon.color = Color.white; // Cor normal para itens equipados
        }
        
        // Atualizar cor de fundo baseada na raridade
        if (backgroundImage != null)
        {
            Color rarityColor = item.GetRarityColor();
            rarityColor.a = 0.4f; // Tornar ligeiramente transparente
            backgroundImage.color = rarityColor;
        }
        
        // Esconder ícone do tipo de slot quando há um item
        if (slotTypeIcon != null)
        {
            slotTypeIcon.enabled = false;
        }
    }

    /// <summary>
    /// Limpa o slot
    /// </summary>
    private void ClearSlot()
    {
        // Esconder ícone do item
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
        
        // Restaurar cor de slot vazio
        if (backgroundImage != null)
        {
            backgroundImage.color = emptySlotColor;
        }
        
        // Mostrar ícone do tipo de slot quando vazio
        if (slotTypeIcon != null)
        {
            slotTypeIcon.enabled = true;
        }
    }

    /// <summary>
    /// Implementação do clique no slot
    /// </summary>
    /// <param name="eventData">Dados do evento</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (equipmentUI != null)
        {
            // Duplo clique para desequipar
            if (eventData.clickCount == 2 && currentItem != null)
            {
                if (EquipmentManager.Instance != null)
                {
                    EquipmentManager.Instance.UnequipItem(equipmentSlot);
                }
                return;
            }
            
            // Clique simples para selecionar
            equipmentUI.OnSlotSelected(equipmentSlot, currentItem);
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
        
        // Mostrar tooltip se houver item
        if (currentItem != null)
        {
            // Aqui você pode implementar um sistema de tooltip
            ShowTooltip(currentItem);
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
        
        // Esconder tooltip
        HideTooltip();
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
        
        // Desmarcar outros slots se este foi selecionado
        if (selected)
        {
            EquipmentSlotUI[] allSlots = FindObjectsOfType<EquipmentSlotUI>();
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
    /// Mostra tooltip com informações do item
    /// </summary>
    /// <param name="item">Item para mostrar tooltip</param>
    private void ShowTooltip(EquipmentItem item)
    {
        // Implementação básica de tooltip
        // Você pode criar um sistema mais elaborado com um prefab de tooltip
        if (TooltipManager.Instance != null)
        {
            string tooltipText = $"{item.itemName}\n{item.description}";
            TooltipManager.Instance.ShowTooltip(tooltipText, transform.position);
        }
    }

    /// <summary>
    /// Esconde o tooltip
    /// </summary>
    private void HideTooltip()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    /// <summary>
    /// Retorna o item atual do slot
    /// </summary>
    public EquipmentItem GetCurrentItem()
    {
        return currentItem;
    }

    /// <summary>
    /// Retorna o tipo do slot
    /// </summary>
    public EquipmentSlot GetSlotType()
    {
        return equipmentSlot;
    }

    /// <summary>
    /// Verifica se o slot pode aceitar um determinado item
    /// </summary>
    /// <param name="item">Item a verificar</param>
    /// <returns>True se pode aceitar</returns>
    public bool CanAcceptItem(EquipmentItem item)
    {
        if (item == null) return false;
        
        // Verificar se o item pode ser equipado neste slot
        return item.equipmentSlot == equipmentSlot;
    }
}

/// <summary>
/// Gerenciador simples de tooltips (você pode criar um mais elaborado)
/// </summary>
public class TooltipManager : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private Canvas tooltipCanvas;
    
    private GameObject currentTooltip;
    
    public static TooltipManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowTooltip(string text, Vector3 position)
    {
        HideTooltip();
        
        if (tooltipPrefab != null && tooltipCanvas != null)
        {
            currentTooltip = Instantiate(tooltipPrefab, tooltipCanvas.transform);
            
            // Configurar texto do tooltip
            var tooltipText = currentTooltip.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tooltipText != null)
            {
                tooltipText.text = text;
            }
            
            // Posicionar o tooltip
            RectTransform rectTransform = currentTooltip.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
                rectTransform.position = screenPosition;
            }
        }
    }

    public void HideTooltip()
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
    }
}