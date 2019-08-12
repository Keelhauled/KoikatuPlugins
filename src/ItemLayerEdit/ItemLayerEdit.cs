using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using SharedPluginCode;
using UILib;
using Studio;

namespace ItemLayerEdit
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin(GUID, "Item Layer Edit", Version)]
    public class ItemLayerEdit : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.itemlayeredit";
        public const string Version = "1.0.0";

        GameObject panel;
        GameObject targetObject;

        void Awake()
        {
            for(int i = 0; i < 31; i++)
            {
                Console.WriteLine(LayerMask.LayerToName(i));
            }

            Studio.Studio.Instance.treeNodeCtrl.onSelect += OnSelect;
            Studio.Studio.Instance.treeNodeCtrl.onDelete += OnDelete;

            var panelTemplate = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/01_Item/Image FK");
            var buttonTemplate = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/01_Item/Image Color1 Background/Button Color Default");

            panel = Instantiate(panelTemplate, panelTemplate.transform.parent, true);
            panel.name = "ItemLayerEdit";
            panel.transform.localScale = Vector3.one;
            foreach(Transform child in panel.transform)
                Destroy(child.gameObject);

            var buttonObject = Instantiate(buttonTemplate, panel.transform, true);
            buttonObject.name = "CharaButton";
            buttonObject.transform.SetRect(0.25f, 0.5f, 0.25f, 0.5f, -40f, -20f, 40f, 20f);
            buttonObject.GetComponent<Button>().onClick.AddListener(() => SetTargetObjectLayer("Chara"));

            var buttonObject2 = Instantiate(buttonTemplate, panel.transform, true);
            buttonObject2.name = "MapButton";
            buttonObject2.transform.SetRect(0.75f, 0.5f, 0.75f, 0.5f, -40f, -20f, 40f, 20f);
            buttonObject2.GetComponent<Button>().onClick.AddListener(() => SetTargetObjectLayer("Map"));
            
            //try { throw new ArithmeticException(); } catch{}
        }

        void OnDestroy()
        {
            DestroyImmediate(panel);
            Studio.Studio.Instance.treeNodeCtrl.onSelect -= OnSelect;
            Studio.Studio.Instance.treeNodeCtrl.onDelete -= OnDelete;
        }

        void OnSelect(TreeNodeObject treeNodeObject)
        {
            if(Studio.Studio.Instance.dicInfo.TryGetValue(treeNodeObject, out var objectCtrlInfo) && objectCtrlInfo.kind == 1)
            {
                targetObject = Traverse.Create(objectCtrlInfo).Field("objectItem").GetValue<GameObject>();
                Console.WriteLine($"{targetObject.name} is in the {LayerMask.LayerToName(targetObject.layer)} ({targetObject.layer}) layer.");
            }
        }

        void OnDelete(TreeNodeObject treeNodeObject)
        {
            targetObject = null;
        }

        void SetTargetObjectLayer(string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            targetObject.layer = layer;
            foreach(Transform child in targetObject.transform)
                child.gameObject.layer = layer;

            Console.WriteLine($"{targetObject.name} is now in the {layerName} ({layer}) layer.");
        }
    }
}
