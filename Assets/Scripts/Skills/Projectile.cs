using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用投射物脚本，用于处理各种投射类技能（火球、箭矢、魔法弹等）
/// 设计灵感来自《死亡细胞》和《星界战士》等游戏
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("基本属性")]
    [Tooltip("投射物造成的伤害")]
    public int damage = 10;
    [Tooltip("投射物移动速度")]
    public float speed = 10f;
    [Tooltip("投射物存在时间（秒）")]
    public float lifetime = 5f;
    [Tooltip("投射物移动方向")]
    public Vector2 direction = Vector2.right;
    
    [Header("物理设置")]
    [Tooltip("是否受重力影响")]
    public bool affectedByGravity = false;
    [Tooltip("重力缩放因子")]
    public float gravityScale = 1f;
    [Tooltip("是否在碰撞时销毁")]
    public bool destroyOnHit = true;
    [Tooltip("最大穿透敌人数量（0表示无穿透）")]
    public int maxPenetration = 0;
    
    [Header("视觉效果")]
    [Tooltip("命中特效预制体")]
    public GameObject hitEffectPrefab;
    [Tooltip("销毁特效预制体")]
    public GameObject destroyEffectPrefab;
    [Tooltip("拖尾渲染器")]
    public TrailRenderer trailRenderer;
    
    [Header("音效")]
    [Tooltip("发射音效")]
    public AudioClip launchSound;
    [Tooltip("命中音效")]
    public AudioClip hitSound;
    [Tooltip("销毁音效")]
    public AudioClip destroySound;
    
    // 私有变量
    private Rigidbody2D rb;
    private int penetrationCount = 0;
    private float timer = 0f;
    private List<GameObject> hitObjects = new List<GameObject>();
    private bool isInitialized = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        // 设置物理属性
        rb.gravityScale = affectedByGravity ? gravityScale : 0f;
        
        // 如果有拖尾渲染器，确保它被正确设置
        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }
        
        // 播放发射音效
        if (launchSound != null)
        {
            AudioSource.PlayClipAtPoint(launchSound, transform.position);
        }
    }
    
    private void Start()
    {
        Initialize();
    }
    
    /// <summary>
    /// 初始化投射物，设置方向和速度
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;
        
        // 根据方向设置旋转
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 设置速度
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
        
        isInitialized = true;
    }
    
    private void Update()
    {
        // 更新生命周期计时器
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            DestroyProjectile(false);
        }
    }
    
    private void FixedUpdate()
    {
        // 如果没有刚体或者已经使用刚体设置了速度，则不需要手动移动
        if (rb == null || rb.gravityScale > 0)
        {
            return;
        }
        
        // 手动移动投射物（如果需要）
        if (rb.linearVelocity.sqrMagnitude < 0.1f)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    /// <summary>
    /// 处理碰撞逻辑
    /// </summary>
    private void HandleCollision(GameObject other)
    {
        // 忽略已经命中的对象
        if (hitObjects.Contains(other))
        {
            return;
        }
        
        // 检查是否是可伤害对象
        IDamageable damageable = other.GetComponent<IDamageable>();
        
        // 如果碰到敌人
        if (damageable != null)
        {
            // 应用伤害
            damageable.TakeDamage(damage);
            
            // 播放命中音效
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }
            
            // 生成命中特效
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // 添加到已命中列表
            hitObjects.Add(other);
            
            // 增加穿透计数
            penetrationCount++;
            
            // 检查是否达到最大穿透数
            if (destroyOnHit || penetrationCount > maxPenetration)
            {
                DestroyProjectile(true);
                return;
            }
        }
        // 如果碰到环境
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            DestroyProjectile(true);
        }
    }
    
    /// <summary>
    /// 销毁投射物
    /// </summary>
    /// <param name="hitSomething">是否因为命中物体而销毁</param>
    private void DestroyProjectile(bool hitSomething)
    {
        // 生成销毁特效
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // 播放销毁音效
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }
        
        // 禁用碰撞器和渲染器，但保留游戏对象以便音效和特效完成播放
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // 停止移动
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }
        
        // 禁用拖尾
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }
        
        // 延迟销毁游戏对象，给特效和音效时间完成
        Destroy(gameObject, 2f);
    }
    
    /// <summary>
    /// 设置投射物属性
    /// </summary>
    public void SetProperties(int newDamage, float newSpeed, float newLifetime, Vector2 newDirection)
    {
        damage = newDamage;
        speed = newSpeed;
        lifetime = newLifetime;
        direction = newDirection;
        
        // 重新初始化
        isInitialized = false;
        Initialize();
    }
} 