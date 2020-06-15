using Scripts;
using Scripts.Utils.Async;
using Scripts.Services;
using Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecutionOrder(-1000)]
public class Entry : SingletonSelfCreator<Entry>
{

    [Header("Don't forget to pass it into the Resources folder!")]
    [SerializeField] private SceneManagementService sceneService = null;
    [SerializeField] private ConverterJsonUtility jsonConverter = null;

    protected override string PrefabPath => nameof(Entry);
    // protected override bool IsDontDestroy => false;
    [System.NonSerialized] private bool isInitialized = false;

    protected override void InitInstance()
    {
        Init();
    }

    public void Init()
    {
        if (isInitialized)
        {
            return;
        }
        isInitialized = true;
        ThreadTools.Initialize();

        List<IServices> currentServices = new List<IServices>
        {
            sceneService,
            jsonConverter
        };

        List<IController> controllers = new List<IController>
        {
            // new DownloadController(),
        };
        App.Start(currentServices, controllers);
        // Init only after the App starts
        foreach (var item in controllers)
        {
            item.Init();
        }

        if (gameObject.scene.name.Equals(sceneService.Scenes[1]))
        {
            return;
        }
        App.SceneService.LoadSceneWithVideo(1, null, 2);
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        App.Quit();
    }

    private void OnApplicationPause(bool value)
    {
        App.Pause(value);
    }
}
