using UnityEngine;

[CreateAssetMenu(fileName = "Fireball", menuName = "Skills/Fireball")]
public class FireballSkill : Skill
{
    [Header("火球参数")]
    public int damage = 20;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 5f;
    public GameObject fireballPrefab;
    public float castDistance = 1f;
    
    public FireballSkill()
    {
        // 使用参数控制类型
        animationControlType = AnimationControlType.Parameter;
        // 设置技能ID为1（在Animator中对应火球技能）
        skillID = 1;
    }
    
    public override void Activate(GameObject user)
    {
        // 调用基类的Activate方法来处理动画、音效等
        base.Activate(user);
        
        // 获取玩家朝向
        bool isFacingRight = true;
        PlayerController playerController = user.GetComponent<PlayerController>();
        if (playerController != null)
        {
            isFacingRight = playerController.IsFacingRight;
        }
        
        // 计算生成位置
        Vector2 spawnPosition = (Vector2)user.transform.position + 
            new Vector2(castDistance * (isFacingRight ? 1 : -1), 0.5f);
        
        // 生成火球
        if (fireballPrefab != null)
        {
            GameObject fireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
            
            // 设置火球属性
            Projectile projectileComponent = fireball.GetComponent<Projectile>();
            if (projectileComponent != null)
            {
                projectileComponent.damage = damage;
                projectileComponent.speed = projectileSpeed;
                projectileComponent.lifetime = projectileLifetime;
                projectileComponent.direction = new Vector2(isFacingRight ? 1 : -1, 0);
            }
        }
    }
} 