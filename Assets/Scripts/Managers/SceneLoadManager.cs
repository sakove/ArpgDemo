using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;


public class SceneLoadManager : MonoBehaviour
{
    public FadePanel fadePanel;
    //AssetReference通用资源标志类
    private AssetReference currentScene;

    public AssetReference menu;

    public AssetReference intro;
    
    //private Room currentRoom;
    //[Header(header:"广播")]
    //public ObjectEventSO afterRoomLoadEvent;
    //public ObjectEventSO updateRoomEvent;

    private void Awake()
    {
        loadMenu();
        //loadIntro();
    }
/*
    public async void  OnLoadRoomEvent(object data)
    {
        if (data is Room)
        {
            currentRoom =data as Room;

            var currentData = currentRoom.roomData;

            currentScene = currentData.sceneToLoad;
        }

        //卸载当前的场景（地图）
        await UnloadSceneTask();
        //加载房间
        await LoadSceneTask();
        
        afterRoomLoadEvent.RaiseEvent(currentRoom,this);
    }
*/
    private async Awaitable LoadSceneTask()
    {
        //Addressable下异步加载 
        var s= currentScene.LoadSceneAsync(LoadSceneMode.Additive);
        //Task
        await s.Task;

        //Status
        if (s.Status == AsyncOperationStatus.Succeeded)
        {
            fadePanel.FadeOut(0.2f);
            //Result
            SceneManager.SetActiveScene(s.Result.Scene);
        }
    }



    private async Awaitable UnloadSceneTask()
    {
        fadePanel.FadeIn(0.4f);
        await Awaitable.WaitForSecondsAsync(0.45f);
        
        await Awaitable.FromAsyncOperation(SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene()));
    }
    
    /*
    public async void loadMap()
    {
        await UnloadSceneTask();
        if (currentRoomVector2 != Vector2.one * -1)
        { 
            updateRoomEvent.RaiseEvent(currentRoomVector2,this);
        }
        currentScene = map;
        await LoadSceneTask(); 
    }
    */
    public async void loadMenu()
    {
        if(currentScene!=null) await UnloadSceneTask();
        currentScene = menu;
        await LoadSceneTask();
    }
    public async void loadIntro()
    {
        if(currentScene!=null) await UnloadSceneTask();
        currentScene = intro;
        await LoadSceneTask();
    }
}