using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Controla a interface do usuário do inventário
/// Versão atualizada com suporte a equipamentos
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Button closeButton;
    
    [Header("Informações do Item")]
    [SerializeField] private GameObject itemInfoPanel;
    [SerializeField] private Image itemInfoIcon;
    [SerializeField] private TextMeshProUGUI itemInfoName;
    [SerializeField] private TextMeshProUGUI itemInfoDescription;
    [SerializeField] private TextMeshProUGUI itemInfoValue;
    [SerializeField] private Button useItemButton;
    [SerializeField] private Button equipItemButton;
    
    // Variáveis privadas
    private List<InventorySlotUI> slotUIList = new List<InventorySlotUI>();
    private InventorySlotUI selectedSlot = null;
    private bool isInventoryOpen = false;
    
    // Singleton
    public static InventoryUI Instance { get; private set; }

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

    private void Start()
    {
        // Configurar eventos
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += UpdateInventoryUI;
        }
        
        // Configurar botões
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInventory);
        }
        
        if (useItemButton != null)
        {
            useItemButton.onClick.AddListener(UseSelectedItem);
        }
        
        if (equipItemButton != null)
        {
            equipItemButton.onClick.AddListener(EquipSelectedItem);
        }
        
        // Inicializar slots
        CreateInventorySlots();
        
        // Esconder inventário inicialmente
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        if (itemInfoPanel != null)
        {
            itemInfoPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Verificar tecla I para abrir/fechar inventário
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isInventoryOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }
        
        // Verificar tecla ESC para fechar inventário
        if (Input.GetKeyDown(KeyCode.Escape) && isInventoryOpen)
        {
            CloseInventory();
        }
    }

    /// <summary>
    /// Cria os slots visuais do inventário
    /// </summary>
    private void CreateInventorySlots()
    {
        if (InventorySystem.Instance == null || slotPrefab == null || slotsParent == null) return;
        
        int slotCount = InventorySystem.Instance.GetSlotCount();
        
        // Limpar slots existentes
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        slotUIList.Clear();
        
        // Criar novos slots
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, slotsParent);
            InventorySlotUI slotUI = slotObject.GetComponent<InventorySlotUI>();
            
            if (slotUI != null)
            {
                slotUI.Initialize(i, this);
                slotUIList.Add(slotUI);
            }
        }
    }

    /// <summary>
    /// Atualiza a UI do inventário
    /// </summary>
    private void UpdateInventoryUI()
    {
        if (InventorySystem.Instance == null) return;
        
        var slots = InventorySystem.Instance.GetAllSlots();
        
        for (int i = 0; i < slotUIList.Count && i < slots.Count; i++)
        {
            slotUIList[i].UpdateSlot(slots[i]);
        }
    }

    /// <summary>
    /// Abre o inventário
    /// </summary>
    public void OpenInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            isInventoryOpen = true;
            UpdateInventoryUI();
        }
    }

    /// <summary>
    /// Fecha o inventário
    /// </summary>
    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
            
            // Esconder painel de informações do item
            if (itemInfoPanel != null)
            {
                itemInfoPanel.SetActive(false);
            }
        }
        
        // Limpar seleção
        selectedSlot = null;
    }

    /// <summary>
    /// Seleciona um slot do inventário
    /// </summary>
    /// <param name="slotUI">Slot selecionado</param>
    public void SelectSlot(InventorySlotUI slotUI)
    {
        selectedSlot = slotUI;
        
        // Mostrar informações do item se houver
        if (slotUI != null && slotUI.GetSlot() != null && !slotUI.GetSlot().IsEmpty())
        {
            ShowItemInfo(slotUI.GetSlot());
            
            // Notificar o EquipmentUI se um equipamento foi selecionado
            if (slotUI.GetSlot().item is EquipmentItem equipmentItem)
            {
                if (EquipmentUI.Instance != null)
                {
                    EquipmentUI.Instance.OnEquipmentItemSelected(equipmentItem);
                }
            }
        }
        else
        {
            HideItemInfo();
        }
    }

    /// <summary>
    /// Mostra as informações de um item
    /// </summary>
    /// <param name="slot">Slot com o item</param>
    private void ShowItemInfo(InventorySlot slot)
    {
        if (itemInfoPanel == null || slot == null || slot.item == null) return;
        
        itemInfoPanel.SetActive(true);
        
        // Atualizar ícone
        if (itemInfoIcon != null)
        {
            itemInfoIcon.sprite = slot.item.icon;
            itemInfoIcon.color = slot.item.GetRarityColor();
        }
        
        // Atualizar nome
        if (itemInfoName != null)
        {
            itemInfoName.text = slot.item.itemName;
            itemInfoName.color = slot.item.GetRarityColor();
        }
        
        // Atualizar descrição
        if (itemInfoDescription != null)
        {
            string description = slot.item.ToString();
            if (slot.quantity > 1)
            {
                description += $"\n\nQuantidade: {slot.quantity}";
            }
            itemInfoDescription.text = description;
        }
        
        // Atualizar valor
        if (itemInfoValue != null)
        {
            itemInfoValue.text = $"Valor: {slot.item.value} moedas";
        }
        
        // Configurar botões baseado no tipo do item
        bool isConsumable = slot.item.itemType == ItemType.Consumable;
        bool isEquipment = slot.item is EquipmentItem;
        
        if (useItemButton != null)
        {
            useItemButton.gameObject.SetActive(isConsumable);
        }
        
        if (equipItemButton != null)
        {
            equipItemButton.gameObject.SetActive(isEquipment);
            
            // Verificar se pode equipar
            if (isEquipment)
            {
                EquipmentItem equipment = slot.item as EquipmentItem;
                CharacterStats playerStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterStats>();
                LevelSystem playerLevel = GameObject.FindGameObjectWithTag("Player")?.GetComponent<LevelSystem>();
                
                bool canEquip = equipment.CanEquip(playerStats, playerLevel);
                equipItemButton.interactable = canEquip;
                
                // Atualizar texto do botão baseado nos requisitos
                var buttonText = equipItemButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = canEquip ? "Equipar" : "Não Atende Requisitos";
                }
            }
        }
    }

    /// <summary>
    /// Esconde as informações do item
    /// </summary>
    private void HideItemInfo()
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Usa o item selecionado
    /// </summary>
    private void UseSelectedItem()
    {
        if (selectedSlot != null && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.UseItem(selectedSlot.SlotIndex);
            
            // Atualizar as informações após usar o item
            if (selectedSlot.GetSlot() != null && !selectedSlot.GetSlot().IsEmpty())
            {
                ShowItemInfo(selectedSlot.GetSlot());
            }
            else
            {
                HideItemInfo();
            }
        }
    }

    /// <summary>
    /// Equipa o item selecionado
    /// </summary>
    private void EquipSelectedItem()
    {
        if (selectedSlot != null && selectedSlot.GetSlot() != null && !selectedSlot.GetSlot().IsEmpty())
        {
            Item item = selectedSlot.GetSlot().item;
            if (item is EquipmentItem equipmentItem)
            {
                // Tentar equipar o item
                if (EquipmentManager.Instance != null)
                {
                    EquipmentItem previousItem = EquipmentManager.Instance.EquipItem(equipmentItem);
                    
                    if (previousItem != null)
                    {
                        Debug.Log($"Equipado {equipmentItem.itemName}, desequipado {previousItem.itemName}");
                    }
                    else
                    {
                        Debug.Log($"Equipado {equipmentItem.itemName}");
                    }
                    
                    // Atualizar informações do item
                    if (selectedSlot.GetSlot() != null && !selectedSlot.GetSlot().IsEmpty())
                    {
                        ShowItemInfo(selectedSlot.GetSlot());
                    }
                    else
                    {
                        HideItemInfo();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Verifica se o inventário está aberto
    /// </summary>
    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }

    /// <summary>
    /// Retorna o slot selecionado atualmente
    /// </summary>
    public InventorySlotUI GetSelectedSlot()
    {
        return selectedSlot;
    }

    private void OnDestroy()
    {
        // Cancelar inscrição nos eventos
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= UpdateInventoryUI;
        }
    }
}