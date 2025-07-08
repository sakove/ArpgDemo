using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// 基于Addressables的资源管理器
/// 负责加载、卸载和管理游戏资源
/// </summary>
public class AddressablesManager : MonoBehaviour
{
    public static AddressablesManager Instance { get; private set; }
    
    // 跟踪所有加载的资源句柄
    private Dictionary<string, AsyncOperationHandle> loadedAssets = new Dictionary<string, AsyncOperationHandle>();
    private Dictionary<string, AsyncOperationHandle<SceneInstance>> loadedScenes = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();
    
    // 预加载资源列表
    [SerializeField] private List<AssetReference> preloadAssets = new List<AssetReference>();
    
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
        // 预加载资源
        StartCoroutine(PreloadAssets());
    }
    
    private void OnDestroy()
    {
        // 释放所有加载的资源
        ReleaseAllAssets();
    }
    
    private IEnumerator PreloadAssets()
    {
        foreach (AssetReference assetRef in preloadAssets)
        {
            if (assetRef != null)
            {
                AsyncOperationHandle handle = assetRef.LoadAssetAsync<UnityEngine.Object>();
                yield return handle;
                
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    string key = assetRef.AssetGUID;
                    loadedAssets[key] = handle;
                    Debug.Log($"Preloaded asset: {key}");
                }
                else
                {
                    Debug.LogError($"Failed to preload asset: {assetRef.AssetGUID}");
                }
            }
        }
    }
    
    #region 资源加载方法
    
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="key">资源键或地址</param>
    /// <param name="callback">加载完成回调</param>
    public void LoadAsset<T>(string key, Action<T> callback) where T : UnityEngine.Object
    {
        StartCoroutine(LoadAssetAsync<T>(key, callback));
    }
    
    private IEnumerator LoadAssetAsync<T>(string key, Action<T> callback) where T : UnityEngine.Object
    {
        // 检查资源是否已加载
        if (loadedAssets.TryGetValue(key, out AsyncOperationHandle existingHandle))
        {
            if (existingHandle.IsValid())
            {
                callback?.Invoke(existingHandle.Result as T);
                yield break;
            }
            else
            {
                loadedAssets.Remove(key);
            }
        }
        
        // 加载新资源
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        yield return handle;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedAssets[key] = handle;
            callback?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogError($"Failed to load asset: {key}");
            callback?.Invoke(null);
        }
    }
    
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="key">场景键或地址</param>
    /// <param name="loadMode">加载模式</param>
    /// <param name="activateOnLoad">加载后是否激活</param>
    /// <param name="callback">加载完成回调</param>
    public void LoadScene(string key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, Action<SceneInstance> callback = null)
    {
        StartCoroutine(LoadSceneAsync(key, loadMode, activateOnLoad, callback));
    }
    
    private IEnumerator LoadSceneAsync(string key, LoadSceneMode loadMode, bool activateOnLoad, Action<SceneInstance> callback)
    {
        // 检查场景是否已加载
        if (loadedScenes.TryGetValue(key, out AsyncOperationHandle<SceneInstance> existingHandle))
        {
            if (existingHandle.IsValid())
            {
                callback?.Invoke(existingHandle.Result);
                yield break;
            }
            else
            {
                loadedScenes.Remove(key);
            }
        }
        
        // 加载新场景
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad);
        yield return handle;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedScenes[key] = handle;
            callback?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogError($"Failed to load scene: {key}");
            callback?.Invoke(new SceneInstance());
        }
    }
    
    /// <summary>
    /// 异步实例化预制体
    /// </summary>
    /// <param name="key">预制体键或地址</param>
    /// <param name="parent">父对象</param>
    /// <param name="instantiateInWorldSpace">是否在世界空间中实例化</param>
    /// <param name="callback">实例化完成回调</param>
    public void InstantiatePrefab(string key, Transform parent = null, bool instantiateInWorldSpace = false, Action<GameObject> callback = null)
    {
        StartCoroutine(InstantiatePrefabAsync(key, parent, instantiateInWorldSpace, callback));
    }
    
    private IEnumerator InstantiatePrefabAsync(string key, Transform parent, bool instantiateInWorldSpace, Action<GameObject> callback)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace);
        yield return handle;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            callback?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogError($"Failed to instantiate prefab: {key}");
            callback?.Invoke(null);
        }
    }
    
    #endregion
    
    #region 资源释放方法
    
    /// <summary>
    /// 释放指定资源
    /// </summary>
    /// <param name="key">资源键或地址</param>
    public void ReleaseAsset(string key)
    {
        if (loadedAssets.TryGetValue(key, out AsyncOperationHandle handle))
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            
            loadedAssets.Remove(key);
        }
    }
    
    /// <summary>
    /// 卸载场景
    /// </summary>
    /// <param name="key">场景键或地址</param>
    /// <param name="callback">卸载完成回调</param>
    public void UnloadScene(string key, Action callback = null)
    {
        StartCoroutine(UnloadSceneAsync(key, callback));
    }
    
    private IEnumerator UnloadSceneAsync(string key, Action callback)
    {
        if (loadedScenes.TryGetValue(key, out AsyncOperationHandle<SceneInstance> handle))
        {
            if (handle.IsValid())
            {
                AsyncOperationHandle<SceneInstance> unloadHandle = Addressables.UnloadSceneAsync(handle);
                yield return unloadHandle;
                
                if (unloadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"Successfully unloaded scene: {key}");
                }
                else
                {
                    Debug.LogError($"Failed to unload scene: {key}");
                }
            }
            
            loadedScenes.Remove(key);
        }
        
        callback?.Invoke();
    }
    
    /// <summary>
    /// 释放所有加载的资源
    /// </summary>
    public void ReleaseAllAssets()
    {
        // 释放所有加载的资源
        foreach (var handle in loadedAssets.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        
        loadedAssets.Clear();
        
        // 卸载所有加载的场景
        foreach (var handle in loadedScenes.Values)
        {
            if (handle.IsValid())
            {
                Addressables.UnloadSceneAsync(handle).Completed += op => { };
            }
        }
        
        loadedScenes.Clear();
    }
    
    #endregion
} 