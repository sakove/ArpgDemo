using UnityEngine;

[CreateAssetMenu(fileName = "BasicAttack", menuName = "Skills/Basic Attack")]
public class BasicAttackSkill : Skill
{
    // 添加构造函数，确保基础攻击使用正确的动画触发器名称
    public BasicAttackSkill()
    {
        // 将基础攻击的动画触发器设置为"Attack"而不是默认的"Skill"
        animationTriggerName = "Attack";
        // 确保使用触发器控制类型
        animationControlType = AnimationControlType.Trigger;
    }

    [Header("攻击参数")]
    public int damage = 10;
    public float attackRange = 1.5f;
    public LayerMask targetLayers;
    public Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    public Vector2 attackBoxOffset = new Vector2(1f, 0f);
    
    [Header("连击设置")]
    public bool isPartOfCombo = false;
    public int comboIndex = 0;
    public float comboWindow = 0.5f; // 连击窗口时间
    
    [Header("击退设置")]
    public bool causesKnockback = false;
    public float knockbackForce = 5f;
    public Vector2 knockbackDirection = new Vector2(1f, 0.5f);
    
    [Header("命中停顿")]
    public bool useHitStop = true;
    public float hitStopDuration = 0.05f;
    
    public override void Activate(GameObject user)
    {
        base.Activate(user);
        
        // 获取用户朝向
        bool isFacingRight = true;
        PlayerController playerController = user.GetComponent<PlayerController>();
        if (playerController != null)
        {
            isFacingRight = playerController.IsFacingRight;
        }
        
        // 根据朝向调整攻击框位置
        Vector2 attackPosition = (Vector2)user.transform.position + 
            new Vector2(attackBoxOffset.x * (isFacingRight ? 1 : -1), attackBoxOffset.y);
        
        // 检测目标
        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            attackPosition, 
            attackBoxSize, 
            0f, 
            targetLayers
        );
        
        bool hitSomething = false;
        
        // 对每个目标应用伤害
        foreach (Collider2D target in hitTargets)
        {
            // 忽略自己
            if (target.gameObject == user)
                continue;
                
            hitSomething = true;
            
            // 应用伤害
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            
            // 应用击退
            if (causesKnockback)
            {
                Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    Vector2 direction = new Vector2(
                        knockbackDirection.x * (isFacingRight ? 1 : -1),
                        knockbackDirection.y
                    ).normalized;
                    
                    targetRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
        
        // 如果命中了目标且启用了命中停顿
        if (hitSomething && useHitStop)
        {
            // 触发命中停顿
            TimeManager.Instance?.DoHitStop(hitStopDuration);
        }
    }
    
    public override bool CanUse(GameObject user)
    {
        // 检查基本条件
        if (!base.CanUse(user))
            return false;
            
        // 如果是连击的一部分，需要检查连击条件
        if (isPartOfCombo)
        {
            CombatController combatController = user.GetComponent<CombatController>();
            if (combatController != null)
            {
                // 检查是否在连击窗口内且连击索引匹配
                return combatController.CurrentComboIndex == comboIndex - 1 && 
                       combatController.IsInComboWindow();
            }
            
            // 如果没有战斗控制器但这是连击的一部分，则不能使用
            return false;
        }
        
        // 不是连击的一部分，可以直接使用
        return true;
    }
    
    // 在Unity编辑器中绘制攻击范围的Gizmo
    private void OnDrawGizmosSelected()
    {
        // 这个函数在非运行时被调用，所以我们不能直接获取user
        // 通常在编辑器模式下，我们会假设一个位置来绘制Gizmo
        // 或者，我们需要将这个逻辑移动到持有Skill的MonoBehaviour中
        
        // 这是一个示例，假设该技能被附加到一个对象上
        // 注意：这种方式在ScriptableObject的编辑器中不会直接显示
        // 更好的方法是在使用这个技能的MonoBehaviour（例如PlayerController）中调用一个绘制Gizmo的公共方法
    }
    
    public void DrawGizmos(Transform userTransform)
    {
        if (userTransform == null) return;

        bool isFacingRight = true;
        // 在编辑器模式下，我们可能无法轻易获取PlayerController的朝向
        // 我们可以暂时假设朝向，或者在PlayerController的OnDrawGizmosSelected中调用此方法并传入朝向
        PlayerController controller = userTransform.GetComponent<PlayerController>();
        if (controller != null)
        {
            isFacingRight = controller.IsFacingRight;
        }

        Vector2 attackPosition = (Vector2)userTransform.position + 
            new Vector2(attackBoxOffset.x * (isFacingRight ? 1 : -1), attackBoxOffset.y);
            
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawCube(attackPosition, attackBoxSize);
    }
} 