using UnityEngine;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    
    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxEnergy = 100;
    
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    
    // Current stats
    private int currentHealth;
    private int currentEnergy;
    
    // Events
    public event Action<int, int> OnHealthChanged; // Current, Max
    public event Action<int, int> OnEnergyChanged; // Current, Max
    public event Action OnPlayerDied;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            

             // Initialize stats
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponentInChildren<PlayerController>();
        }
    }
    
    // Health management
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    } 
    
    // Energy management
    public bool UseEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            return true;
        }
        return false;
    }
    
    public void RestoreEnergy(int amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
    
    // Death handling
    private void Die()
    {
        // Disable player controller
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Trigger death event
        OnPlayerDied?.Invoke();
        
        // Additional death logic can be added here
        // e.g., play death animation, show game over screen, etc.
    } 
    
    // Respawn logic
    public void Respawn(Vector3 position)
    {
        // Reset stats
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        
        // Update UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        
        // Enable player controller and teleport
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        TeleportToPosition(position);
    }
    
    // Used by GameManager to position player at spawn points
    public void TeleportToPosition(Vector3 position)
    {
        transform.position = position;
    }
    
    // Getters for current stats
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentEnergy() => currentEnergy;
    public int GetMaxEnergy() => maxEnergy;
} 