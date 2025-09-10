using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerencia a integração entre todos os sistemas do jogo
/// Garante que os sistemas se comuniquem corretamente
/// </summary>
public class GameSystemsIntegration : MonoBehaviour
{
    [Header("Referencias dos Sistemas")]
    [SerializeField] private GameObject player;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private EquipmentUI equipmentUI;
    [SerializeField] private PlayerHUD playerHUD;
    
    [Header("Botões de Interface")]
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button characterButton;
    
    [Header("Painéis de UI")]
    [SerializeField] private GameObject mainUIPanel;
    [SerializeField] private GameObject pauseMenu;
    
    private bool isPaused = false;

    private void Start()
    {
        // Validar e configurar referências
        ValidateReferences();
        SetupSystemsIntegration();
        SetupUIButtons();
        
        // Configurar controles
        ConfigureInputs();
    }

    private void Update()
    {
        HandleInputs();
    }

    /// <summary>
    /// Valida se todas as referências necessárias estão configuradas
    /// </summary>
    private void ValidateReferences()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("GameSystemsIntegration: Jogador não encontrado! Certifique-se de que há um GameObject com a tag 'Player'.");
                return;
            }
        }
        
        // Buscar sistemas automaticamente se não foram atribuídos
        if (inventorySystem == null)
            inventorySystem = InventorySystem.Instance;
        
        if (equipmentManager == null)
            equipmentManager = player.GetComponent<EquipmentManager>();
        
        if (inventoryUI == null)
            inventoryUI = InventoryUI.Instance;
        
        if (equipmentUI == null)
            equipmentUI = EquipmentUI.Instance;
        
        if (playerHUD == null)
            playerHUD = FindObjectOfType<PlayerHUD>();
        
        // Log de sistemas encontrados
        Debug.Log($"Sistemas encontrados - Inventory: {inventorySystem != null}, Equipment: {equipmentManager != null}, UI: {inventoryUI != null}");
    }

    /// <summary>
    /// Configura a integração entre os sistemas
    /// </summary>
    private void SetupSystemsIntegration()
    {
        // Garantir que o EquipmentManager está no jogador
        if (equipmentManager == null && player != null)
        {
            equipmentManager = player.AddComponent<EquipmentManager>();
        }
        
        // Configurar eventos de integração
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged += OnInventoryChanged;
        }
        
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged += OnEquipmentChanged;
            equipmentManager.OnItemEquipped += OnItemEquipped;
            equipmentManager.OnItemUnequipped += OnItemUnequipped;
        }
    }

    /// <summary>
    /// Configura os botões da interface
    /// </summary>
    private void SetupUIButtons()
    {
        if (inventoryButton != null)
        {
            inventoryButton.onClick.AddListener(() => {
                ToggleInventory();
            });
        }
        
        if (equipmentButton != null)
        {
            equipmentButton.onClick.AddListener(() => {
                ToggleEquipment();
            });
        }
        
        if (characterButton != null)
        {
            characterButton.onClick.AddListener(() => {
                ToggleCharacterStats();
            });
        }
    }

    /// <summary>
    /// Configura os controles de entrada
    /// </summary>
    private void ConfigureInputs()
    {
        // Configurações básicas já estão nos sistemas individuais
        // Aqui podemos adicionar configurações globais se necessário
    }

    /// <summary>
    /// Gerencia as entradas do usuário
    /// </summary>
    private void HandleInputs()
    {
        // ESC para menu de pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
        
        // Tab para alternar entre inventário e equipamentos
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryUI != null && inventoryUI.IsInventoryOpen())
            {
                inventoryUI.CloseInventory();
                if (equipmentUI != null)
                {
                    equipmentUI.OpenEquipmentPanel();
                }
            }
            else if (equipmentUI != null && equipmentUI.IsEquipmentPanelOpen())
            {
                equipmentUI.CloseEquipmentPanel();
                if (inventoryUI != null)
                {
                    inventoryUI.OpenInventory();
                }
            }
            else
            {
                // Abrir inventário por padrão
                if (inventoryUI != null)
                {
                    inventoryUI.OpenInventory();
                }
            }
        }
    }

    /// <summary>
    /// Alterna o inventário
    /// </summary>
    public void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            if (inventoryUI.IsInventoryOpen())
            {
                inventoryUI.CloseInventory();
            }
            else
            {
                inventoryUI.OpenInventory();
            }
        }
    }

    /// <summary>
    /// Alterna o painel de equipamentos
    /// </summary>
    public void ToggleEquipment()
    {
        if (equipmentUI != null)
        {
            if (equipmentUI.IsEquipmentPanelOpen())
            {
                equipmentUI.CloseEquipmentPanel();
            }
            else
            {
                equipmentUI.OpenEquipmentPanel();
            }
        }
    }

    /// <summary>
    /// Alterna o painel de estatísticas do personagem
    /// </summary>
    public void ToggleCharacterStats()
    {
        if (playerHUD != null)
        {
            // Implementar toggle do painel de atributos
            // Isso pode ser feito através de um método no PlayerHUD
            Debug.Log("Toggling character stats panel");
        }
    }

    /// <summary>
    /// Alterna o menu de pausa
    /// </summary>
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }
        
        // Pausar/despausar o tempo do jogo
        Time.timeScale = isPaused ? 0f : 1f;
        
        // Fechar outros painéis quando pausar
        if (isPaused)
        {
            if (inventoryUI != null && inventoryUI.IsInventoryOpen())
            {
                inventoryUI.CloseInventory();
            }
            
            if (equipmentUI != null && equipmentUI.IsEquipmentPanelOpen())
            {
                equipmentUI.CloseEquipmentPanel();
            }
        }
    }

    /// <summary>
    /// Evento chamado quando o inventário muda
    /// </summary>
    private void OnInventoryChanged()
    {
        Debug.Log("Inventário foi alterado");
        
        // Aqui você pode adicionar lógica adicional que deve acontecer
        // sempre que o inventário mudar
    }

    /// <summary>
    /// Evento chamado quando equipamentos mudam
    /// </summary>
    private void OnEquipmentChanged()
    {
        Debug.Log("Equipamentos foram alterados");
        
        // Forçar recálculo de estatísticas
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.CalculateStats();
            }
        }
    }

    /// <summary>
    /// Evento chamado quando um item é equipado
    /// </summary>
    private void OnItemEquipped(EquipmentSlot slot, EquipmentItem item)
    {
        Debug.Log($"Item equipado: {item.itemName} no slot {slot}");
        
        // Mostrar notificação na UI
        if (playerHUD != null)
        {
            playerHUD.ShowNotification($"Equipado: {item.itemName}", Color.green);
        }
    }

    /// <summary>
    /// Evento chamado quando um item é desequipado
    /// </summary>
    private void OnItemUnequipped(EquipmentSlot slot, EquipmentItem item)
    {
        Debug.Log($"Item desequipado: {item.itemName} do slot {slot}");
        
        // Mostrar notificação na UI
        if (playerHUD != null)
        {
            playerHUD.ShowNotification($"Desequipado: {item.itemName}", Color.yellow);
        }
    }

    /// <summary>
    /// Método utilitário para criar um item de equipamento para teste
    /// </summary>
    [ContextMenu("Adicionar Item de Teste")]
    public void AddTestEquipment()
    {
        if (inventorySystem == null)
        {
            Debug.LogWarning("Sistema de inventário não encontrado!");
            return;
        }
        
        // Este método é apenas para teste - em um jogo real,
        // os itens seriam criados como ScriptableObjects
        Debug.Log("Para adicionar itens de teste, crie EquipmentItems como ScriptableObjects no projeto.");
    }

    /// <summary>
    /// Método para salvar o estado atual do jogador
    /// </summary>
    public void SavePlayerData()
    {
        // Implementar sistema de save aqui
        // Por exemplo, salvar inventário, equipamentos, estatísticas, etc.
        Debug.Log("Salvando dados do jogador...");
        
        PlayerPrefs.SetString("LastSaveTime", System.DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Método para carregar dados salvos do jogador
    /// </summary>
    public void LoadPlayerData()
    {
        // Implementar sistema de load aqui
        Debug.Log("Carregando dados do jogador...");
        
        if (PlayerPrefs.HasKey("LastSaveTime"))
        {
            string lastSaveTime = PlayerPrefs.GetString("LastSaveTime");
            Debug.Log($"Último save: {System.DateTime.FromBinary(System.Convert.ToInt64(lastSaveTime))}");
        }
    }

    /// <summary>
    /// Método para resetar todos os sistemas
    /// </summary>
    public void ResetAllSystems()
    {
        Debug.Log("Resetando todos os sistemas...");
        
        // Limpar inventário
        if (inventorySystem != null)
        {
            // Implementar método de reset no InventorySystem se necessário
        }
        
        // Limpar equipamentos
        if (equipmentManager != null)
        {
            // Desequipar todos os itens
            var equippedItems = equipmentManager.GetAllEquippedItems();
            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    equipmentManager.UnequipItem(kvp.Key);
                }
            }
        }
        
        // Resetar estatísticas do jogador
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.ClearAllModifiers();
                stats.CalculateStats();
            }
        }
    }

    private void OnDestroy()
    {
        // Limpar eventos para evitar vazamentos de memória
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= OnInventoryChanged;
        }
        
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
            equipmentManager.OnItemEquipped -= OnItemEquipped;
            equipmentManager.OnItemUnequipped -= OnItemUnequipped;
        }
    }

    /// <summary>
    /// Método para debug - mostra o estado atual de todos os sistemas
    /// </summary>
    [ContextMenu("Debug - Mostrar Estado dos Sistemas")]
    public void DebugShowSystemsState()
    {
        Debug.Log("=== ESTADO DOS SISTEMAS ===");
        
        if (player != null)
        {
            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                Debug.Log("ESTATÍSTICAS DO JOGADOR:\n" + stats.GetStatsInfo());
            }
        }
        
        if (equipmentManager != null)
        {
            var equipped = equipmentManager.GetAllEquippedItems();
            Debug.Log($"EQUIPAMENTOS: {equipped.Count} itens equipados");
            foreach (var kvp in equipped)
            {
                if (kvp.Value != null)
                {
                    Debug.Log($"  {kvp.Key}: {kvp.Value.itemName}");
                }
            }
        }
        
        if (inventorySystem != null)
        {
            Debug.Log($"INVENTÁRIO: {inventorySystem.GetSlotCount()} slots total");
            // Adicionar mais detalhes se necessário
        }
    }
}