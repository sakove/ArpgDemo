using UnityEngine;

/// <summary>
/// 动画状态辅助工具，用于在不同动画层之间共享状态信息。
/// 它同时维护详细的四向移动状态和简化的“地面/空中”状态，以提供最大的灵活性和扩展性。
/// </summary>
public class AnimationStateHelper : MonoBehaviour
{
    // 详细的移动状态枚举
    public enum MovementState
    {
        Idle,
        Moving,
        Jumping,
        Falling
    }

    // 为Animator简化的宏观状态枚举
    public enum LocomotionState
    {
        Ground, // 包含了 Idle 和 Moving
        Air     // 包含了 Jumping 和 Falling
    }
    
    [Header("组件引用")]
    [SerializeField] private Animator animator;
    
    [Header("参数名称")]
    [Tooltip("传递给Animator的宏观状态参数名 (Ground/Air)")]
    [SerializeField] private string locomotionStateParameter = "LocomotionState";
    
    // 内部组件引用
    private PlayerController playerController;
    private Rigidbody2D rb;
    
    // 当前状态
    private MovementState currentMovementState = MovementState.Idle;
    private LocomotionState currentLocomotionState = LocomotionState.Ground;
    
    private void Awake()
    {
        // 自动获取组件
        if (animator == null) animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        // 核心：在每一帧更新所有状态
        UpdateStates();
        
        // 将简化的“地面/空中”状态传递给Animator
        if (animator != null)
        {
            animator.SetInteger(locomotionStateParameter, (int)currentLocomotionState);
        }
    }
    
    /// <summary>
    /// 根据角色的物理状态，一次性更新详细和宏观两种状态
    /// </summary>
    private void UpdateStates()
    {
        if (playerController == null || rb == null) return;
        
        // 首先判断宏观状态：在地面还是空中
        if (playerController.IsGrounded)
        {
            currentLocomotionState = LocomotionState.Ground;
            
            // 在地面的基础上，判断详细状态：静止还是移动
            // 注意：这里我们使用playerController的HorizontalInput，因为它直接反映玩家意图
            if (Mathf.Abs(playerController.HorizontalInput) > 0.1f)
            {
                currentMovementState = MovementState.Moving;
            }
            else
            {
                currentMovementState = MovementState.Idle;
            }
        }
        else
        {
            currentLocomotionState = LocomotionState.Air;

            // 在空中的基础上，判断详细状态：上升还是下落
            // 注意：这里我们使用刚体的垂直速度，因为它反映了真实的物理运动
            if (rb.linearVelocity.y > 0.1f)
            {
                currentMovementState = MovementState.Jumping;
            }
            else
            {
                currentMovementState = MovementState.Falling;
            }
        }
    }
    
    /// <summary>
    /// 获取详细的移动状态 (Idle, Moving, Jumping, Falling)
    /// </summary>
    public MovementState GetCurrentMovementState()
    {
        return currentMovementState;
    }

    /// <summary>
    /// 获取为Animator简化的宏观状态 (Ground, Air)
    /// </summary>
    public LocomotionState GetCurrentLocomotionState()
    {
        return currentLocomotionState;
    }
} 