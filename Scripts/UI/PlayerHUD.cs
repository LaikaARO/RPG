using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla a interface do usuário (HUD) do jogador, exibindo barras de vida, mana e experiência.
/// Versão atualizada com integração do inventário.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("Referências de Componentes")]
    [SerializeField] private CharacterStats playerStats;
    [SerializeField] private LevelSystem playerLevelSystem;

    [Header("Barras de Status")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider manaBar;
    [SerializeField] private Slider expBar;

    [Header("Textos de Status")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("Painel de Atributos")]
    [SerializeField] private GameObject attributesPanel;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI dexterityText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI vitalityText;
    [SerializeField] private TextMeshProUGUI attributePointsText;

    [Header("Botões de Atributos")]
    [SerializeField] private Button addStrengthButton;
    [SerializeField] private Button addDexterityButton;
    [SerializeField] private Button addIntelligenceButton;
    [SerializeField] private Button addVitalityButton;
    [SerializeField] private Button toggleAttributesPanelButton;

    [Header("Botão do Inventário")]
    [SerializeField] private Button inventoryButton;
    
    [Header("Notificações")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;

    private void Start()
    {
        // Encontrar referências se não foram atribuídas no inspetor
        FindReferences();
        
        // Configurar listeners de eventos
        SetupEventListeners();
        
        // Configurar botões de atributos
        SetupAttributeButtons();
        
        // Configurar botão do inventário
        SetupInventoryButton();
        
        // Atualizar UI inicial com um pequeno delay para garantir que os valores estejam carregados
        Invoke("UpdateAllUI", 0.1f);

        // Esconder o painel de atributos inicialmente
        if (attributesPanel != null)
        {
            attributesPanel.SetActive(false);
        }
        
        // Esconder painel de notificações inicialmente
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Verificar se a tecla C foi pressionada para abrir/fechar o painel de atributos
        if (Input.GetKeyDown(KeyCode.C) && attributesPanel != null)
        {
            attributesPanel.SetActive(!attributesPanel.activeSelf);
        }
    }

    /// <summary>
    /// Encontra referências aos componentes necessários se não foram atribuídos no inspetor
    /// </summary>
    private void FindReferences()
    {
        // Encontrar referências aos componentes do jogador
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerStats = player.GetComponent<CharacterStats>();
            }
        }

        if (playerLevelSystem == null && playerStats != null)
        {
            playerLevelSystem = playerStats.GetComponent<LevelSystem>();
        }

        // Verificar se encontrou as referências necessárias
        if (playerStats == null)
        {
            Debug.LogError("PlayerHUD: Não foi possível encontrar o componente CharacterStats do jogador.");
        }

        if (playerLevelSystem == null)
        {
            Debug.LogError("PlayerHUD: Não foi possível encontrar o componente LevelSystem do jogador.");
        }
    }

    /// <summary>
    /// Configura os listeners de eventos para atualizar a UI quando os valores mudarem
    /// </summary>
    private void SetupEventListeners()
    {
        if (playerStats != null)
        {
            // Inscrever-se nos eventos de mudança de vida e mana
            playerStats.OnHealthChanged += UpdateHealthUI;
            playerStats.OnManaChanged += UpdateManaUI;
            playerStats.OnStatsUpdated += UpdateAttributesUI;
        }

        if (playerLevelSystem != null)
        {
            // Inscrever-se nos eventos de ganho de experiência e subida de nível
            playerLevelSystem.OnExpGain += UpdateExpUI;
            playerLevelSystem.OnLevelUp += UpdateLevelUI;
        }
    }

    /// <summary>
    /// Configura os botões de adição de atributos
    /// </summary>
    private void SetupAttributeButtons()
    {
        // Configurar botões de atributos
        if (addStrengthButton != null)
        {
            addStrengthButton.onClick.AddListener(() => {
                playerStats.AddStrength(1);
                UpdateAttributesUI();
            });
        }

        if (addDexterityButton != null)
        {
            addDexterityButton.onClick.AddListener(() => {
                playerStats.AddDexterity(1);
                UpdateAttributesUI();
            });
        }

        if (addIntelligenceButton != null)
        {
            addIntelligenceButton.onClick.AddListener(() => {
                playerStats.AddIntelligence(1);
                UpdateAttributesUI();
            });
        }

        if (addVitalityButton != null)
        {
            addVitalityButton.onClick.AddListener(() => {
                playerStats.AddVitality(1);
                UpdateAttributesUI();
            });
        }

        // Configurar botão de toggle do painel de atributos
        if (toggleAttributesPanelButton != null && attributesPanel != null)
        {
            toggleAttributesPanelButton.onClick.AddListener(() => {
                attributesPanel.SetActive(!attributesPanel.activeSelf);
            });
        }
    }

    /// <summary>
    /// Configura o botão do inventário
    /// </summary>
    private void SetupInventoryButton()
    {
        if (inventoryButton != null)
        {
            inventoryButton.onClick.AddListener(() => {
                if (InventoryUI.Instance != null)
                {
                    if (InventoryUI.Instance.IsInventoryOpen())
                    {
                        InventoryUI.Instance.CloseInventory();
                    }
                    else
                    {
                        InventoryUI.Instance.OpenInventory();
                    }
                }
                else
                {
                    Debug.LogWarning("InventoryUI não encontrado!");
                }
            });
        }
    }

    /// <summary>
    /// Atualiza todos os elementos da UI
    /// </summary>
    private void UpdateAllUI()
    {
        if (playerStats != null)
        {
            // Atualizar barras de vida e mana
            UpdateHealthUI(playerStats.CurrentHealth, playerStats.MaxHealth);
            UpdateManaUI(playerStats.CurrentMana, playerStats.MaxMana);
            UpdateAttributesUI();
        }

        if (playerLevelSystem != null)
        {
            // Atualizar nível e experiência
            UpdateLevelUI(playerLevelSystem.Level);
            UpdateExpUI(playerLevelSystem.CurrentExp, playerLevelSystem.GetExpToNextLevel(), 0);
        }
    }

    /// <summary>
    /// Atualiza a UI de vida
    /// </summary>
    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    /// <summary>
    /// Atualiza a UI de mana
    /// </summary>
    private void UpdateManaUI(int currentMana, int maxMana)
    {
        if (manaBar != null)
        {
            manaBar.maxValue = maxMana;
            manaBar.value = currentMana;
        }

        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }

    /// <summary>
    /// Atualiza a UI de experiência
    /// </summary>
    private void UpdateExpUI(int currentExp, int expToNextLevel, int expGained)
    {
        if (expBar != null)
        {
            expBar.maxValue = expToNextLevel;
            expBar.value = currentExp;
        }

        if (expText != null)
        {
            expText.text = $"{currentExp}/{expToNextLevel}";
        }

        // Mostrar notificação de ganho de experiência se necessário
        if (expGained > 0)
        {
            ShowNotification($"+{expGained} XP", Color.yellow);
        }
    }

    /// <summary>
    /// Atualiza a UI de nível
    /// </summary>
    private void UpdateLevelUI(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Nível {level}";
        }

        // Mostrar notificação de subida de nível
        ShowNotification($"LEVEL UP! Nível {level}", Color.yellow);
        Debug.Log($"Subiu para o nível {level}!");
    }

    /// <summary>
    /// Atualiza a UI de atributos
    /// </summary>
    private void UpdateAttributesUI()
    {
        if (playerStats == null) return;

        // Atualizar textos de atributos
        if (strengthText != null)
        {
            strengthText.text = $"Força: {playerStats.Strength}";
        }

        if (dexterityText != null)
        {
            dexterityText.text = $"Destreza: {playerStats.Dexterity}";
        }

        if (intelligenceText != null)
        {
            intelligenceText.text = $"Inteligência: {playerStats.Intelligence}";
        }

        if (vitalityText != null)
        {
            vitalityText.text = $"Vitalidade: {playerStats.Vitality}";
        }

        if (attributePointsText != null)
        {
            attributePointsText.text = $"Pontos: {playerStats.AttributePoints}";
        }

        // Habilitar/desabilitar botões de atributos com base nos pontos disponíveis
        bool hasPoints = playerStats.AttributePoints > 0;
        if (addStrengthButton != null) addStrengthButton.interactable = hasPoints;
        if (addDexterityButton != null) addDexterityButton.interactable = hasPoints;
        if (addIntelligenceButton != null) addIntelligenceButton.interactable = hasPoints;
        if (addVitalityButton != null) addVitalityButton.interactable = hasPoints;
    }

    /// <summary>
    /// Mostra uma notificação temporária na tela
    /// </summary>
    /// <param name="message">Mensagem a ser exibida</param>
    /// <param name="color">Cor da notificação</param>
    public void ShowNotification(string message, Color color)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationText.color = color;
            notificationPanel.SetActive(true);
            
            // Esconder a notificação após alguns segundos
            CancelInvoke("HideNotification");
            Invoke("HideNotification", 3f);
        }
    }

    /// <summary>
    /// Esconde a notificação
    /// </summary>
    private void HideNotification()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Cancelar inscrição nos eventos para evitar memory leaks
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthUI;
            playerStats.OnManaChanged -= UpdateManaUI;
            playerStats.OnStatsUpdated -= UpdateAttributesUI;
        }

        if (playerLevelSystem != null)
        {
            playerLevelSystem.OnExpGain -= UpdateExpUI;
            playerLevelSystem.OnLevelUp -= UpdateLevelUI;
        }
    }
}