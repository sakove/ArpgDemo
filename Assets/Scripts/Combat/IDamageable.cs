using UnityEngine;

/// <summary>
/// 可受伤害的接口，实现此接口的对象可以受到伤害
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    void TakeDamage(int damage);
    
    /// <summary>
    /// 检查是否可以受到伤害
    /// </summary>
    /// <returns>如果可以受到伤害，则返回true</returns>
    bool CanBeDamaged();
} 