using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // 攻击方法，接收伤害值和攻击范围参数
    public void Attack(float damage, float radius)
    {
        // 打印测试信息
        Debug.Log($"发动攻击! 伤害: {damage}, 范围: {radius}");
    }
    
    public void TestAttack(float damage)
    {
        // 打印测试信息
        Debug.Log($"发动攻击! 伤害: {damage}");
    }
    public void TestComb(float damage)
    {
        // 打印测试信息
        Debug.Log($"发动攻击! 伤害: {damage}");
    }
}