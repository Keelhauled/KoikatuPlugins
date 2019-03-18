using BepInEx;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TogglePOVKK
{
    [BepInPlugin("togglepovkk", "TogglePOV", Version)]
    public class TogglePOV : BaseUnityPlugin
    {
        public const string Version = "1.0.0";

        [DisplayName("Toggle POV")]
        public static SavedKeyboardShortcut POVKey { get; set; }

        [DisplayName("Default fov")]
        [AcceptableValueRange(20f, 120f, false)]
        public static ConfigWrapper<float> DefaultFov { get; set; }

        [DisplayName("Show hair")]
        [Description("Controls if hair is left untouched when going into first person.")]
        public static ConfigWrapper<bool> ShowHair { get; set; }

        [DisplayName("Male offset")]
        [Advanced(true)]
        public static ConfigWrapper<float> MaleOffset { get; set; }

        [DisplayName("Female offset")]
        [Advanced(true)]
        public static ConfigWrapper<float> FemaleOffset { get; set; }

        TogglePOV()
        {
            POVKey = new SavedKeyboardShortcut("POVKey", this, new KeyboardShortcut(KeyCode.Backspace));
            DefaultFov = new ConfigWrapper<float>("DefaultFov", this, 70f);
            ShowHair = new ConfigWrapper<bool>("ShowHair", this, false);
            MaleOffset = new ConfigWrapper<float>("MaleOffset", this, 0.042f);
            FemaleOffset = new ConfigWrapper<float>("FemaleOffset", this, 0.0315f);
        }

        void Awake()
        {
            SceneLoaded();
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneLoaded();
        }

        void SceneLoaded()
        {
            var comp = gameObject.GetComponent<BaseMono>();

            if(FindObjectOfType<HScene>())
            {
                if(!comp) gameObject.AddComponent<HSceneMono>();
            }
            if(FindObjectOfType<StudioScene>())
            {
                if(!comp) gameObject.AddComponent<StudioMono>();
            }
            else if(comp)
            {
                Destroy(comp);
                Destroy(gameObject.GetComponent<DragManager>());
            }
        }
    }
}
