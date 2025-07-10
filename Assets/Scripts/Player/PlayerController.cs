using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 玩家控制器，负责处理玩家的基本移动和输入
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float airControlFactor = 0.8f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityTime = 1f;
    
    // 组件
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private PlayerStateMachine stateMachine;
    private CombatController combatController;
    
    // 输入动作
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction dashAction;
    
    // 移动状态
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float dashCooldownCounter;
    
    // 健康状态
    private int currentHealth;
    private bool isInvincible;
    private float invincibilityCounter;
    
    // 输入状态
    private bool jumpInput;
    private bool jumpHeld;
    private bool attackInput;
    private bool dashInput;
    
    // 属性
    public bool IsGrounded => isGrounded;
    public float CoyoteTimeCounter => coyoteTimeCounter;
    public bool JumpInput => jumpInput;
    public bool JumpHeld => jumpHeld;
    public bool AttackInput => attackInput;
    public bool DashInput => dashInput;
    public bool CanDash => dashCooldownCounter <= 0f;
    public float DashDuration => dashDuration;
    public bool IsFacingRight => isFacingRight;
    public float AirControlFactor => airControlFactor;
    
    private void Awake()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        
        // 尝试获取状态机和战斗控制器（可能尚未添加）
        stateMachine = GetComponent<PlayerStateMachine>();
        combatController = GetComponent<CombatController>();
        
        // 设置输入动作
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            attackAction = playerInput.actions["Attack"];
            dashAction = playerInput.actions["Dash"];
        }
        
        // 如果地面检测点为空，创建一个地面检测点
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0f, -1f, 0f);
            groundCheck = groundCheckObj.transform;
        }
        
        // 如果没有设置地面层，默认使用"Ground"层
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
        
        // 初始化健康状态
        currentHealth = maxHealth;
    }
    
    private void OnEnable()
    {
        // 订阅输入事件
        if (jumpAction != null) jumpAction.performed += OnJumpPerformed;
        if (jumpAction != null) jumpAction.canceled += OnJumpCanceled;
        if (attackAction != null) attackAction.performed += OnAttackPerformed;
        if (dashAction != null) dashAction.performed += OnDashPerformed;
    }
    
    private void OnDisable()
    {
        // 取消订阅输入事件
        if (jumpAction != null) jumpAction.performed -= OnJumpPerformed;
        if (jumpAction != null) jumpAction.canceled -= OnJumpCanceled;
        if (attackAction != null) attackAction.performed -= OnAttackPerformed;
        if (dashAction != null) dashAction.performed -= OnDashPerformed;
    }
    
    private void Update()
    {
        // 获取移动输入
        moveInput = moveAction.ReadValue<Vector2>();
        
        // 检查是否接触地面
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 处理土狼时间
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        // 处理跳跃缓冲
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        
        // 处理冲刺冷却
        if (dashCooldownCounter > 0)
        {
            dashCooldownCounter -= Time.deltaTime;
        }
        
        // 处理无敌时间
        if (isInvincible)
        {
            invincibilityCounter -= Time.deltaTime;
            if (invincibilityCounter <= 0)
            {
                isInvincible = false;
            }
        }
        
        // 处理朝向
        HandleFacing();
        
        // 重置输入状态
        attackInput = false;
        dashInput = false;
    }
    
    /// <summary>
    /// 获取移动输入
    /// </summary>
    /// <returns>移动输入向量</returns>
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
    
    /// <summary>
    /// 移动玩家
    /// </summary>
    /// <param name="horizontalInput">水平输入值</param>
    /// <param name="speedModifier">速度修正因子</param>
    public void Move(float horizontalInput, float speedModifier = 1f)
    {
        // 应用水平移动
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed * speedModifier, rb.linearVelocity.y);
    }
    
    /// <summary>
    /// 执行跳跃
    /// </summary>
    public void PerformJump()
    {
        // 设置垂直速度为跳跃力
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        
        // 重置土狼时间和跳跃缓冲
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        
        // 播放跳跃音效（如果有）
        // AudioManager.Instance?.PlaySound("Jump");
    }
    
    /// <summary>
    /// 减小跳跃高度（短跳）
    /// </summary>
    public void CutJump()
    {
        // 如果玩家正在上升，减小上升速度
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }
    
    /// <summary>
    /// 执行冲刺
    /// </summary>
    /// <param name="direction">冲刺方向</param>
    public void PerformDash(Vector2 direction)
    {
        // 应用冲刺速度
        rb.linearVelocity = direction.normalized * dashSpeed;
        
        // 暂时禁用重力
        rb.gravityScale = 0f;
        
        // 播放冲刺音效和特效
        // AudioManager.Instance?.PlaySound("Dash");
        // EffectsManager.Instance?.PlayEffect("DashTrail", transform.position);
        
        // 相机震动（如果有CameraShake组件）
        if (FindObjectOfType<CameraShake>() != null)
        {
            FindObjectOfType<CameraShake>().ShakeCamera(0.1f, 0.2f);
        }
    }
    
    /// <summary>
    /// 应用冲刺速度
    /// </summary>
    /// <param name="direction">冲刺方向</param>
    public void ApplyDashVelocity(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * dashSpeed;
    }
    
    /// <summary>
    /// 恢复重力
    /// </summary>
    public void RestoreGravity()
    {
        rb.gravityScale = 1f;
    }
    
    /// <summary>
    /// 开始冲刺冷却
    /// </summary>
    public void StartDashCooldown()
    {
        dashCooldownCounter = dashCooldown;
    }
    
    /// <summary>
    /// 应用下落加速度
    /// </summary>
    public void ApplyFallMultiplier()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
    
    /// <summary>
    /// 设置跳跃缓冲
    /// </summary>
    public void SetJumpBuffer()
    {
        jumpBufferCounter = jumpBufferTime;
    }
    
    /// <summary>
    /// 重置土狼时间
    /// </summary>
    public void ResetCoyoteTime()
    {
        coyoteTimeCounter = 0f;
    }
    
    /// <summary>
    /// 处理角色朝向
    /// </summary>
    private void HandleFacing()
    {
        // 根据移动方向翻转角色
        if (moveInput.x > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput.x < -0.1f && isFacingRight)
        {
            Flip();
        }
    }
    
    /// <summary>
    /// 翻转角色
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    
    #region Input Callbacks
    
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpInput = true;
        jumpHeld = true;
        SetJumpBuffer();
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpInput = false;
        jumpHeld = false;
    }
    
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        attackInput = true;
    }
    
    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        dashInput = true;
    }
    
    #endregion
    
    #region IDamageable Implementation
    
    public void TakeDamage(int damage)
    {
        // 如果处于无敌状态，忽略伤害
        if (isInvincible)
            return;
            
        // 应用伤害
        currentHealth -= damage;
        
        // 检查生命值
        if (currentHealth <= 0)
        {
            // 处理死亡
            Die();
        }
        else
        {
            // 进入无敌状态
            isInvincible = true;
            invincibilityCounter = invincibilityTime;
            
            // 播放受伤动画和音效
            animator?.SetTrigger("Hit");
            // AudioManager.Instance?.PlaySound("PlayerHit");
            
            // 相机震动（如果有CameraShake组件）
            if (FindObjectOfType<CameraShake>() != null)
            {
                FindObjectOfType<CameraShake>().ShakeCamera(0.2f, 0.3f);
            }
        }
    }
    
    public bool CanBeDamaged()
    {
        return !isInvincible;
    }
    
    private void Die()
    {
        // 播放死亡动画和音效
        animator?.SetTrigger("Die");
        // AudioManager.Instance?.PlaySound("PlayerDeath");
        
        // 禁用玩家输入
        playerInput.enabled = false;
        
        // 禁用物理
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        
        // 禁用碰撞体
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        
        // 通知游戏管理器
        // GameManager.Instance?.OnPlayerDeath();
    }
    
    #endregion
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
} 