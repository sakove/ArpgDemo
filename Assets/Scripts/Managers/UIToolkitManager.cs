using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

/// <summary>
/// 基于UI Toolkit和Addressables的UI管理器
/// 负责加载、显示和管理游戏中的所有UI元素
/// </summary>
public class UIToolkitManager : MonoBehaviour
{
    public static UIToolkitManager Instance { get; private set; }
    
    [Header("UI Documents")]
    [SerializeField] private UIDocument mainUIDocument;
    [SerializeField] private UIDocument overlayUIDocument;
    
    [Header("UI Asset References")]
    [SerializeField] private string mainUIAssetPath = "Assets/UI/MainUI.uxml";
    [SerializeField] private string gameOverUIAssetPath = "Assets/UI/GameOverUI.uxml";
    [SerializeField] private string pauseUIAssetPath = "Assets/UI/PauseUI.uxml";
    
    // UI元素缓存
    private Dictionary<string, VisualElement> uiElements = new Dictionary<string, VisualElement>();
    
    // UI Document根元素
    private VisualElement mainRoot;
    private VisualElement overlayRoot;
    
    // UI状态
    private bool isPaused = false;
    private bool isGameOver = false;
    
    // 当前加载的UI操作句柄
    private List<AsyncOperationHandle> activeHandles = new List<AsyncOperationHandle>();

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
    
    private void OnDestroy()
    {
        // 释放所有Addressables资源
        foreach (var handle in activeHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        
        activeHandles.Clear();
    }
    
    private void Start()
    {
        // 初始化UI Document根元素
        if (mainUIDocument != null)
        {
            mainRoot = mainUIDocument.rootVisualElement;
        }
        
        if (overlayUIDocument != null)
        {
            overlayRoot = overlayUIDocument.rootVisualElement;
        }
        
        // 加载主UI
        StartCoroutine(LoadMainUI());
        
        // 订阅玩家事件
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            playerManager.OnHealthChanged += UpdateHealthUI;
            playerManager.OnEnergyChanged += UpdateEnergyUI;
            playerManager.OnPlayerDied += ShowGameOverUI;
        }
    }
    
    private IEnumerator LoadMainUI()
    {
        // 使用Addressables异步加载主UI资源
        AsyncOperationHandle<VisualTreeAsset> handle = Addressables.LoadAssetAsync<VisualTreeAsset>(mainUIAssetPath);
        activeHandles.Add(handle);
        
        yield return handle;
        
        if (handle.Status == AsyncOperationStatus.Succeeded && mainRoot != null)
        {
            // 实例化UI树
            VisualTreeAsset visualTree = handle.Result;
            visualTree.CloneTree(mainRoot);
            
            // 缓存常用UI元素引用
            CacheUIElements(mainRoot);
            
            // 初始化UI状态
            InitializeUI();
        }
        else
        {
            Debug.LogError("Failed to load main UI: " + mainUIAssetPath);
        }
    }
    
    private void CacheUIElements(VisualElement root)
    {
        // 缓存健康值UI元素
        VisualElement healthBar = root.Q<VisualElement>("health-bar");
        if (healthBar != null)
        {
            uiElements["health-bar"] = healthBar;
            uiElements["health-bar-fill"] = healthBar.Q<VisualElement>("health-bar-fill");
            uiElements["health-text"] = healthBar.Q<Label>("health-text");
        }
        
        // 缓存能量UI元素
        VisualElement energyBar = root.Q<VisualElement>("energy-bar");
        if (energyBar != null)
        {
            uiElements["energy-bar"] = energyBar;
            uiElements["energy-bar-fill"] = energyBar.Q<VisualElement>("energy-bar-fill");
            uiElements["energy-text"] = energyBar.Q<Label>("energy-text");
        }
        
        // 缓存关卡名称
        Label levelNameLabel = root.Q<Label>("level-name");
        if (levelNameLabel != null)
        {
            uiElements["level-name"] = levelNameLabel;
        }
    }
    
    private void InitializeUI()
    {
        // 初始化玩家状态UI
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            UpdateHealthUI(playerManager.GetCurrentHealth(), playerManager.GetMaxHealth());
            UpdateEnergyUI(playerManager.GetCurrentEnergy(), playerManager.GetMaxEnergy());
        }
        
