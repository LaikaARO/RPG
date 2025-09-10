using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Sistema de inventário que gerencia os itens do jogador
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Header("Configurações do Inventário")]
    [Tooltip("Número máximo de slots no inventário")]
    [SerializeField] private int maxSlots = 30;
    
    // Lista de slots do inventário
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    
    // Eventos
    public event Action OnInventoryChanged;
    public event Action<Item, int> OnItemAdded;
    public event Action<Item, int> OnItemRemoved;
    
    // Singleton para acesso fácil
    public static InventorySystem Instance { get; private set; }

    private void Awake()
    {
        // Configurar singleton
        if (Instance == null)
        {
            Instance = this;
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inicializa o inventário com slots vazios
    /// </summary>
    private void InitializeInventory()
    {
        inventorySlots.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            inventorySlots.Add(new InventorySlot());
        }
    }

    /// <summary>
    /// Adiciona um item ao inventário
    /// </summary>
    /// <param name="item">Item a ser adicionado</param>
    /// <param name="quantity">Quantidade do item</param>
    /// <returns>True se o item foi adicionado com sucesso</returns>
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        // Se o item é empilhável, tentar adicionar a pilhas existentes primeiro
        if (item.isStackable)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].item == item && inventorySlots[i].quantity < item.maxStackSize)
                {
                    int availableSpace = item.maxStackSize - inventorySlots[i].quantity;
                    int amountToAdd = Mathf.Min(quantity, availableSpace);
                    
                    inventorySlots[i].quantity += amountToAdd;
                    quantity -= amountToAdd;
                    
                    if (quantity <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        OnItemAdded?.Invoke(item, amountToAdd);
                        Debug.Log($"Adicionado {item.itemName} ao inventário (empilhado)");
                        return true;
                    }
                }
            }
        }

        // Procurar slots vazios para adicionar o restante
        while (quantity > 0)
        {
            int emptySlotIndex = FindEmptySlot();
            if (emptySlotIndex == -1)
            {
                Debug.Log("Inventário cheio!");
                return false; // Inventário cheio
            }

            int amountToAdd = item.isStackable ? Mathf.Min(quantity, item.maxStackSize) : 1;
            inventorySlots[emptySlotIndex].item = item;
            inventorySlots[emptySlotIndex].quantity = amountToAdd;
            quantity -= amountToAdd;
        }

        OnInventoryChanged?.Invoke();
        OnItemAdded?.Invoke(item, quantity);
        Debug.Log($"Adicionado {item.itemName} ao inventário");
        return true;
    }

    /// <summary>
    /// Remove uma quantidade específica de um item do inventário
    /// </summary>
    /// <param name="item">Item a ser removido</param>
    /// <param name="quantity">Quantidade a ser removida</param>
    /// <returns>True se o item foi removido com sucesso</returns>
    public bool RemoveItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        int remainingToRemove = quantity;

        for (int i = 0; i < inventorySlots.Count && remainingToRemove > 0; i++)
        {
            if (inventorySlots[i].item == item)
            {
                int amountToRemove = Mathf.Min(remainingToRemove, inventorySlots[i].quantity);
                inventorySlots[i].quantity -= amountToRemove;
                remainingToRemove -= amountToRemove;

                // Se o slot ficar vazio, limpar
                if (inventorySlots[i].quantity <= 0)
                {
                    inventorySlots[i].item = null;
                    inventorySlots[i].quantity = 0;
                }
            }
        }

        if (remainingToRemove < quantity)
        {
            int amountRemoved = quantity - remainingToRemove;
            OnInventoryChanged?.Invoke();
            OnItemRemoved?.Invoke(item, amountRemoved);
            Debug.Log($"Removido {amountRemoved} de {item.itemName} do inventário");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Usa um item do inventário
    /// </summary>
    /// <param name="slotIndex">Índice do slot</param>
    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count) return;
        
        InventorySlot slot = inventorySlots[slotIndex];
        if (slot.item == null) return;

        // Usar o item
        CharacterStats playerStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterStats>();
        if (playerStats != null)
        {
            slot.item.UseItem(playerStats);
            
            // Se é um item consumível, remover uma unidade
            if (slot.item.itemType == ItemType.Consumable)
            {
                RemoveItem(slot.item, 1);
            }
        }
    }

    /// <summary>
    /// Verifica se o inventário contém uma quantidade específica de um item
    /// </summary>
    /// <param name="item">Item a ser verificado</param>
    /// <param name="quantity">Quantidade necessária</param>
    /// <returns>True se contém a quantidade especificada</returns>
    public bool HasItem(Item item, int quantity = 1)
    {
        int totalCount = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item)
            {
                totalCount += slot.quantity;
                if (totalCount >= quantity)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Retorna a quantidade total de um item no inventário
    /// </summary>
    /// <param name="item">Item a ser contado</param>
    /// <returns>Quantidade total do item</returns>
    public int GetItemCount(Item item)
    {
        int totalCount = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.item == item)
            {
                totalCount += slot.quantity;
            }
        }
        return totalCount;
    }

    /// <summary>
    /// Encontra o primeiro slot vazio
    /// </summary>
    /// <returns>Índice do slot vazio ou -1 se não houver</returns>
    private int FindEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].item == null)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Retorna todos os slots do inventário
    /// </summary>
    public List<InventorySlot> GetAllSlots()
    {
        return new List<InventorySlot>(inventorySlots);
    }

    /// <summary>
    /// Retorna um slot específico
    /// </summary>
    /// <param name="index">Índice do slot</param>
    /// <returns>Slot do inventário ou null se inválido</returns>
    public InventorySlot GetSlot(int index)
    {
        if (index >= 0 && index < inventorySlots.Count)
        {
            return inventorySlots[index];
        }
        return null;
    }

    /// <summary>
    /// Retorna o número de slots no inventário
    /// </summary>
    public int GetSlotCount()
    {
        return inventorySlots.Count;
    }
}

/// <summary>
/// Representa um slot individual do inventário
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;

    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }

    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    /// <summary>
    /// Verifica se o slot está vazio
    /// </summary>
    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }

    /// <summary>
    /// Limpa o slot
    /// </summary>
    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}