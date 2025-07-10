using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DemoScene : MonoBehaviour
{
    [Header("预制体")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject platformPrefab;
    
    [Header("场景设置")]
    [SerializeField] private Vector2 sceneSize = new Vector2(20f, 10f);
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f);
    
    private void Awake()
    {
        // 创建必要的游戏对象
        CreateCamera();
        CreatePlatform();
        CreatePlayer();
        CreateEnemy();
    }
    
    private void CreateCamera()
    {
        // 查找或创建主相机
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }
        
        // 设置相机属性
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = sceneSize.y / 2;
        mainCamera.backgroundColor = backgroundColor;
        mainCamera.transform.position = new Vector3(0f, sceneSize.y / 4, -10f);
        
        // 添加简单的相机跟随脚本
        SimpleCameraFollow cameraFollow = mainCamera.gameObject.GetComponent<SimpleCameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<SimpleCameraFollow>();
        }
    }
    
    private void CreatePlatform()
    {
        // 如果没有指定平台预制体，创建一个简单的平台
        if (platformPrefab == null)
        {
            platformPrefab = new GameObject("Platform");
            platformPrefab.AddComponent<BoxCollider2D>();
            platformPrefab.AddComponent<Platform>();
            
            // 添加精灵渲染器
            SpriteRenderer renderer = platformPrefab.AddComponent<SpriteRenderer>();
            renderer.color = Color.green;
            
            // 创建一个简单的白色方块精灵
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(
                texture, 
                new Rect(0, 0, 1, 1), 
                new Vector2(0.5f, 0.5f)
            );
            
            renderer.sprite = sprite;
        }
        
        // 实例化平台
        GameObject platform = Instantiate(
            platformPrefab, 
            new Vector3(0, -sceneSize.y / 2 + 0.5f, 0), 
            Quaternion.identity
        );
        
        platform.name = "Ground";
        platform.transform.localScale = new Vector3(sceneSize.x, 1f, 1f);
        platform.layer = LayerMask.NameToLayer("Ground");
    }
    
    private void CreatePlayer()
    {
        // 如果没有指定玩家预制体，创建一个简单的玩家
        if (playerPrefab == null)
        {
            playerPrefab = new GameObject("Player");
            
            // 添加必要的组件
            Rigidbody2D rb = playerPrefab.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            playerPrefab.AddComponent<BoxCollider2D>();
            playerPrefab.AddComponent<PlayerController>();
            playerPrefab.AddComponent<PlayerHealth>();
            playerPrefab.AddComponent<PlayerInput>();
            
            // 添加精灵渲染器
            SpriteRenderer renderer = playerPrefab.AddComponent<SpriteRenderer>();
            renderer.color = Color.blue;
            
            // 创建一个简单的白色方块精灵
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(
                texture, 
                new Rect(0, 0, 1, 1), 
                new Vector2(0.5f, 0.5f)
            );
            
            renderer.sprite = sprite;
            
            // 创建攻击点
            GameObject attackPoint = new GameObject("AttackPoint");
            attackPoint.transform.SetParent(playerPrefab.transform);
            attackPoint.transform.localPosition = new Vector3(1f, 0f, 0f);
        }
        
        // 实例化玩家
        GameObject player = Instantiate(
            playerPrefab, 
            new Vector3(-5f, 0f, 0f), 
            Quaternion.identity
        );
        
        player.name = "Player";
        player.transform.localScale = new Vector3(1f, 2f, 1f);
        player.layer = LayerMask.NameToLayer("Player");
        
        // 设置相机跟随目标
        Camera.main.GetComponent<SimpleCameraFollow>().target = player.transform;
    }
    
    private void CreateEnemy()
    {
        // 如果没有指定敌人预制体，创建一个简单的敌人
        if (enemyPrefab == null)
        {
            enemyPrefab = new GameObject("Enemy");
            
            // 添加必要的组件
            Rigidbody2D rb = enemyPrefab.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            enemyPrefab.AddComponent<BoxCollider2D>();
            enemyPrefab.AddComponent<BasicEnemy>();
            
            // 添加精灵渲染器
            SpriteRenderer renderer = enemyPrefab.AddComponent<SpriteRenderer>();
            renderer.color = Color.red;
            
            // 创建一个简单的白色方块精灵
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(
                texture, 
                new Rect(0, 0, 1, 1), 
                new Vector2(0.5f, 0.5f)
            );
            
            renderer.sprite = sprite;
        }
        
        // 实例化敌人
        GameObject enemy = Instantiate(
            enemyPrefab, 
            new Vector3(5f, 0f, 0f), 
            Quaternion.identity
        );
        
        enemy.name = "Enemy";
        enemy.transform.localScale = new Vector3(1f, 2f, 1f);
        enemy.layer = LayerMask.NameToLayer("Enemy");
    }
} 