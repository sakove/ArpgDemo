using UnityEngine;
using System.Collections;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Scene Management")]
    [SerializeField] private string persistentSceneKey = "PersistentScene";
    [SerializeField] private string mainMenuSceneKey = "MainMenuScene";
    [SerializeField] private string firstLevelKey = "Level1";
    [SerializeField] private string currentLevelKey;
    
    [Header("Player Settings")]
    [SerializeField] private string playerPrefabKey = "Player";
    [SerializeField] private Transform defaultSpawnPoint;
    
    private bool isLoading = false;
    private PlayerManager playerManager;
    
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
    }
    
    private void Start()
    {
        // 初始化游戏
        StartCoroutine(InitializeGame());
    }
    
    private IEnumerator InitializeGame()
    {
        // 等待AddressablesManager初始化完成
        yield return new WaitUntil(() => AddressablesManager.Instance != null);
        
        // 加载第一个关卡
        yield return StartCoroutine(LoadFirstLevel());
    }
    
    private IEnumerator LoadFirstLevel()
    {
        yield return StartCoroutine(LoadLevel(firstLevelKey));
    }
    
    public void LoadMainMenu()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadMainMenuScene());
        }
    }
    
    private IEnumerator LoadMainMenuScene()
    {
        isLoading = true;
        
        // 显示加载屏幕
        ShowLoadingScreen(true);
        
        // 卸载当前关卡
        if (!string.IsNullOrEmpty(currentLevelKey))
        {
            AddressablesManager.Instance.UnloadScene(currentLevelKey, null);
            currentLevelKey = null;
        }
        
        // 加载主菜单场景
        AddressablesManager.Instance.LoadScene(mainMenuSceneKey, UnityEngine.SceneManagement.LoadSceneMode.Additive, true, OnMainMenuLoaded);
        
        // 等待加载完成
        yield return new WaitUntil(() => !isLoading);
        
        // 隐藏加载屏幕
        ShowLoadingScreen(false);
    }
    
    private void OnMainMenuLoaded(SceneInstance scene)
    {
        currentLevelKey = mainMenuSceneKey;
        isLoading = false;
        
        // 更新UI
        UIToolkitManager.Instance?.SetLevelName("主菜单");
    }
    
    public void LoadNextLevel(string levelKey)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadLevel(levelKey));
        }
    }
    
    public void RestartCurrentLevel()
    {
        if (!string.IsNullOrEmpty(currentLevelKey) && !isLoading)
        {
            StartCoroutine(LoadLevel(currentLevelKey));
        }
    }
    
    private IEnumerator LoadLevel(string levelKey)
    {
        isLoading = true;
        
        // 显示加载屏幕
        ShowLoadingScreen(true);
        
        // 卸载当前关卡
        if (!string.IsNullOrEmpty(currentLevelKey))
        {
            bool unloadComplete = false;
            AddressablesManager.Instance.UnloadScene(currentLevelKey, () => unloadComplete = true);
            yield return new WaitUntil(() => unloadComplete);
        }
        
        // 加载新关卡
        bool loadComplete = false;
        AddressablesManager.Instance.LoadScene(levelKey, UnityEngine.SceneManagement.LoadSceneMode.Additive, true, scene => {
            OnLevelLoaded(scene, levelKey);
            loadComplete = true;
        });
        
        // 等待加载完成
        yield return new WaitUntil(() => loadComplete);
        
        // 隐藏加载屏幕
        ShowLoadingScreen(false);
    }
    
    private void OnLevelLoaded(SceneInstance scene, string levelKey)
    {
        currentLevelKey = levelKey;
        
        // 查找生成点
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        Transform spawnTransform = spawnPoint != null ? spawnPoint.transform : defaultSpawnPoint;
        
        // 获取或生成玩家
        if (playerManager == null)
        {
            SpawnPlayer(spawnTransform.position);
        }
        else
        {
            // 移动现有玩家到生成点
            playerManager.TeleportToPosition(spawnTransform.position);
        }
        
        // 更新UI
        UIToolkitManager.Instance?.SetLevelName(GetLevelDisplayName(levelKey));
        
        isLoading = false;
    }
    
    private void SpawnPlayer(Vector3 position)
    {
        AddressablesManager.Instance.InstantiatePrefab(playerPrefabKey, null, true, playerObject => {
            if (playerObject != null)
            {
                // 设置玩家位置
                playerObject.transform.position = position;
                
                // 获取PlayerManager组件
                playerManager = playerObject.GetComponent<PlayerManager>();
                
                // 确保不会被销毁
                DontDestroyOnLoad(playerObject);
            }
            else
            {
                Debug.LogError("Failed to spawn player prefab!");
            }
        });
    }
    
    private string GetLevelDisplayName(string levelKey)
    {
        // 可以从本地化系统或配置中获取关卡显示名称
        switch (levelKey)
        {
            case "MainMenuScene":
                return "主菜单";
            case "Level1":
                return "第一关";
            case "Level2":
                return "第二关";
            default:
                return levelKey;
        }
    }
    
    private void ShowLoadingScreen(bool show)
    {
        // 显示/隐藏加载屏幕
        // 可以通过UIToolkitManager实现
    }
} 