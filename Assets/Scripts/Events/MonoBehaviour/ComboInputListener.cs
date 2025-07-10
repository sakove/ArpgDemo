using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 连招系统输入监听器
/// 展示了优先级和条件判断在2D动作游戏中的应用
/// </summary>
public class ComboInputListener : InputEventListener
{
    [Header("连招设置")]
    [SerializeField] private float _comboWindowTime = 0.5f;
    [SerializeField] private int _maxComboCount = 3;
    [SerializeField] private List<string> _comboAnimations;
    
    [Header("连招效果")]
    [SerializeField] private float[] _comboDamage = new float[] { 10f, 15f, 25f };
    [SerializeField] private float[] _comboRadius = new float[] { 1f, 1.2f, 1.5f };
    
    // 内部状态
    private int _currentCombo = 0;
    private bool _inComboWindow = false;
    private Coroutine _comboWindowCoroutine;
    
    private Animator _animator;
    private PlayerAttack _playerAttack;
    
    private void Awake()
    {
        base.Awake();
        _animator = GetComponentInParent<Animator>();
        _playerAttack = GetComponentInParent<PlayerAttack>();
        
        // 确保有足够的动画名称和伤害值
        while (_comboAnimations.Count < _maxComboCount)
            _comboAnimations.Add("Attack" + (_comboAnimations.Count + 1));
            
        while (_comboDamage.Length < _maxComboCount)
            _comboDamage = new float[] { 10f, 15f, 25f };
    }
    
    protected override bool CanProcessEvent(string inputValue)
    {
        // 首先检查基础条件
        if (!base.CanProcessEvent(inputValue))
            return false;
            
        // 连招特定条件
        if (_currentCombo >= _maxComboCount)
            return false;
            
        // 第一段攻击没有特殊条件
        if (_currentCombo == 0)
            return true;
            
        // 后续攻击需要在连招窗口期内
        return _inComboWindow;
    }
    
    public override void OnEventRaised(string inputValue)
    {
        // 如果条件不满足，则不响应事件
        if (_useCondition && !CanProcessEvent(inputValue))
            return;
        
        // 执行攻击
        PerformComboAttack();
        
        // 不调用基类方法，因为我们有自己的处理逻辑
    }
    
    private void PerformComboAttack()
    {
        // 停止之前的连招窗口计时器
        if (_comboWindowCoroutine != null)
            StopCoroutine(_comboWindowCoroutine);
            
        // 播放对应的攻击动画
        if (_animator != null && _currentCombo < _comboAnimations.Count)
            _animator.Play(_comboAnimations[_currentCombo]);
            
        // 执行攻击逻辑
        if (_playerAttack != null && _currentCombo < _comboDamage.Length)
            _playerAttack.Attack(_comboDamage[_currentCombo], _comboRadius[_currentCombo]);
            
        // 增加连击计数
        _currentCombo++;
        
        // 如果还没到最大连击数，开启连招窗口
        if (_currentCombo < _maxComboCount)
            _comboWindowCoroutine = StartCoroutine(ComboWindowRoutine());
        else
            StartCoroutine(ResetComboAfterDelay());
    }
    
    private IEnumerator ComboWindowRoutine()
    {
        // 开启连招窗口
        _inComboWindow = true;
        
        // 等待连招窗口时间
        yield return new WaitForSeconds(_comboWindowTime);
        
        // 关闭连招窗口并重置连击
        _inComboWindow = false;
        ResetCombo();
    }
    
    private IEnumerator ResetComboAfterDelay()
    {
        // 等待最后一击动画播放完毕
        yield return new WaitForSeconds(0.8f);
        
        // 重置连击
        ResetCombo();
    }
    
    public void ResetCombo()
    {
        _currentCombo = 0;
        _inComboWindow = false;
        if (_comboWindowCoroutine != null)
        {
            StopCoroutine(_comboWindowCoroutine);
            _comboWindowCoroutine = null;
        }
    }
    
    // 在玩家受伤或其他状态变化时调用此方法
    public void InterruptCombo()
    {
        ResetCombo();
    }
} 