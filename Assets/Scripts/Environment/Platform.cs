using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Platform : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    
    private void OnDrawGizmos()
    {
        // 获取BoxCollider2D组件
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        
        if (boxCollider != null)
        {
            // 设置Gizmo颜色
            Gizmos.color = gizmoColor;
            
            // 绘制平台的边界框
            Vector3 center = transform.TransformPoint(boxCollider.offset);
            Vector3 size = new Vector3(
                boxCollider.size.x * transform.localScale.x,
                boxCollider.size.y * transform.localScale.y,
                0.1f
            );
            
            Gizmos.DrawCube(center, size);
        }
    }
} 