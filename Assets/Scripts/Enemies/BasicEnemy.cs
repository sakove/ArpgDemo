using UnityEngine;

public class BasicEnemy : EnemyBase
{
    [Header("Basic Enemy Settings")]
    [SerializeField] private bool canMove = true;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 如果没有设置玩家层，默认使用"Player"层
        if (playerLayer == 0)
        {
            playerLayer = LayerMask.GetMask("Player");
        }
    }
    
    protected override void Start()
    {
        base.Start();
        
        // 初始化敌人
        Debug.Log($"敌人初始化，当前血量: {currentHealth}");
    }
    
    protected override void Update()
    {
        // 如果不能移动，就不执行基类的移动逻辑
        if (!canMove)
        {
            // 敌人不移动，但仍然可以攻击玩家
            if (target != null)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);
                if (distanceToTarget <= attackRange)
                {
                    if (Time.time >= lastAttackTime + attackCooldown)
                    {
                        Attack();
                    }
                }
            }
            
            // 更新动画
            UpdateAnimations();
        }
        else
        {
            // 使用基类的完整行为
            base.Update();
        }
    }
    
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        
        // 打印当前血量
        Debug.Log($"敌人受到 {damage} 点伤害，当前血量: {currentHealth}");
    }
    
    protected override void Die()
    {
        Debug.Log("敌人死亡");
        base.Die();
    }
} 