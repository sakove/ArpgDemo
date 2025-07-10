using UnityEngine;

/// <summary>
/// 玩家状态的基类，所有具体状态都继承自此类
/// </summary>
public abstract class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected PlayerController playerController;
    protected Rigidbody2D rb;
    protected Animator animator;
    
    protected float startTime;
    
    public PlayerState(PlayerStateMachine stateMachine, PlayerController playerController)
    {
        this.stateMachine = stateMachine;
        this.playerController = playerController;
        this.rb = playerController.GetComponent<Rigidbody2D>();
        this.animator = playerController.GetComponent<Animator>();
    }
    
    public virtual void Enter()
    {
        startTime = Time.time;
        DoChecks();
    }
    
    public virtual void Exit()
    {
        // 退出状态时的清理工作
    }
    
    public virtual void LogicUpdate()
    {
        // 处理状态逻辑
    }
    
    public virtual void PhysicsUpdate()
    {
        // 处理物理更新
        DoChecks();
    }
    
    public virtual void DoChecks()
    {
        // 进行状态检查，如是否接触地面等
    }
    
    public virtual bool CanBeInterrupted()
    {
        // 默认所有状态都可以被中断
        return true;
    }
} 