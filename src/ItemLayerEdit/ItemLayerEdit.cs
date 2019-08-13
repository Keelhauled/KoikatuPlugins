using BepInEx;
using Harmony;
using SharedPluginCode;
using Studio;
using System;
using System.Collections.Generic;
using TMPro;
using UILib;
using UnityEngine;
using UnityEngine.UI;

namespace ItemLayerEdit
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin(GUID, "Item Layer Edit", Version)]
    public class ItemLayerEdit : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.itemlayeredit";
        public const string Version = "1.0.0";

        static HarmonyInstance harmony;
        static GameObject panel;
        static List<GameObject> targetObjects = new List<GameObject>();
        static Slider layerSliderComponent;
        static TMP_InputField layerInputComponent;
        static bool pluginSetup = false;

        void Awake()
        {
            harmony = HarmonyInstance.Create($"{GUID}.harmony");
            harmony.PatchAll(GetType());
            //SetupStudio();
        }

#if DEBUG
        void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
            DestroyImmediate(panel);
            Studio.Studio.Instance.treeNodeCtrl.onSelect -= OnTreeNodeCtrlChange;
            Studio.Studio.Instance.treeNodeCtrl.onSelectMultiple -= OnSelectMultiple;
            Studio.Studio.Instance.treeNodeCtrl.onDeselect -= OnTreeNodeCtrlChange;
            Studio.Studio.Instance.treeNodeCtrl.onDelete -= OnTreeNodeCtrlChange;
        }
#endif

        [HarmonyPostfix, HarmonyPatch(typeof(ManipulatePanelCtrl), "OnSelect")]
        public static void Entrypoint() => SetupStudio();

        static void SetupStudio()
        {
            if(pluginSetup)
                return;

            Studio.Studio.Instance.treeNodeCtrl.onSelect += OnTreeNodeCtrlChange;
            Studio.Studio.Instance.treeNodeCtrl.onSelectMultiple += OnSelectMultiple;
            Studio.Studio.Instance.treeNodeCtrl.onDeselect += OnTreeNodeCtrlChange;
            Studio.Studio.Instance.treeNodeCtrl.onDelete += OnTreeNodeCtrlChange;

            var panelTemplate = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/01_Item/Image Color1 Background");
            var textTemplate = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/01_Item/Image Line/TextMeshPro Width");
            var sliderTemplate = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/01_Item/Image Line/Slider Width");
            var inputTemplate = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/01_Item/Image Line/TextMeshPro - InputField Width");
            var defButtonTemplate = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/01_Item/Image Line/Button Width Default");

            panel = Instantiate(panelTemplate, panelTemplate.transform.parent, true);
            panel.name = "ItemLayerEdit";
            panel.transform.localScale = Vector3.one;
            foreach(Transform child in panel.transform)
                Destroy(child.gameObject);

            var layerTextObject = Instantiate(textTemplate, panel.transform, true);
            layerTextObject.name = "LayerTextObject";
            layerTextObject.transform.SetRect(0.01f, 0.1f, 0.13f, 0.9f);
            var layerTextComponent = layerTextObject.GetComponent<TextMeshProUGUI>();
            layerTextComponent.text = "Layer";

            var layerSliderObject = Instantiate(sliderTemplate, panel.transform, true);
            layerSliderObject.name = "LayerSliderObject";
            layerSliderObject.transform.SetRect(0.15f, 0.35f, 0.71f, 0.65f);
            layerSliderComponent = layerSliderObject.GetComponent<Slider>();
            layerSliderComponent.wholeNumbers = true;
            layerSliderComponent.minValue = 0;
            layerSliderComponent.maxValue = 30;

            var layerInputObject = Instantiate(inputTemplate, panel.transform, true);
            layerInputObject.name = "LayerInputObject";
            layerInputObject.transform.SetRect(0.73f, 0.15f, 0.84f, 0.85f);
            layerInputComponent = layerInputObject.GetComponent<TMP_InputField>();

            var layerDefButtonObject = Instantiate(defButtonTemplate, panel.transform, true);
            layerDefButtonObject.name = "LayerDefButtonObject";
            layerDefButtonObject.transform.SetRect(0.86f, 0.15f, 0.97f, 0.85f);
            var layerDefButtonComponent = layerDefButtonObject.GetComponent<Button>();

            layerSliderComponent.onValueChanged.AddListener((x) =>
            {
                SetTargetObjectLayer((int)x);
                layerInputComponent.text = x.ToString();
            });

            layerInputComponent.onValueChanged.AddListener((x) =>
            {
                if(int.TryParse(x, out int result))
                {
                    SetTargetObjectLayer(result);
                    layerSliderComponent.value = result;
                }
            });

            pluginSetup = true;

            try { throw new ArithmeticException(); } catch { }
        }

        static void OnTreeNodeCtrlChange(TreeNodeObject treeNodeObject) => UpdateTargetObjects();
        static void OnSelectMultiple() => UpdateTargetObjects();

        static void UpdateTargetObjects()
        {
            targetObjects.Clear();

            foreach(var objectCtrl in Studio.Studio.GetSelectObjectCtrl())
            {
                if(objectCtrl.kind == 1)
                {
                    var targetObject = Traverse.Create(objectCtrl).Field("objectItem").GetValue<GameObject>();
                    targetObjects.Add(targetObject);
                }
            }

            if(targetObjects.Count > 0)
            {
                layerSliderComponent.value = targetObjects[0].layer;
                layerInputComponent.text = targetObjects[0].layer.ToString();
            }
        }

        static void SetTargetObjectLayer(int layer)
        {
            foreach(var targetObject in targetObjects)
            {
                targetObject.layer = layer;
                foreach(Transform child in targetObject.transform)
                    child.gameObject.layer = layer;

                Console.WriteLine($"{targetObject.name} is now in the {LayerMask.LayerToName(layer)} ({layer}) layer.");
            }
        }
    }
}
