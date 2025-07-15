using UnityEngine;

/// <summary>
/// 睡觉点示例，演示如何使用特殊动画状态
/// </summary>
public class SleepingSpot : MonoBehaviour
{
    [Tooltip("睡觉动画的持续时间")]
    [SerializeField] private float sleepDuration = 3f;
    
    [Tooltip("是否在睡觉时恢复生命值")]
    [SerializeField] private bool healWhileSleeping = true;
    
    [Tooltip("每秒恢复的生命值")]
    [SerializeField] private int healAmountPerSecond = 5;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家
        if (other.CompareTag("Player"))
        {
            // 获取玩家状态机
            PlayerStateMachine stateMachine = other.GetComponent<PlayerStateMachine>();
            if (stateMachine != null)
            {
                // 切换到睡觉状态
                stateMachine.ChangeToSpecialAnimationState("Sleep", sleepDuration);
                
                // 如果需要恢复生命值，启动协程
                if (healWhileSleeping)
                {
                    StartCoroutine(HealPlayer(other.gameObject, sleepDuration));
                }
            }
        }
    }
    
    private System.Collections.IEnumerator HealPlayer(GameObject player, float duration)
    {
        // 获取玩家的IDamageable接口
        IDamageable damageable = player.GetComponent<IDamageable>();
        if (damageable == null)
        {
            yield break;
        }
        
        float timer = 0f;
        while (timer < duration)
        {
            // 每秒恢复生命值
            damageable.Heal(healAmountPerSecond * Time.deltaTime);
            
            timer += Time.deltaTime;
            yield return null;
        }
    }
} 