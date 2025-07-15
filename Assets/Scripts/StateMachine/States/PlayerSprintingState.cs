using UnityEngine;

public class PlayerSprintingState : PlayerState
{
    private float sprintStartTime;
    private float sprintDuration;
    private Vector2 sprintDirection;
    private bool isSprintFinished;

    public PlayerSprintingState(PlayerStateMachine stateMachine, PlayerController playerController) 
        : base(stateMachine, playerController)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        sprintStartTime = Time.time;
        sprintDuration = playerController.SprintDuration;
        sprintDirection = DetermineSprintDirection();
        isSprintFinished = false;

        playerController.PerformSprint(sprintDirection);
        playerController.StartSprintCooldown();
    }

    public override void Exit()
    {
        base.Exit();
        playerController.EndSprint();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if (Time.time >= sprintStartTime + sprintDuration)
        {
            isSprintFinished = true;
        }
        
        if (isSprintFinished)
        {
            if (playerController.IsGrounded)
            {
                stateMachine.ChangeState(stateMachine.IdleState);
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
        playerController.ApplySprintVelocity(sprintDirection);
    }

    private Vector2 DetermineSprintDirection()
    {
        Vector2 moveInput = playerController.GetMoveInput();
        
        // 如果有移动输入，则朝该方向冲刺
        if (moveInput.magnitude > 0.1f)
        {
            return new Vector2(Mathf.Sign(moveInput.x), 0).normalized;
        }
        
        // 如果没有移动输入，则朝角色面朝方向冲刺
        return new Vector2(playerController.IsFacingRight ? 1 : -1, 0);
    }
} 