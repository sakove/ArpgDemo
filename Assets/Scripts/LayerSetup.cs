using UnityEngine;

// 这个脚本用于在编辑器中设置必要的层级
// 注意：这个脚本应该在游戏开始前执行一次
public class LayerSetup : MonoBehaviour
{
    private void Awake()
    {
        // 确保必要的层级存在
        CreateLayerIfNeeded("Ground", 8);
        CreateLayerIfNeeded("Player", 9);
        CreateLayerIfNeeded("Enemy", 10);
        
        // 设置层级碰撞矩阵
        SetupLayerCollisionMatrix();
        
        Debug.Log("层级设置完成");
        
        // 完成后自动销毁
        Destroy(this.gameObject);
    }
    
    // 创建层级（如果不存在）
    private void CreateLayerIfNeeded(string layerName, int layerIndex)
    {
        // 检查层级是否已存在
        if (LayerMask.NameToLayer(layerName) == -1)
        {
            Debug.LogWarning($"需要手动创建层级: {layerName}，建议索引: {layerIndex}");
            Debug.LogWarning("请在Unity编辑器中: Edit > Project Settings > Tags and Layers 中添加所需层级");
        }
        else
        {
            Debug.Log($"层级 {layerName} 已存在");
        }
    }
    
    // 设置层级碰撞矩阵
    private void SetupLayerCollisionMatrix()
    {
        // 注意：无法在运行时设置层级碰撞矩阵，这只是一个提示
        Debug.LogWarning("请在Unity编辑器中: Edit > Project Settings > Physics 2D > Layer Collision Matrix 中设置层级碰撞");
        Debug.LogWarning("建议设置:");
        Debug.LogWarning("1. Player 可以与 Ground 和 Enemy 碰撞");
        Debug.LogWarning("2. Enemy 可以与 Ground 和 Player 碰撞");
    }
} 