        // 注册全局输入事件
        RegisterGlobalEvents();
    }
    
    private void RegisterGlobalEvents()
    {
        // 注册ESC键暂停事件
        if (mainRoot != null)
        {
            mainRoot.RegisterCallback<KeyDownEvent>(evt => {
                if (evt.keyCode == KeyCode.Escape)
                {
                    TogglePauseUI();
                }
            });
        }
    }
    
    #region UI更新方法
    
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (uiElements.TryGetValue("health-bar-fill", out VisualElement healthBarFill))
        {
            // 更新血条宽度
            float fillPercentage = (float)currentHealth / maxHealth;
            healthBarFill.style.width = Length.Percent(fillPercentage * 100);
        }
        
        if (uiElements.TryGetValue("health-text", out VisualElement healthText))
        {
            // 更新血量文本
            (healthText as Label).text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    public void UpdateEnergyUI(int currentEnergy, int maxEnergy)
    {
        if (uiElements.TryGetValue("energy-bar-fill", out VisualElement energyBarFill))
        {
            // 更新能量条宽度
            float fillPercentage = (float)currentEnergy / maxEnergy;
            energyBarFill.style.width = Length.Percent(fillPercentage * 100);
        }
        
        if (uiElements.TryGetValue("energy-text", out VisualElement energyText))
        {
            // 更新能量文本
            (energyText as Label).text = $"{currentEnergy}/{maxEnergy}";
        }
    }
    
    public void SetLevelName(string levelName)
    {
        if (uiElements.TryGetValue("level-name", out VisualElement levelNameElement))
        {
            (levelNameElement as Label).text = levelName;
        }
    }
    
    #endregion
    
    #region UI显示控制
    
    public void ShowGameOverUI()
    {
        if (isGameOver) return;
        
        StartCoroutine(LoadOverlayUI(gameOverUIAssetPath, OnGameOverUILoaded));
        isGameOver = true;
    }
    
    private void OnGameOverUILoaded(VisualElement gameOverUI)
    {
        // 注册游戏结束UI的按钮事件
        Button restartButton = gameOverUI.Q<Button>("restart-button");
        if (restartButton != null)
        {
            restartButton.clicked += OnRestartButtonClicked;
        }
        
        Button mainMenuButton = gameOverUI.Q<Button>("main-menu-button");
        if (mainMenuButton != null)
        {
            mainMenuButton.clicked += OnMainMenuButtonClicked;
        }
    }
    
    public void TogglePauseUI()
    {
        if (isGameOver) return;
        
        if (isPaused)
        {
            HidePauseUI();
        }
        else
        {
            ShowPauseUI();
        }
    }
    
    private void ShowPauseUI()
    {
        if (isPaused) return;
        
        StartCoroutine(LoadOverlayUI(pauseUIAssetPath, OnPauseUILoaded));
        Time.timeScale = 0f;
        isPaused = true;
    }
    
    private void HidePauseUI()
    {
        if (!isPaused) return;
        
        if (overlayRoot != null)
        {
            overlayRoot.Clear();
        }
        
        Time.timeScale = 1f;
        isPaused = false;
    }
    
    private void OnPauseUILoaded(VisualElement pauseUI)
    {
        // 注册暂停UI的按钮事件
        Button resumeButton = pauseUI.Q<Button>("resume-button");
        if (resumeButton != null)
        {
            resumeButton.clicked += OnResumeButtonClicked;
        }
        
        Button optionsButton = pauseUI.Q<Button>("options-button");
        if (optionsButton != null)
        {
            optionsButton.clicked += OnOptionsButtonClicked;
        }
        
        Button quitButton = pauseUI.Q<Button>("quit-button");
        if (quitButton != null)
        {
            quitButton.clicked += OnQuitButtonClicked;
        }
    }
    
    private IEnumerator LoadOverlayUI(string assetPath, Action<VisualElement> onLoaded)
    {
        if (overlayRoot == null)
        {
            Debug.LogError("Overlay UI Document not assigned");
            yield break;
        }
        
        // 清除之前的UI
        overlayRoot.Clear();
        
        // 使用Addressables异步加载UI资源
        AsyncOperationHandle<VisualTreeAsset> handle = Addressables.LoadAssetAsync<VisualTreeAsset>(assetPath);
        activeHandles.Add(handle);
        
        yield return handle;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // 实例化UI树
            VisualTreeAsset visualTree = handle.Result;
            TemplateContainer container = visualTree.CloneTree();
            overlayRoot.Add(container);
            
            // 调用回调
            onLoaded?.Invoke(container);
        }
        else
        {
            Debug.LogError($"Failed to load UI: {assetPath}");
        }
    }
    
    #endregion
    
    #region 按钮事件处理
    
    private void OnResumeButtonClicked()
    {
        HidePauseUI();
    }
    
    private void OnOptionsButtonClicked()
    {
        // 显示选项UI
        Debug.Log("Options button clicked");
    }
    
    private void OnQuitButtonClicked()
    {
        // 返回主菜单
        Time.timeScale = 1f;
        // GameManager.Instance.LoadMainMenu();
    }
    
    private void OnRestartButtonClicked()
    {
        // 重新开始当前关卡
        isGameOver = false;
        Time.timeScale = 1f;
        
        if (overlayRoot != null)
        {
            overlayRoot.Clear();
        }
        
        // GameManager.Instance.RestartCurrentLevel();
    }
    
    private void OnMainMenuButtonClicked()
    {
        // 返回主菜单
        isGameOver = false;
        Time.timeScale = 1f;
        
        if (overlayRoot != null)
        {
            overlayRoot.Clear();
        }
        
        // GameManager.Instance.LoadMainMenu();
    }
    
    #endregion
} 