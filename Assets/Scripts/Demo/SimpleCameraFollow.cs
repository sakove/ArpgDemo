using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;
    
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private float minX = float.MinValue;
    [SerializeField] private float maxX = float.MaxValue;
    [SerializeField] private float minY = float.MinValue;
    [SerializeField] private float maxY = float.MaxValue;
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;
        
        // 限制坐标范围
        float targetX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float targetY = Mathf.Clamp(desiredPosition.y, minY, maxY);
        
        // 只跟随选定的轴
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        if (!followX) smoothedPosition.x = transform.position.x;
        if (!followY) smoothedPosition.y = transform.position.y;
        
        // 应用位置
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, offset.z);
    }
} 