using BepInEx;
using UnityEngine.SceneManagement;

namespace StudioAnimLoader
{
    [BepInProcess("CharaStudio")]
    [BepInPlugin("studioanimloaderkk", "StudioAnimLoader", Version)]
    class StudioAnimLoader : BaseUnityPlugin
    {
        public const string Version = "1.0.0";

        public static ConfigWrapper<string> OtherGameDir { get; set; }
        public static ConfigWrapper<string> GroupSuffix { get; set; }
        public static ConfigWrapper<int> GroupOffset { get; set; }
        public static ConfigWrapper<bool> Overwrite { get; set; }

        StudioAnimLoader()
        {
            OtherGameDir = new ConfigWrapper<string>("OtherGameDir", this, "");
            GroupSuffix = new ConfigWrapper<string>("GroupSuffix", this, "[MOD]");
            GroupOffset = new ConfigWrapper<int>("GroupOffset", this, 100);
            Overwrite = new ConfigWrapper<bool>("Overwrite", this, false);
        }

        void Start()
        {
            SceneLoaded();
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        void SceneLoaded()
        {
            if(Studio.Studio.Instance && !gameObject.GetComponent<LoaderComponent>())
                gameObject.AddComponent<LoaderComponent>();
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneLoaded();
        }
    }
}
