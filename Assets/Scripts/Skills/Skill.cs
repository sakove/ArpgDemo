using UnityEngine;

/// <summary>
/// 技能基类，所有技能都继承自此类
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Basic Skill")]
public class Skill : ScriptableObject
{
    [Header("基本信息")]
    public string skillName = "New Skill";
    public Sprite skillIcon;
    public string description = "Skill description";
    
    [Header("技能参数")]
    public float cooldown = 1f;
    public float skillDuration = 0.5f;
    public bool canMoveWhileUsing = false;
    public float movementSpeedModifier = 0.5f; // 使用技能时的移动速度修正
    
    [Header("动画设置")]
    public AnimationClip skillAnimation;
    public string animationTriggerName = "Skill";
    
    [Header("音效设置")]
    public AudioClip skillSound;
    public float volume = 1f;
    
    [Header("视觉效果")]
    public GameObject visualEffect;
    public Vector3 effectOffset = Vector3.zero;
    
    /// <summary>
    /// 激活技能
    /// </summary>
    /// <param name="user">使用技能的游戏对象</param>
    public virtual void Activate(GameObject user)
    {
        // 基本的激活逻辑，派生类可以重写此方法
        Debug.Log($"使用了技能: {skillName}");
        
        // 播放动画
        Animator animator = user.GetComponent<Animator>();
        if (animator != null && !string.IsNullOrEmpty(animationTriggerName))
        {
            animator.SetTrigger(animationTriggerName);
        }
        
        // 播放音效
        if (skillSound != null)
        {
            AudioSource.PlayClipAtPoint(skillSound, user.transform.position, volume);
        }
        
        // 生成视觉效果
        if (visualEffect != null)
        {
            Instantiate(visualEffect, user.transform.position + effectOffset, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// 获取技能的持续时间
    /// </summary>
    /// <returns>技能持续时间</returns>
    public virtual float GetDuration()
    {
        return skillDuration;
    }
    
    /// <summary>
    /// 检查技能是否可以使用
    /// </summary>
    /// <param name="user">使用技能的游戏对象</param>
    /// <returns>如果可以使用，则返回true</returns>
    public virtual bool CanUse(GameObject user)
    {
        // 默认实现，派生类可以添加额外的条件检查
        return true;
    }
    
    /// <summary>
    /// 技能结束时调用
    /// </summary>
    /// <param name="user">使用技能的游戏对象</param>
    public virtual void OnSkillEnd(GameObject user)
    {
        // 技能结束时的清理工作，派生类可以重写此方法
    }
} 