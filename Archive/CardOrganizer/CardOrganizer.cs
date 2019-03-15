using BepInEx;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardOrganizer
{
    [BepInPlugin("com.keelhauled.cardorganizer", "CardOrganizer", "1.0.0")]
    class CardOrganizer : BaseUnityPlugin
    {
        internal const string configName = "CardOrganizer";
        const string objectName = "CardOrganizerGameObject";

        void Awake()
        {
            SceneManager.sceneLoaded += (scene, mode) => StartMod();
        }

        static void StartMod()
        {
            if(!GameObject.Find(objectName))
            {
                switch(SceneManager.GetActiveScene().name)
                {
                    case "Studio":
                        new GameObject(objectName).AddComponent<StudioLoaderUI>();
                        break;

                    case "CustomScene":
                        new GameObject(objectName).AddComponent<CharaMaker>();
                        break;

                    case "FreeH":
                        //if(GameObject.Find("FreeHCharaSelect")) new GameObject(objectName).AddComponent<>();
                        break;
                }
            }
        }

        public static void Bootstrap()
        {
            var gameobject = GameObject.Find(objectName);
            if(gameobject) DestroyImmediate(gameobject);
            StartMod();
        }
    }
}
