using System.Collections;
using UnityEngine;

/// <summary>
/// 时间管理器，用于控制游戏时间，实现命中停顿等效果
/// </summary>
public class TimeManager : MonoBehaviour
{
    // 单例实例
    public static TimeManager Instance { get; private set; }
    
    [Header("命中停顿设置")]
    [SerializeField] private float defaultHitStopDuration = 0.05f;
    [SerializeField] private float hitStopTimeScale = 0.1f;
    
    [Header("时间缩放设置")]
    [SerializeField] private float defaultTimeScaleDuration = 0.5f;
    
    // 状态
    private bool isHitStopping = false;
    private bool isTimeScaling = false;
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // 确保时间恢复正常
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// 执行命中停顿效果
    /// </summary>
    /// <param name="duration">停顿持续时间，如果为0则使用默认值</param>
    public void DoHitStop(float duration = 0f)
    {
        // 如果已经在执行命中停顿，不重复执行
        if (isHitStopping)
            return;
            
        // 使用默认值或指定值
        float hitStopDuration = duration > 0f ? duration : defaultHitStopDuration;
        
        // 开始命中停顿协程
        StartCoroutine(HitStopCoroutine(hitStopDuration));
    }
    
    /// <summary>
    /// 设置时间缩放
    /// </summary>
    /// <param name="timeScale">时间缩放值</param>
    /// <param name="duration">持续时间，如果为0则使用默认值</param>
    public void SetTimeScale(float timeScale, float duration = 0f)
    {
        // 如果已经在执行时间缩放，停止之前的协程
        if (isTimeScaling)
        {
            StopAllCoroutines();
            isTimeScaling = false;
        }
        
        // 使用默认值或指定值
        float scaleDuration = duration > 0f ? duration : defaultTimeScaleDuration;
        
        // 开始时间缩放协程
        StartCoroutine(TimeScaleCoroutine(timeScale, scaleDuration));
    }
    
    /// <summary>
    /// 命中停顿协程
    /// </summary>
    private IEnumerator HitStopCoroutine(float duration)
    {
        // 设置状态
        isHitStopping = true;
        
        // 保存原始时间缩放
        float originalTimeScale = Time.timeScale;
        
        // 设置时间缩放为命中停顿值
        Time.timeScale = hitStopTimeScale;
        
        // 等待指定时间（使用非缩放时间）
        yield return new WaitForSecondsRealtime(duration);
        
        // 恢复原始时间缩放
        Time.timeScale = originalTimeScale;
        
        // 重置状态
        isHitStopping = false;
    }
    
    /// <summary>
    /// 时间缩放协程
    /// </summary>
    private IEnumerator TimeScaleCoroutine(float targetScale, float duration)
    {
        // 设置状态
        isTimeScaling = true;
        
        // 设置时间缩放
        Time.timeScale = targetScale;
        
        // 等待指定时间（使用非缩放时间）
        yield return new WaitForSecondsRealtime(duration);
        
        // 恢复正常时间缩放
        Time.timeScale = 1f;
        
        // 重置状态
        isTimeScaling = false;
    }
} 