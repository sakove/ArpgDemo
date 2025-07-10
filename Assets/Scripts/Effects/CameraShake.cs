using System.Collections;
using UnityEngine;

/// <summary>
/// 相机震动效果，用于增强游戏反馈
/// </summary>
public class CameraShake : MonoBehaviour
{
    // 单例实例
    public static CameraShake Instance { get; private set; }
    
    [Header("震动设置")]
    [SerializeField] private float defaultShakeDuration = 0.2f;
    [SerializeField] private float defaultShakeIntensity = 0.3f;
    [SerializeField] private float defaultShakeFrequency = 25f;
    
    // 状态
    private bool isShaking = false;
    private Vector3 originalPosition;
    private Camera targetCamera;
    
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
            return;
        }
        
        // 获取相机组件
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }
    
    private void Start()
    {
        // 记录原始位置
        if (targetCamera != null)
        {
            originalPosition = targetCamera.transform.localPosition;
        }
    }
    
    /// <summary>
    /// 执行相机震动
    /// </summary>
    /// <param name="duration">震动持续时间，如果为0则使用默认值</param>
    /// <param name="intensity">震动强度，如果为0则使用默认值</param>
    public void ShakeCamera(float duration = 0f, float intensity = 0f)
    {
        // 如果没有相机，不执行震动
        if (targetCamera == null)
            return;
            
        // 如果已经在震动，停止之前的震动
        if (isShaking)
        {
            StopAllCoroutines();
            isShaking = false;
            targetCamera.transform.localPosition = originalPosition;
        }
        
        // 使用默认值或指定值
        float shakeDuration = duration > 0f ? duration : defaultShakeDuration;
        float shakeIntensity = intensity > 0f ? intensity : defaultShakeIntensity;
        
        // 开始震动协程
        StartCoroutine(ShakeCoroutine(shakeDuration, shakeIntensity));
    }
    
    /// <summary>
    /// 执行相机震动（高级版本）
    /// </summary>
    /// <param name="duration">震动持续时间</param>
    /// <param name="intensity">震动强度</param>
    /// <param name="frequency">震动频率</param>
    /// <param name="fadeIn">淡入时间</param>
    /// <param name="fadeOut">淡出时间</param>
    public void ShakeCameraAdvanced(float duration, float intensity, float frequency, float fadeIn = 0f, float fadeOut = 0.1f)
    {
        // 如果没有相机，不执行震动
        if (targetCamera == null)
            return;
            
        // 如果已经在震动，停止之前的震动
        if (isShaking)
        {
            StopAllCoroutines();
            isShaking = false;
            targetCamera.transform.localPosition = originalPosition;
        }
        
        // 开始高级震动协程
        StartCoroutine(ShakeAdvancedCoroutine(duration, intensity, frequency, fadeIn, fadeOut));
    }
    
    /// <summary>
    /// 震动协程
    /// </summary>
    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        // 设置状态
        isShaking = true;
        
        // 记录开始时间
        float startTime = Time.time;
        float endTime = startTime + duration;
        
        while (Time.time < endTime)
        {
            // 计算震动强度（随时间衰减）
            float elapsedTime = Time.time - startTime;
            float currentIntensity = intensity * (1f - (elapsedTime / duration));
            
            // 生成随机位置
            float offsetX = Random.Range(-1f, 1f) * currentIntensity;
            float offsetY = Random.Range(-1f, 1f) * currentIntensity;
            
            // 应用震动
            targetCamera.transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            
            // 等待下一帧
            yield return null;
        }
        
        // 恢复原始位置
        targetCamera.transform.localPosition = originalPosition;
        
        // 重置状态
        isShaking = false;
    }
    
    /// <summary>
    /// 高级震动协程
    /// </summary>
    private IEnumerator ShakeAdvancedCoroutine(float duration, float intensity, float frequency, float fadeIn, float fadeOut)
    {
        // 设置状态
        isShaking = true;
        
        // 记录开始时间
        float startTime = Time.time;
        float endTime = startTime + duration;
        
        // 初始化震动参数
        Vector3 lastShakeOffset = Vector3.zero;
        float seed = Random.Range(0f, 1000f);
        
        while (Time.time < endTime)
        {
            // 计算当前时间
            float elapsedTime = Time.time - startTime;
            float remainingTime = duration - elapsedTime;
            
            // 计算当前强度（考虑淡入淡出）
            float intensityMultiplier = 1f;
            
            // 淡入
            if (fadeIn > 0f && elapsedTime < fadeIn)
            {
                intensityMultiplier = Mathf.Lerp(0f, 1f, elapsedTime / fadeIn);
            }
            
            // 淡出
            if (fadeOut > 0f && remainingTime < fadeOut)
            {
                intensityMultiplier = Mathf.Lerp(0f, 1f, remainingTime / fadeOut);
            }
            
            // 计算当前震动强度
            float currentIntensity = intensity * intensityMultiplier;
            
            // 使用Perlin噪声生成平滑的震动
            float noiseX = Mathf.PerlinNoise(seed, elapsedTime * frequency);
            float noiseY = Mathf.PerlinNoise(seed + 1000f, elapsedTime * frequency);
            
            // 将噪声值映射到[-1, 1]范围
            float offsetX = (noiseX * 2f - 1f) * currentIntensity;
            float offsetY = (noiseY * 2f - 1f) * currentIntensity;
            
            // 应用震动，使用插值使移动更平滑
            Vector3 targetOffset = new Vector3(offsetX, offsetY, 0f);
            lastShakeOffset = Vector3.Lerp(lastShakeOffset, targetOffset, Time.deltaTime * frequency);
            targetCamera.transform.localPosition = originalPosition + lastShakeOffset;
            
            // 等待下一帧
            yield return null;
        }
        
        // 恢复原始位置
        targetCamera.transform.localPosition = originalPosition;
        
        // 重置状态
        isShaking = false;
    }
} 