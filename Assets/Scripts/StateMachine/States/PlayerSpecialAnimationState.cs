using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家特殊动画状态，用于处理面向摄像机的特殊动画，如睡觉、重生等
/// </summary>
public class PlayerSpecialAnimationState : PlayerState
{
    private float animationStartTime;
    private float animationDuration;
    private bool animationFinished;
    private string animationTrigger;
    private bool lockInput;
    
    public PlayerSpecialAnimationState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // 如果没有设置动画触发器，使用默认的"SpecialAction"
        if (string.IsNullOrEmpty(animationTrigger))
        {
            animationTrigger = "SpecialAction";
        }
        
        // 设置动画开始时间
        animationStartTime = Time.time;
        
        // 重置动画完成标志
        animationFinished = false;
        
        // 禁止玩家转向
        playerController.CanFlip = false;
        
        // 触发特殊动画
        playerController.TriggerSpecialAnimation(animationTrigger);
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // 结束特殊动画
        playerController.EndSpecialAnimation();
        
        // 恢复玩家的转向能力
        playerController.CanFlip = true;
    }
    
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 检查动画是否播放完毕
        if (Time.time >= animationStartTime + animationDuration)
        {
            animationFinished = true;
        }
        
        // 如果动画已完成，切换到适当的状态
        if (animationFinished)
        {
            if (playerController.IsGrounded)
            {
                // 根据是否有移动输入决定切换到闲置还是移动状态
                Vector2 moveInput = playerController.GetMoveInput();
                if (Mathf.Abs(moveInput.x) > 0.1f)
                {
                    stateMachine.ChangeState(stateMachine.MovingState);
                }
                else
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                }
            }
            else
            {
                stateMachine.ChangeState(stateMachine.FallingState);
            }
        }
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // 特殊动画状态下不应用物理移动
    }
    
    public override bool CanBeInterrupted()
    {
        // 特殊动画状态通常不能被中断
        return false;
    }
    
    /// <summary>
    /// 设置特殊动画参数
    /// </summary>
    /// <param name="trigger">动画触发器名称</param>
    /// <param name="duration">动画持续时间</param>
    /// <param name="lockInput">是否锁定输入</param>
    public void SetAnimationParameters(string trigger, float duration, bool lockInput = true)
    {
        animationTrigger = trigger;
        animationDuration = duration;
        this.lockInput = lockInput;
    }
} 