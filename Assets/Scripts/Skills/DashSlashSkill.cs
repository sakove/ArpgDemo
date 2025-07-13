using UnityEngine;

[CreateAssetMenu(fileName = "DashSlash", menuName = "Skills/DashSlash")]
public class DashSlashSkill : Skill
{
    [Header("冲斩参数")]
    public int damage = 30;
    public float dashDistance = 5f;
    public float dashSpeed = 20f;
    public float attackWidth = 2f;
    public float attackHeight = 1.5f;
    
    public DashSlashSkill()
    {
        // 注意：现在我们使用动画覆盖方式，需要在Inspector中设置skillAnimation
    }
    
    public override void Activate(GameObject user)
    {
        // 调用基类的Activate方法来处理动画、音效等
        base.Activate(user);
        
        // 获取玩家控制器和刚体
        PlayerController playerController = user.GetComponent<PlayerController>();
        Rigidbody2D rb = user.GetComponent<Rigidbody2D>();
        
        if (playerController != null && rb != null)
        {
            // 获取朝向
            bool isFacingRight = playerController.IsFacingRight;
            
            // 执行冲刺
            Vector2 dashDirection = new Vector2(isFacingRight ? 1 : -1, 0);
            rb.linearVelocity = dashDirection * dashSpeed;
            
            // 启动协程来处理冲斩逻辑
            MonoBehaviour monoBehaviour = user.GetComponent<MonoBehaviour>();
            if (monoBehaviour != null)
            {
                monoBehaviour.StartCoroutine(DashSlashRoutine(user, dashDirection));
            }
        }
    }
    
    private System.Collections.IEnumerator DashSlashRoutine(GameObject user, Vector2 direction)
    {
        // 等待一小段时间让动画开始播放
        yield return new WaitForSeconds(0.1f);
        
        // 获取玩家位置
        Vector2 userPosition = user.transform.position;
        
        // 计算攻击区域
        Vector2 attackCenter = userPosition + direction * 1f;
        
        // 检测攻击范围内的敌人
        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            attackCenter,
            new Vector2(attackWidth, attackHeight),
            0f,
            LayerMask.GetMask("Enemy")
        );
        
        // 对每个命中的敌人造成伤害
        foreach (Collider2D target in hitTargets)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            
            // 可以添加击退效果
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                targetRb.AddForce(direction * 10f, ForceMode2D.Impulse);
            }
        }
        
        // 等待技能持续时间结束
        yield return new WaitForSeconds(skillDuration - 0.1f);
        
        // 停止冲刺
        Rigidbody2D userRb = user.GetComponent<Rigidbody2D>();
        if (userRb != null)
        {
            userRb.linearVelocity = Vector2.zero;
        }
    }
} 