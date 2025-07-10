using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    
    private int currentHealth;
    
    // 事件
    public event Action<int, int> OnHealthChanged; // 当前血量, 最大血量
    public event Action OnPlayerDied;
    
    private void Awake()
    {
        // 初始化血量
        currentHealth = maxHealth;
    }
    
    private void Start()
    {
        // 打印初始血量
        Debug.Log($"玩家初始血量: {currentHealth}/{maxHealth}");
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 打印当前血量
        Debug.Log($"玩家受到 {damage} 点伤害，当前血量: {currentHealth}/{maxHealth}");
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 打印当前血量
        Debug.Log($"玩家恢复 {amount} 点血量，当前血量: {currentHealth}/{maxHealth}");
    }
    
    private void Die()
    {
        Debug.Log("玩家死亡");
        
        // 触发死亡事件
        OnPlayerDied?.Invoke();
        
        // 禁用玩家控制器
        GetComponent<PlayerController>().enabled = false;
    }
    
    // 获取当前血量
    public int GetCurrentHealth() => currentHealth;
    
    // 获取最大血量
    public int GetMaxHealth() => maxHealth;
} 