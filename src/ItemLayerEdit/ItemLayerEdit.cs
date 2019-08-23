﻿using BepInEx;
using Harmony;
using KKAPI.Studio.SaveLoad;
using SharedPluginCode;
using Studio;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        static List<GameObject> targetObjects = new List<GameObject>();
        static Slider layerSliderComponent;
        static TMP_InputField layerInputComponent;
        static bool pluginSetup = false;

        void Awake()
        {
            harmony = HarmonyInstance.Create($"{GUID}.harmony");
            harmony.PatchAll(GetType());
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneDataController>(GUID);
        }

#if DEBUG
        void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
            DestroyImmediate(panel);

            var studio = Studio.Studio.Instance;
            studio.treeNodeCtrl.onSelect -= OnTreeNodeCtrlChange;
            studio.treeNodeCtrl.onSelectMultiple -= OnSelectMultiple;
            studio.treeNodeCtrl.onDeselect -= OnTreeNodeCtrlChange;
            studio.treeNodeCtrl.onDelete -= OnTreeNodeCtrlChange;
        }
#endif

        [HarmonyPostfix, HarmonyPatch(typeof(ManipulatePanelCtrl), "OnSelect")]
        public static void Entrypoint() => SetupStudio();

        static void SetupStudio()
        {
            if(pluginSetup)
                return;

            var studio = Studio.Studio.Instance;
            studio.treeNodeCtrl.onSelect += OnTreeNodeCtrlChange;
            studio.treeNodeCtrl.onSelectMultiple += OnSelectMultiple;
            studio.treeNodeCtrl.onDeselect += OnTreeNodeCtrlChange;
            studio.treeNodeCtrl.onDelete += OnTreeNodeCtrlChange;

            var panel = LayerUIBackend.CreatePanel();
            LayerUIBackend.CreateText(panel.transform);
            layerSliderComponent = LayerUIBackend.CreateSlider(panel.transform);
            layerInputComponent = LayerUIBackend.CreateInputfield(panel.transform);
            var layerDefButtonComponent = LayerUIBackend.CreateButton(panel.transform);

            layerDefButtonComponent.onClick.AddListener(() =>
            {
                SetTargetObjectLayersDefault();

                var defaultLayer = targetObjects.First().layer;
                layerInputComponent.text = defaultLayer.ToString();
                layerSliderComponent.value = defaultLayer;
            });

            layerSliderComponent.onValueChanged.AddListener((x) =>
            {
                SetTargetObjectLayers((int)x);
                layerInputComponent.text = x.ToString();
            });

            layerInputComponent.onValueChanged.AddListener((x) =>
            {
                if(int.TryParse(x, out int result))
                {
                    SetTargetObjectLayers(result);
                    layerSliderComponent.value = result;
                }
            });

            UpdateTargetObjects();
            pluginSetup = true;
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
                    var target = Traverse.Create(objectCtrl).Field("objectItem").GetValue<GameObject>();
                    targetObjects.Add(target);
                }
            }

            if(targetObjects.Count > 0)
            {
                layerSliderComponent.value = targetObjects[0].layer;
                layerInputComponent.text = targetObjects[0].layer.ToString();
            }
        }

        static void SetTargetObjectLayers(int layer)
        {
            foreach(var targetObject in targetObjects)
            {
                if(targetObject.AddComponentIfNotExist<LayerDataContainer>(out var data))
                {
                    data = targetObject.AddComponent<LayerDataContainer>();
                    data.DefaultLayer = targetObject.layer;
                }

                targetObject.SetLayers(layer);
            }
        }

        static void SetTargetObjectLayersDefault()
        {
            foreach(var targetObject in targetObjects)
            {
                var data = targetObject.GetComponent<LayerDataContainer>();
                if(data && targetObject.layer != data.DefaultLayer)
                    targetObject.SetLayers(data.DefaultLayer);
            }
        }
    }
}
