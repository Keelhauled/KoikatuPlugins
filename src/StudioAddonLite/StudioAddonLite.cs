using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using SharedPluginCode;

namespace StudioAddonLite
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin("studioaddonlite", "StudioAddonLite", Version)]
    public class StudioAddonLite : BaseUnityPlugin
    {
        public const string Version = "1.0.0";

        [AcceptableValueRange(0f, 10f, true)]
        public static ConfigWrapper<float> MOVE_RATIO { get; set; }
        [AcceptableValueRange(0f, 360f, true)]
        public static ConfigWrapper<float> ROTATE_RATIO { get; set; }
        [AcceptableValueRange(0f, 2f, true)]
        public static ConfigWrapper<float> SCALE_RATIO { get; set; }

        public static SavedKeyboardShortcut KEY_OBJ_MOVE_XZ { get; set; }
        public static SavedKeyboardShortcut KEY_OBJ_MOVE_Y { get; set; }

        public static SavedKeyboardShortcut KEY_OBJ_ROT_X { get; set; }
        public static SavedKeyboardShortcut KEY_OBJ_ROT_Y { get; set; }
        public static SavedKeyboardShortcut KEY_OBJ_ROT_Z { get; set; }

        public static SavedKeyboardShortcut KEY_OBJ_ROT_X_2 { get; set; }
        public static SavedKeyboardShortcut KEY_OBJ_ROT_Y_2 { get; set; }
        public static SavedKeyboardShortcut KEY_OBJ_ROT_Z_2 { get; set; }

        public static SavedKeyboardShortcut KEY_OBJ_SCALE_ALL { get; set; }
        public static SavedKeyboardShortcut KEY_OBJ_SCALE_X { get; set; }
        public static SavedKeyboardShortcut KEY_OBJ_SCALE_Y { get; set; }
        public static SavedKeyboardShortcut KEY_OBJ_SCALE_Z { get; set; }

        StudioAddonLite()
        {
            MOVE_RATIO = new ConfigWrapper<float>("MOVE_RATIO", this, 2.5f);
            ROTATE_RATIO = new ConfigWrapper<float>("ROTATE_RATIO", this, 90f);
            SCALE_RATIO = new ConfigWrapper<float>("SCALE_RATIO", this, 0.5f);

            KEY_OBJ_MOVE_XZ = new SavedKeyboardShortcut("KEY_OBJ_MOVE_XZ", this, new KeyboardShortcut(KeyCode.G));
            KEY_OBJ_MOVE_Y = new SavedKeyboardShortcut("KEY_OBJ_MOVE_Y", this, new KeyboardShortcut(KeyCode.H));

            KEY_OBJ_ROT_X = new SavedKeyboardShortcut("KEY_OBJ_ROT_X", this, new KeyboardShortcut(KeyCode.G, KeyCode.LeftShift));
            KEY_OBJ_ROT_Y = new SavedKeyboardShortcut("KEY_OBJ_ROT_Y", this, new KeyboardShortcut(KeyCode.H, KeyCode.LeftShift));
            KEY_OBJ_ROT_Z = new SavedKeyboardShortcut("KEY_OBJ_ROT_Z", this, new KeyboardShortcut(KeyCode.Y, KeyCode.LeftShift));
            KEY_OBJ_ROT_X_2 = new SavedKeyboardShortcut("KEY_OBJ_ROT_X_2", this, new KeyboardShortcut(KeyCode.G));
            KEY_OBJ_ROT_Y_2 = new SavedKeyboardShortcut("KEY_OBJ_ROT_Y_2", this, new KeyboardShortcut(KeyCode.H));
            KEY_OBJ_ROT_Z_2 = new SavedKeyboardShortcut("KEY_OBJ_ROT_Z_2", this, new KeyboardShortcut(KeyCode.Y));

            KEY_OBJ_SCALE_ALL = new SavedKeyboardShortcut("KEY_OBJ_SCALE_ALL", this, new KeyboardShortcut(KeyCode.T));
            KEY_OBJ_SCALE_X = new SavedKeyboardShortcut("KEY_OBJ_SCALE_X", this, new KeyboardShortcut(KeyCode.G));
            KEY_OBJ_SCALE_Y = new SavedKeyboardShortcut("KEY_OBJ_SCALE_Y", this, new KeyboardShortcut(KeyCode.H));
            KEY_OBJ_SCALE_Z = new SavedKeyboardShortcut("KEY_OBJ_SCALE_Z", this, new KeyboardShortcut(KeyCode.Y));
        }

        void Awake()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void OnDestroy() // for ScriptEngine
        {
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        bool done = false;
        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(!done && FindObjectOfType<StudioScene>())
            {
                done = true;
                gameObject.AddComponent<ObjMoveRotAssistMgr>();
                //gameObject.AddComponent<FKIKAssistMgr>();
            }
        }
    }
}
