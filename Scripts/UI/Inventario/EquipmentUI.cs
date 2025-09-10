using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Interface do usuário para o sistema de equipamentos
/// </summary>
public class EquipmentUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private Button equipmentPanelButton;
    [SerializeField] private Button closeEquipmentButton;
    
    [Header("Slots de Equipamento - UI")]
    [SerializeField] private EquipmentSlotUI mainHandSlot;
    [SerializeField] private EquipmentSlotUI offHandSlot;
    [SerializeField] private EquipmentSlotUI helmetSlot;
    [SerializeField] private EquipmentSlotUI chestSlot;
    [SerializeField] private EquipmentSlotUI legsSlot;
    [SerializeField] private EquipmentSlotUI bootsSlot;
    [SerializeField] private EquipmentSlotUI glovesSlot;
    [SerializeField] private EquipmentSlotUI ring1Slot;
    [SerializeField] private EquipmentSlotUI ring2Slot;
    [SerializeField] private EquipmentSlotUI necklaceSlot;
    [SerializeField] private EquipmentSlotUI earring1Slot;
    [SerializeField] private EquipmentSlotUI earring2Slot;
    
    [Header("Informações do Equipamento")]
    [SerializeField] private GameObject equipmentInfoPanel;
    [SerializeField] private Image equipmentInfoIcon;
    [SerializeField] private TextMeshProUGUI equipmentInfoName;
    [SerializeField] private TextMeshProUGUI equipmentInfoDescription;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button unequipButton;
    
    [Header("Estatísticas")]
    [SerializeField] private TextMeshProUGUI totalAttackText;
    [SerializeField] private TextMeshProUGUI totalDefenseText;
    [SerializeField] private TextMeshProUGUI totalMagicText;
    
    // Dicionário para mapear slots para componentes UI
    private Dictionary<EquipmentSlot, EquipmentSlotUI> slotUIMapping = new Dictionary<EquipmentSlot, EquipmentSlotUI>();
    
    // Estado da UI
    private bool isEquipmentPanelOpen = false;
    private EquipmentItem selectedEquipmentItem = null;
    private EquipmentSlot selectedSlot = EquipmentSlot.MainHand;
    
    // Singleton
    public static EquipmentUI Instance { get; private set; }

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
        SetupSlotMapping();
        SetupEventListeners();
        SetupButtons();
        
        // Inicializar UI
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
        }
        
        if (equipmentInfoPanel != null)
        {
            equipmentInfoPanel.SetActive(false);
        }
        
        // Atualizar UI inicial
        UpdateEquipmentUI();
    }

    private void Update()
    {
        // Tecla E para abrir/fechar painel de equipamentos
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleEquipmentPanel();
        }
    }

    /// <summary>
    /// Configura o mapeamento entre slots e componentes UI
    /// </summary>
    private void SetupSlotMapping()
    {
        slotUIMapping[EquipmentSlot.MainHand] = mainHandSlot;
        slotUIMapping[EquipmentSlot.OffHand] = offHandSlot;
        slotUIMapping[EquipmentSlot.Helmet] = helmetSlot;
        slotUIMapping[EquipmentSlot.Chest] = chestSlot;
        slotUIMapping[EquipmentSlot.Legs] = legsSlot;
        slotUIMapping[EquipmentSlot.Boots] = bootsSlot;
        slotUIMapping[EquipmentSlot.Gloves] = glovesSlot;
        slotUIMapping[EquipmentSlot.Ring1] = ring1Slot;
        slotUIMapping[EquipmentSlot.Ring2] = ring2Slot;
        slotUIMapping[EquipmentSlot.Necklace] = necklaceSlot;
        slotUIMapping[EquipmentSlot.Earring1] = earring1Slot;
        slotUIMapping[EquipmentSlot.Earring2] = earring2Slot;
        
        // Inicializar cada slot UI
        foreach (var kvp in slotUIMapping)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Initialize(kvp.Key, this);
            }
        }
    }

    /// <summary>
    /// Configura os event listeners
    /// </summary>
    private void SetupEventListeners()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnItemEquipped += OnItemEquipped;
            EquipmentManager.Instance.OnItemUnequipped += OnItemUnequipped;
            EquipmentManager.Instance.OnEquipmentChanged += UpdateEquipmentUI;
        }
    }

    /// <summary>
    /// Configura os botões
    /// </summary>
    private void SetupButtons()
    {
        if (equipmentPanelButton != null)
        {
            equipmentPanelButton.onClick.AddListener(ToggleEquipmentPanel);
        }
        
        if (closeEquipmentButton != null)
        {
            closeEquipmentButton.onClick.AddListener(CloseEquipmentPanel);
        }
        
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(EquipSelectedItem);
        }
        
        if (unequipButton != null)
        {
            unequipButton.onClick.AddListener(UnequipSelectedSlot);
        }
    }

    /// <summary>
    /// Alterna o painel de equipamentos
    /// </summary>
    public void ToggleEquipmentPanel()
    {
        if (isEquipmentPanelOpen)
        {
            CloseEquipmentPanel();
        }
        else
        {
            OpenEquipmentPanel();
        }
    }

    /// <summary>
    /// Abre o painel de equipamentos
    /// </summary>
    public void OpenEquipmentPanel()
    {
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(true);
            isEquipmentPanelOpen = true;
            UpdateEquipmentUI();
        }
    }

    /// <summary>
    /// Fecha o painel de equipamentos
    /// </summary>
    public void CloseEquipmentPanel()
    {
        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
            isEquipmentPanelOpen = false;
        }
        
        if (equipmentInfoPanel != null)
        {
            equipmentInfoPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Atualiza toda a UI de equipamentos
    /// </summary>
    private void UpdateEquipmentUI()
    {
        if (EquipmentManager.Instance == null) return;
        
        // Atualizar cada slot
        var equippedItems = EquipmentManager.Instance.GetAllEquippedItems();
        foreach (var kvp in slotUIMapping)
        {
            if (kvp.Value != null)
            {
                EquipmentItem item = equippedItems.ContainsKey(kvp.Key) ? equippedItems[kvp.Key] : null;
                kvp.Value.UpdateSlot(item);
            }
        }
        
        // Atualizar estatísticas totais
        UpdateStatsDisplay();
    }

    /// <summary>
    /// Atualiza a exibição das estatísticas totais
    /// </summary>
    private void UpdateStatsDisplay()
    {
        if (EquipmentManager.Instance == null) return;
        
        if (totalAttackText != null)
        {
            float totalAttack = EquipmentManager.Instance.GetTotalAttackPower();
            totalAttackText.text = $"Ataque: {totalAttack:F1}";
        }
        
        if (totalDefenseText != null)
        {
            int totalPhysicalDef = EquipmentManager.Instance.GetTotalPhysicalDefense();
            int totalMagicalDef = EquipmentManager.Instance.GetTotalMagicalDefense();
            totalDefenseText.text = $"Defesa: {totalPhysicalDef}Físico / {totalMagicalDef}Mágica";
        }
        
        if (totalMagicText != null)
        {
            float totalMagic = EquipmentManager.Instance.GetTotalMagicPower();
            totalMagicText.text = $"Magia: {totalMagic:F1}";
        }
    }

    /// <summary>
    /// Chamado quando um slot é selecionado
    /// </summary>
    /// <param name="slot">Slot selecionado</param>
    /// <param name="item">Item no slot (pode ser null)</param>
    public void OnSlotSelected(EquipmentSlot slot, EquipmentItem item)
    {
        selectedSlot = slot;
        selectedEquipmentItem = item;
        
        if (item != null)
        {
            ShowEquipmentInfo(item, false); // false = já equipado
        }
        else
        {
            // Slot vazio - verificar se há item compatível selecionado no inventário
            if (InventoryUI.Instance != null && InventoryUI.Instance.IsInventoryOpen())
            {
                // Lógica para mostrar itens compatíveis será implementada
                if (equipmentInfoPanel != null)
                {
                    equipmentInfoPanel.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Chamado quando um item de equipamento é selecionado no inventário
    /// </summary>
    /// <param name="item">Item selecionado</param>
    public void OnEquipmentItemSelected(EquipmentItem item)
    {
        selectedEquipmentItem = item;
        ShowEquipmentInfo(item, true); // true = não equipado
    }

    /// <summary>
    /// Mostra as informações de um equipamento
    /// </summary>
    /// <param name="item">Item a mostrar</param>
    /// <param name="canEquip">Se pode ser equipado (true) ou desequipado (false)</param>
    private void ShowEquipmentInfo(EquipmentItem item, bool canEquip)
    {
        if (equipmentInfoPanel == null || item == null) return;
        
        equipmentInfoPanel.SetActive(true);
        
        // Atualizar ícone
        if (equipmentInfoIcon != null)
        {
            equipmentInfoIcon.sprite = item.icon;
            equipmentInfoIcon.color = item.GetRarityColor();
        }
        
        // Atualizar nome
        if (equipmentInfoName != null)
        {
            equipmentInfoName.text = item.itemName;
            equipmentInfoName.color = item.GetRarityColor();
        }
        
        // Atualizar descrição
        if (equipmentInfoDescription != null)
        {
            equipmentInfoDescription.text = item.ToString();
        }
        
        // Configurar botões
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(canEquip);
        }
        
        if (unequipButton != null)
        {
            unequipButton.gameObject.SetActive(!canEquip);
        }
    }

    /// <summary>
    /// Equipa o item selecionado
    /// </summary>
    private void EquipSelectedItem()
    {
        if (selectedEquipmentItem != null && EquipmentManager.Instance != null)
        {
            EquipmentItem previousItem = EquipmentManager.Instance.EquipItem(selectedEquipmentItem);
            
            // Se havia um item anterior, ele foi automaticamente adicionado ao inventário
            if (previousItem != null)
            {
                Debug.Log($"Item {previousItem.itemName} foi desequipado e adicionado ao inventário");
            }
        }
    }

    /// <summary>
    /// Desequipa o item do slot selecionado
    /// </summary>
    private void UnequipSelectedSlot()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentItem unequippedItem = EquipmentManager.Instance.UnequipItem(selectedSlot);
            
            if (unequippedItem != null)
            {
                Debug.Log($"Item {unequippedItem.itemName} foi desequipado");
                
                // Fechar painel de informações
                if (equipmentInfoPanel != null)
                {
                    equipmentInfoPanel.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Evento chamado quando um item é equipado
    /// </summary>
    private void OnItemEquipped(EquipmentSlot slot, EquipmentItem item)
    {
        // A UI será atualizada automaticamente via UpdateEquipmentUI
    }

    /// <summary>
    /// Evento chamado quando um item é desequipado
    /// </summary>
    private void OnItemUnequipped(EquipmentSlot slot, EquipmentItem item)
    {
        // A UI será atualizada automaticamente via UpdateEquipmentUI
    }

    /// <summary>
    /// Verifica se o painel de equipamentos está aberto
    /// </summary>
    public bool IsEquipmentPanelOpen()
    {
        return isEquipmentPanelOpen;
    }

    private void OnDestroy()
    {
        // Cancelar inscrição nos eventos
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnItemEquipped -= OnItemEquipped;
            EquipmentManager.Instance.OnItemUnequipped -= OnItemUnequipped;
            EquipmentManager.Instance.OnEquipmentChanged -= UpdateEquipmentUI;
        }
    }
}