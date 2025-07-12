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
    [Tooltip("动画控制方式")]
    public AnimationControlType animationControlType = AnimationControlType.Trigger;
    
    [Tooltip("技能动画片段，用于动画覆盖方式")]
    public AnimationClip skillAnimation;
    
    [Tooltip("触发器名称，用于触发器控制方式")]
    public string animationTriggerName = "Skill";
    
    [Tooltip("技能ID，用于参数控制方式")]
    public int skillID = 1;
    
    [Header("音效设置")]
    public AudioClip skillSound;
    public float volume = 1f;
    
    [Header("视觉效果")]
    public GameObject visualEffect;
    public Vector3 effectOffset = Vector3.zero;
    
    /// <summary>
    /// 动画控制类型枚举
    /// </summary>
    public enum AnimationControlType
    {
        /// <summary>使用触发器激活动画</summary>
        Trigger,
        /// <summary>使用整数参数激活动画</summary>
        Parameter,
        /// <summary>直接覆盖当前动画片段</summary>
        AnimationOverride
    }
    
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
        if (animator != null)
        {
            PlayAnimation(animator);
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
    /// 根据配置的动画控制类型播放相应动画
    /// </summary>
    /// <param name="animator">动画控制器</param>
    protected virtual void PlayAnimation(Animator animator)
    {
        if (animator == null) return;
        
        switch (animationControlType)
        {
            case AnimationControlType.Trigger:
                if (!string.IsNullOrEmpty(animationTriggerName))
                {
                    animator.SetTrigger(animationTriggerName);
                }
                break;
                
            case AnimationControlType.Parameter:
                // 设置技能ID参数
                animator.SetInteger("SkillID", skillID);
                // 触发通用技能触发器
                animator.SetTrigger("UseSkill");
                break;
                
            case AnimationControlType.AnimationOverride:
                if (skillAnimation != null)
                {
                    // 尝试获取动画覆盖控制器
                    AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
                    if (overrideController == null)
                    {
                        // 如果没有覆盖控制器，创建一个
                        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                        animator.runtimeAnimatorController = overrideController;
                    }
                    
                    // 覆盖"Skill"动画片段
                    overrideController["Skill"] = skillAnimation;
                    // 触发通用技能触发器
                    animator.SetTrigger("UseSkill");
                }
                break;
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
        
        // 重置触发器以确保动画状态机正确工作
        Animator animator = user.GetComponent<Animator>();
        if (animator != null)
        {
            switch (animationControlType)
            {
                case AnimationControlType.Trigger:
                    if (!string.IsNullOrEmpty(animationTriggerName))
                    {
                        animator.ResetTrigger(animationTriggerName);
                    }
                    break;
                    
                case AnimationControlType.Parameter:
                    // 重置通用技能触发器
                    animator.ResetTrigger("UseSkill");
                    break;
                    
                case AnimationControlType.AnimationOverride:
                    // 重置通用技能触发器
                    animator.ResetTrigger("UseSkill");
                    break;
            }
        }
    }
} 