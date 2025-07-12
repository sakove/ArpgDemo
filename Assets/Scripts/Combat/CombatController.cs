using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 战斗控制器，负责处理玩家的战斗相关逻辑
/// </summary>
public class CombatController : MonoBehaviour
{
    [Header("基础攻击设置")]
    [SerializeField] private Skill basicAttack;
    
    [Header("技能槽")]
    [SerializeField] private Skill[] equippedSkills = new Skill[3];
    
    [Header("连击设置")]
    [SerializeField] private float comboResetTime = 1.5f;
    [SerializeField] private float comboWindowTime = 0.5f;
    
    [Header("攻击移动设置")]
    [SerializeField] private bool canMoveWhileAttacking = false;
    [SerializeField] private float attackMovementFactor = 0.3f;
    
    // 输入引用
    private PlayerInput playerInput;
    private InputAction skill1Action;
    private InputAction skill2Action;
    private InputAction skill3Action;
    
    // 连击状态
    private int currentComboIndex = 0;
    private float lastAttackTime;
    public bool isNextComboReady = false;
    private bool isInComboWindow = false;
    
    // 冷却状态
    private float[] skillCooldowns = new float[3];
    
    // 组件引用
    private PlayerController playerController;
    private Animator animator;
    
    // 当前使用的技能，用于跟踪并在适当的时候调用OnSkillEnd
    private Skill currentActiveSkill;
    private float currentSkillEndTime;
    
    // 属性
    public int CurrentComboIndex => currentComboIndex;
    public float AttackMovementFactor => attackMovementFactor;
    
    private void Awake()
    {
        // 获取组件
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        
        // 设置输入动作
        if (playerInput != null)
        {
            skill1Action = playerInput.actions["Skill1"];
            skill2Action = playerInput.actions["Skill2"];
            skill3Action = playerInput.actions["Skill3"];
        }
    }
    
    private void OnEnable()
    {
        // 订阅输入事件
        if (skill1Action != null) skill1Action.performed += OnSkill1;
        if (skill2Action != null) skill2Action.performed += OnSkill2;
        if (skill3Action != null) skill3Action.performed += OnSkill3;
    }
    
    private void OnDisable()
    {
        // 取消订阅输入事件
        if (skill1Action != null) skill1Action.performed -= OnSkill1;
        if (skill2Action != null) skill2Action.performed -= OnSkill2;
        if (skill3Action != null) skill3Action.performed -= OnSkill3;
    }
    
    private void Update()
    {
        // 更新技能冷却
        for (int i = 0; i < skillCooldowns.Length; i++)
        {
            if (skillCooldowns[i] > 0)
            {
                skillCooldowns[i] -= Time.deltaTime;
            }
        }
        
        // 检查连击重置
        if (Time.time > lastAttackTime + comboResetTime)
        {
            ResetCombo();
        }
        
        // 检查连击窗口
        isInComboWindow = Time.time <= lastAttackTime + comboWindowTime;
        
        // 检查当前技能是否结束
        if (currentActiveSkill != null && Time.time >= currentSkillEndTime)
        {
            // 调用技能结束方法以清理
            currentActiveSkill.OnSkillEnd(gameObject);
            currentActiveSkill = null;
        }
    }
    
    /// <summary>
    /// 执行基础攻击
    /// </summary>
    public void PerformAttack()
    {
        if (basicAttack != null)
        {
            // 如果有活动技能，先结束它
            if (currentActiveSkill != null)
            {
                currentActiveSkill.OnSkillEnd(gameObject);
            }
            
            // 激活基础攻击技能
            basicAttack.Activate(gameObject);
            
            // 设置当前活动技能和结束时间
            currentActiveSkill = basicAttack;
            currentSkillEndTime = Time.time + basicAttack.GetDuration();
            
            // 更新连击状态
            lastAttackTime = Time.time;
            isInComboWindow = true;
            
            // 如果下一个连击已经准备好，增加连击索引
            if (isNextComboReady)
            {
                currentComboIndex++;
                isNextComboReady = false;
            }
        }
    }
    
