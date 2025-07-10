using UnityEngine;
using UnityEngine.InputSystem;

public class DemoManager : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    
    [Header("Enemy Setup")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemySpawnPoint;
    
    [Header("Environment")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private Vector2 platformSize = new Vector2(20f, 1f);
    
    private GameObject player;
    private GameObject enemy;
    private GameObject platform;
    
    private void Start()
    {
        SetupEnvironment();
        SpawnPlayer();
        SpawnEnemy();
        
        Debug.Log("Demo场景初始化完成");
    }
    
    private void SetupEnvironment()
    {
        // 创建平台
        platform = Instantiate(platformPrefab, Vector3.zero, Quaternion.identity);
        platform.transform.localScale = new Vector3(platformSize.x, platformSize.y, 1f);
        platform.name = "Ground";
        
        // 添加碰撞器
        BoxCollider2D collider = platform.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = platform.AddComponent<BoxCollider2D>();
        }
    }
    
    private void SpawnPlayer()
    {
        Vector3 spawnPosition = playerSpawnPoint != null 
            ? playerSpawnPoint.position 
            : new Vector3(-5f, 2f, 0f);
            
        player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        player.name = "Player";
        
        // 确保玩家有必要的组件
        if (player.GetComponent<PlayerController>() == null)
        {
            player.AddComponent<PlayerController>();
        }
        
        if (player.GetComponent<PlayerHealth>() == null)
        {
            player.AddComponent<PlayerHealth>();
        }
        
        if (player.GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
        }
        
        if (player.GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 2f);
        }
        
        if (player.GetComponent<PlayerInput>() == null)
        {
            player.AddComponent<PlayerInput>();
        }
    }
    
    private void SpawnEnemy()
    {
        Vector3 spawnPosition = enemySpawnPoint != null 
            ? enemySpawnPoint.position 
            : new Vector3(5f, 2f, 0f);
            
        enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.name = "Enemy";
        
        // 确保敌人有必要的组件
        if (enemy.GetComponent<BasicEnemy>() == null)
        {
            enemy.AddComponent<BasicEnemy>();
        }
        
        if (enemy.GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
        }
        
        if (enemy.GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 2f);
        }
    }
    
    // 用于测试的重置方法
    public void ResetDemo()
    {
        if (player != null) Destroy(player);
        if (enemy != null) Destroy(enemy);
        
        SpawnPlayer();
        SpawnEnemy();
        
        Debug.Log("Demo已重置");
    }
} 