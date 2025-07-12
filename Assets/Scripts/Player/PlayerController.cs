using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 玩家控制器，负责处理玩家的基本移动和输入。
/// 该类既可以作为直接的输入处理器，也可以作为事件监听者，
/// 具体行为由 `useEventBasedInput` 字段决定。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("移动设置")]
    [Tooltip("玩家的正常移动速度")]
    [SerializeField] private float moveSpeed = 8f;
    [Tooltip("玩家跳跃时施加的初始力量")]
    [SerializeField] private float jumpForce = 16f;
    [Tooltip("玩家下落时应用的重力倍率，用于实现更快的下落感")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [Tooltip("玩家提前松开跳跃键时应用的重力倍率，用于实现小跳")]
    [SerializeField] private float lowJumpMultiplier = 2f;
    [Tooltip("土狼时间：允许玩家在离开平台后的一小段时间内仍然可以跳跃")]
    [SerializeField] private float coyoteTime = 0.1f;
    [Tooltip("跳跃缓冲时间：允许玩家在落地前的一小段时间内输入跳跃，落地后会自动执行")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    [Tooltip("空中控制系数：玩家在空中时，移动输入的控制能力（0为无控制，1为完全控制）")]
    [SerializeField] private float airControlFactor = 0.8f;
    
    [Header("地面检测")]
    [Tooltip("用于检测地面的参考点")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("地面检测的半径")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [Tooltip("定义哪些层被视作地面")]
    [SerializeField] private LayerMask groundLayer;
    
    [Header("冲刺设置")]
    [Tooltip("冲刺时的移动速度")]
    [SerializeField] private float sprintSpeed = 20f;
    [Tooltip("单次冲刺的持续时间（秒）")]
    [SerializeField] private float sprintDuration = 0.15f;
    [Tooltip("冲刺技能的冷却时间（秒）")]
    [SerializeField] private float sprintCooldown = 1f;
    
    [Header("生命值设置")]
    [Tooltip("玩家的最大生命值")]
    [SerializeField] private int maxHealth = 100;
    [Tooltip("玩家受伤后的无敌时间（秒）")]
    [SerializeField] private float invincibilityTime = 1f;
    
    // --- 组件引用 ---
    private Rigidbody2D rb;                     // 物理刚体组件
    private Animator animator;                  // 动画控制器组件
    private PlayerInput playerInput;            // Unity输入系统组件
    private PlayerStateMachine stateMachine;    // 玩家状态机
    private CombatController combatController;  // 战斗控制器
    
    // --- 移动状态变量 ---
    private Vector2 moveInput;                  // 当前的移动输入向量
    private bool isGrounded;                    // 玩家当前是否在地面上
    private bool isFacingRight = true;          // 玩家当前是否朝向右边
    private float coyoteTimeCounter;            // 土狼时间的计时器
    private float jumpBufferCounter;            // 跳跃缓冲的计时器
    private bool isSprinting;                   // 玩家当前是否正在冲刺
    private float sprintCooldownCounter;        // 冲刺冷却时间的计时器
    
    // --- 健康状态变量 ---
    private int currentHealth;                  // 当前生命值
    private bool isInvincible;                  // 当前是否处于无敌状态
    private float invincibilityCounter;         // 无敌状态的计时器
    
    // --- 输入状态标志 ---
    private bool jumpInput;                     // 跳跃键是否被按下
    private bool jumpHeld;                      // 跳跃键是否被按住
    private bool attackInput;                   // 攻击键是否被按下
    private bool sprintInput;                   // 冲刺键是否被按下
    
    // --- 输入系统选择 ---
    [Header("输入系统")]
    [Tooltip("如果为true，将使用事件驱动的输入系统。需要确保InputManager在场景中。")]
    [SerializeField] private bool useEventBasedInput = true;
    
    // --- 公开属性（供状态机等外部脚本访问） ---
    
    /// <summary> 玩家当前是否可以转向 </summary>
    public bool CanFlip { get; set; } = true;

    /// <summary> 玩家当前是否在地面上 </summary>
    public bool IsGrounded => isGrounded;
    /// <summary> 土狼时间的剩余时间 </summary>
    public float CoyoteTimeCounter => coyoteTimeCounter;
    /// <summary> 跳跃键是否被按下 </summary>
    public bool JumpInput => jumpInput;
    /// <summary> 跳跃键是否被持续按住 </summary>
    public bool JumpHeld => jumpHeld;
    /// <summary> 攻击键是否被按下 </summary>
    public bool AttackInput => attackInput;
    /// <summary> 冲刺键是否被按下 </summary>
    public bool SprintInput => sprintInput;
    /// <summary> 当前是否可以冲刺（冷却时间是否结束） </summary>
    public bool CanSprint => sprintCooldownCounter <= 0f;
    /// <summary> 冲刺的持续时间 </summary>
    public float SprintDuration => sprintDuration;
    /// <summary> 玩家当前是否朝向右边 </summary>
    public bool IsFacingRight => isFacingRight;
    /// <summary> 空中控制能力系数 </summary>
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
        // 根据选择的输入系统订阅事件
        if (useEventBasedInput)
        {
            SubscribeToInputEvents();
        }
    }
    
    private void OnDisable()
    {
        // 根据选择的输入系统取消订阅事件
        if (useEventBasedInput)
        {
            UnsubscribeFromInputEvents();
        }
    }
    
    private void SubscribeToInputEvents()
    {
        // 自动查找挂载在同一对象上的所有InputEventListener
        var listeners = GetComponents<InputEventListener>();
        foreach (var listener in listeners)
        {
            if (listener.eventSO == null) continue;

            // 尝试将事件SO转换为InputEventSO以访问其类型
            if (listener.eventSO is InputEventSO inputEvent)
            {
                // 根据事件类型订阅不同的回调
                switch (inputEvent.inputType)
                {
                    case InputEventType.Move:
                        listener.AddValueChangedListener(OnMoveEventReceived);
                        break;
                    case InputEventType.Jump:
                        listener.AddPressedListener(OnJumpEventPressed);
                        listener.AddReleasedListener(OnJumpEventReleased);
                        break;
                    case InputEventType.Sprint:
                        listener.AddPressedListener(OnSprintEventPressed);
                        listener.AddReleasedListener(OnSprintEventReleased);
                        break;
                    case InputEventType.Attack:
                        listener.AddPressedListener(OnAttackEventPressed);
                        break;
                    // 其他类型的输入可以在这里扩展
                }
            }
        }
    }
    
    private void UnsubscribeFromInputEvents()
    {
        // UnityEvent在对象销毁时会自动处理大部分反注册逻辑，
        // 但如果需要手动、精确地移除，可以扩展此方法。
        // 为保持简单，暂时留空。
    }
    
    // 基于事件的输入处理方法
    private void OnMoveEventReceived(InputEventData data)
    {
        moveInput = data.Vector;
    }
    
    private void OnJumpEventPressed(InputEventData data)
    {
        jumpInput = true;
        jumpHeld = true;
        SetJumpBuffer();
    }
    
    private void OnJumpEventReleased(InputEventData data)
    {
        jumpInput = false;
        jumpHeld = false;
    }
    
    private void OnAttackEventPressed(InputEventData data)
    {
        attackInput = true;
    }
    
    private void OnSprintEventPressed(InputEventData data)
    {
        sprintInput = true;
    }
    
    private void OnSprintEventReleased(InputEventData data)
    {
        sprintInput = false;
    }
    
    private void Update()
    {
        // 使用事件系统时，moveInput已经通过事件更新，无需在此读取
        
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
        if (sprintCooldownCounter > 0)
        {
            sprintCooldownCounter -= Time.deltaTime;
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
        sprintInput = false;
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
    public void PerformSprint(Vector2 direction)
    {
        if (isSprinting) return;

        isSprinting = true;
        rb.linearVelocity = direction.normalized * sprintSpeed;
        
        // 可以在这里添加冲刺开始的音效或特效
        // AudioManager.Instance?.PlaySound("Sprint");
        // EffectsManager.Instance?.PlayEffect("SprintTrail", transform.position);
    }
    
    public void EndSprint()
    {
        isSprinting = false;
        // 可以在这里添加冲刺结束的逻辑
    }
    
    public void ApplySprintVelocity(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * sprintSpeed;
    }

    /// <summary>
    /// 开始冲刺技能冷却
    /// </summary>
    public void StartSprintCooldown()
    {
        sprintCooldownCounter = sprintCooldown;
    }
    
    /// <summary>
    /// 应用下落加速度，并处理可变跳跃高度的逻辑
    /// </summary>
    public void ApplyFallMultiplier()
    {
        // 核心改动：优化跳跃手感，消除最高点附近的“漂浮感”。
        // 只要角色速度向量向下（即正在下落），就施加一个额外的重力倍率。
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        // 如果角色正在上升，并且玩家已经松开了跳跃键，则施加另一个重力倍率。
        // 这会让角色更快地停止上升，从而实现“小跳”。
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
        if (!CanFlip) return;

        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    
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