    /// <summary>
    /// 使用指定槽位的技能
    /// </summary>
    /// <param name="slotIndex">技能槽位索引</param>
    public void UseSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length)
            return;
            
        Skill skill = equippedSkills[slotIndex];
        
        // 检查技能是否存在且可用
        if (skill != null && CanUseSkill(slotIndex))
        {
            // 如果有活动技能，先结束它
            if (currentActiveSkill != null)
            {
                currentActiveSkill.OnSkillEnd(gameObject);
            }
            
            // 激活技能
            skill.Activate(gameObject);
            
            // 设置当前活动技能和结束时间
            currentActiveSkill = skill;
            currentSkillEndTime = Time.time + skill.GetDuration();
            
            // 设置冷却
            skillCooldowns[slotIndex] = skill.cooldown;
            
            // 重置连击
            ResetCombo();
        }
    }
    
    /// <summary>
    /// 检查指定槽位的技能是否可用
    /// </summary>
    /// <param name="slotIndex">技能槽位索引</param>
    /// <returns>如果技能可用，则返回true</returns>
    public bool CanUseSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length)
            return false;
            
        Skill skill = equippedSkills[slotIndex];
        
        // 检查技能是否存在且冷却完成
        if (skill != null && skillCooldowns[slotIndex] <= 0)
        {
            return skill.CanUse(gameObject);
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取当前攻击的持续时间
    /// </summary>
    /// <returns>攻击持续时间</returns>
    public float GetCurrentAttackDuration()
    {
        // 如果有基础攻击，返回其持续时间
        if (basicAttack != null)
        {
            return basicAttack.GetDuration();
        }
        
        // 默认持续时间
        return 0.5f;
    }
    
    /// <summary>
    /// 检查是否可以执行下一个连击
    /// </summary>
    /// <returns>如果可以执行下一个连击，则返回true</returns>
    public bool CanPerformNextCombo()
    {
        return isInComboWindow && !isNextComboReady;
    }
    
    /// <summary>
    /// 设置下一个连击准备就绪
    /// </summary>
    public void SetNextComboReady()
    {
        isNextComboReady = true;
    }
    
    /// <summary>
    /// 检查是否在连击窗口内
    /// </summary>
    /// <returns>如果在连击窗口内，则返回true</returns>
    public bool IsInComboWindow()
    {
        return isInComboWindow;
    }
    
    /// <summary>
    /// 重置连击状态
    /// </summary>
    public void ResetCombo()
    {
        currentComboIndex = 0;
        isNextComboReady = false;
        isInComboWindow = false;
    }
    
    /// <summary>
    /// 检查是否可以在攻击时移动
    /// </summary>
    /// <returns>如果可以在攻击时移动，则返回true</returns>
    public bool CanMoveWhileAttacking()
    {
        return canMoveWhileAttacking;
    }
    
    /// <summary>
    /// 装备技能到指定槽位
    /// </summary>
    /// <param name="skill">要装备的技能</param>
    /// <param name="slotIndex">槽位索引</param>
    public void EquipSkill(Skill skill, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedSkills.Length)
        {
            equippedSkills[slotIndex] = skill;
            skillCooldowns[slotIndex] = 0f;
        }
    }
    
    /// <summary>
    /// 技能1输入回调
    /// </summary>
    private void OnSkill1(InputAction.CallbackContext context)
    {
        UseSkill(0);
    }
    
    /// <summary>
    /// 技能2输入回调
    /// </summary>
    private void OnSkill2(InputAction.CallbackContext context)
    {
        UseSkill(1);
    }
    
    /// <summary>
    /// 技能3输入回调
    /// </summary>
    private void OnSkill3(InputAction.CallbackContext context)
    {
        UseSkill(2);
    }
} 