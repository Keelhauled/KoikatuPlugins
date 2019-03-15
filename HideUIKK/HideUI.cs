using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HideUIKK
{
    [BepInPlugin("keelhauled.hideuibep", "HideUI", "1.0.0")]
    class HideUI : BaseUnityPlugin
    {
        string path;
        Dictionary<string, CacheObject> canvasCache;
        SavedKeyboardShortcut HideUIHotkey { get; }

        HideUI()
        {
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/HideUI.txt";
            canvasCache = new Dictionary<string, CacheObject>();
            HideUIHotkey = new SavedKeyboardShortcut("Hide UI", this, new KeyboardShortcut(KeyCode.M));
        }

        void Awake()
        {
            if(Application.productName != "CharaStudio")
                SceneManager.sceneLoaded += (x, y) => canvasCache = new Dictionary<string, CacheObject>();
        }

        void Update()
        {
            if(HideUIHotkey.IsDown() && Allowed())
            {
                UpdateCache();
                ProcessCache();
            }
        }

        bool Allowed()
        {
            return !Manager.Scene.Instance.IsNowLoadingFade && SceneManager.GetActiveScene().name != "CustomScene";
        }

        void UpdateCache()
        {
            var names = File.ReadAllLines(path);
            if(names.Length > 0)
            {
                var gameobjects = FindObjectsOfType<GameObject>();
                foreach(var canvasName in names)
                {
                    if(!canvasCache.TryGetValue(canvasName, out CacheObject cacheobject))
                    {
                        var split = canvasName.Split('|');
                        var match = gameobjects.Where(x => x.name == split[0]).ToList();

                        // Pick gameobject with RectTransform
                        GameObject gameobject = null;
                        for(int i = 0; i < match.Count; i++)
                        {
                            if(match[i].GetComponent<RectTransform>())
                            {
                                gameobject = match[i];
                                break;
                            }
                        }

                        if(gameobject)
                        {
                            try
                            {
                                cacheobject = new CacheObject
                                {
                                    gameobject = gameobject,
                                    canvas = gameobject.GetComponent<Canvas>(),
                                    canvasName = split[0],
                                    hideAction = (CacheObject.HideAction)Enum.Parse(typeof(CacheObject.HideAction), split[1], true),
                                    hideMethod = (CacheObject.HideMethod)Enum.Parse(typeof(CacheObject.HideMethod), split[2], true)
                                };

                                Console.WriteLine(gameobject.name + " detected");

                                if(cacheobject.hideMethod == CacheObject.HideMethod.Enabled && !cacheobject.canvas)
                                {
                                    cacheobject.hideMethod = CacheObject.HideMethod.SetActive;
                                    Console.WriteLine(" Canvas not found, switching to setactive mode");
                                }

                                canvasCache.Add(canvasName, cacheobject);
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
            }
        }

        void ProcessCache()
        {
            if(canvasCache.Count > 0)
            {
                bool flag = GetMasterFlag(canvasCache.First().Value);
                bool savedFlag = canvasCache.First().Value.savedState;

                foreach(var cacheobject in canvasCache.Values.ToList())
                {
                    switch(cacheobject.hideAction)
                    {
                        case CacheObject.HideAction.Toggle:
                        {
                            ShowCanvas(cacheobject, !flag);
                            break;
                        }
                        case CacheObject.HideAction.False:
                        {
                            ShowCanvas(cacheobject, false);
                            break;
                        }
                        case CacheObject.HideAction.True:
                        {
                            ShowCanvas(cacheobject, true);
                            break;
                        }
                        case CacheObject.HideAction.Save:
                        {
                            switch(cacheobject.hideMethod)
                            {
                                case CacheObject.HideMethod.SetActive:
                                {
                                    if(cacheobject.gameobject.activeSelf)
                                    {
                                        cacheobject.savedState = cacheobject.gameobject.activeSelf;
                                        ShowCanvas(cacheobject, false);
                                    }
                                    else if(!cacheobject.gameobject.activeSelf && cacheobject.savedState)
                                    {
                                        cacheobject.savedState = cacheobject.gameobject.activeSelf;
                                        ShowCanvas(cacheobject, !flag);
                                    }
                                    else if(!cacheobject.gameobject.activeSelf && !cacheobject.savedState)
                                    {
                                        cacheobject.savedState = cacheobject.gameobject.activeSelf;
                                        ShowCanvas(cacheobject, false);
                                    }
                                    break;
                                }

                                case CacheObject.HideMethod.Enabled:
                                {
                                    if(cacheobject.canvas.enabled)
                                    {
                                        cacheobject.savedState = cacheobject.canvas.enabled;
                                        ShowCanvas(cacheobject, false);
                                    }
                                    else if(!cacheobject.canvas.enabled && cacheobject.savedState)
                                    {
                                        cacheobject.savedState = cacheobject.canvas.enabled;
                                        ShowCanvas(cacheobject, true);
                                    }
                                    else if(!cacheobject.canvas.enabled && !cacheobject.savedState)
                                    {
                                        cacheobject.savedState = cacheobject.canvas.enabled;
                                        ShowCanvas(cacheobject, false);
                                    }
                                    break;
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }

        void ShowCanvas(CacheObject cacheobject, bool flag)
        {
            switch(cacheobject.hideMethod)
            {
                case CacheObject.HideMethod.SetActive:
                {
                    cacheobject.gameobject.SetActive(flag);
                    break;
                }
                case CacheObject.HideMethod.Enabled:
                {
                    cacheobject.canvas.enabled = flag;
                    break;
                }
            }
        }

        bool GetMasterFlag(CacheObject cacheobject)
        {
            switch(cacheobject.hideMethod)
            {
                case CacheObject.HideMethod.SetActive:
                {
                    return cacheobject.gameobject.activeSelf;
                }
                case CacheObject.HideMethod.Enabled:
                {
                    return cacheobject.canvas.enabled;
                }
            }

            return true;
        }

        class CacheObject
        {
            public GameObject gameobject;
            public Canvas canvas;
            public bool savedState = false;
            public string canvasName;
            public HideAction hideAction;
            public HideMethod hideMethod;

            public enum HideAction
            {
                Toggle,
                Save,
                False,
                True,
            }

            public enum HideMethod
            {
                SetActive,
                Enabled,
            }
        }
    }
}
