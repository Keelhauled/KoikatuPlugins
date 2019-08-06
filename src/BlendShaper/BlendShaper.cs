using BepInEx;
using Studio;
using System.Collections.Generic;
using UILib;
using UnityEngine;
using UnityEngine.UI;
using SharedPluginCode;

//cf_O_canine
//cf_O_face
//cf_O_mayuge
//cf_O_noseline
//cf_O_tooth
//cf_O_eyeline
//cf_O_eyeline_low
//cf_O_namida_L
//cf_O_namida_M
//cf_O_namida_S
//cf_Ohitomi_L
//cf_Ohitomi_R
//o_tang

namespace BlendShaper
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin("keelhauled.blendshaper", "BlendShaper", Version)]
    class BlendShaper : BaseUnityPlugin
    {
        public const string Version = "1.0.0";

        float UIScale = 1.0f;
        float elementSize = 20f;

        Canvas UISystem;
        ScrollRect categoryList;
        ScrollRect blendSetList;
        BlendSets blendSets = BlendSets.LoadBlendSetData();

        void Start()
        {
            UIUtility.Init(nameof(BlendShaper));

            var panel = CreateUIBase(400f, 400f);
            categoryList = CreateList(panel.transform, 0f, 0f, 0.25f, 1f);
            blendSetList = CreateList(panel.transform, 0.25f, 0f, 1f, 1f);

            if(Studio.Studio.Instance.treeNodeCtrl.selectNode)
                UpdateUI(Studio.Studio.Instance.treeNodeCtrl.selectNode);

            Studio.Studio.Instance.treeNodeCtrl.onSelect += UpdateUI;
            Studio.Studio.Instance.treeNodeCtrl.onDelete += ClearUI;
        }

        void OnDestroy()
        {
            Destroy(UISystem);
            Studio.Studio.Instance.treeNodeCtrl.onSelect -= UpdateUI;
            Studio.Studio.Instance.treeNodeCtrl.onDelete -= ClearUI;
        }

        void LateUpdate()
        {
            foreach(var category in blendSets.Categories)
            {
                foreach(var set in category.Sets)
                    set.Execute();
            }
        }

        void UpdateUI(TreeNodeObject node)
        {
            ClearUI(node);

            if(Studio.Studio.Instance.dicInfo.TryGetValue(node, out var objectCtrlInfo))
            {
                if(objectCtrlInfo is OCIChar chara)
                {
                    foreach(var category in blendSets.Categories)
                    {
                        var button = UIUtility.CreateButton("Button", categoryList.content.transform, category.Name);
                        button.gameObject.AddComponent<LayoutElement>().preferredHeight = elementSize;
                        button.onClick.AddListener(() => UpdateBlend(category.Sets, chara));
                    }
                }
            }
        }

        void UpdateBlend(List<BlendSets.Set> sets, OCIChar chara)
        {
            foreach(var button in blendSetList.content.GetComponentsInChildren<Image>())
            {
                button.gameObject.SetActive(false);
                Destroy(button);
            }

            foreach(var set in sets)
            {
                set.CreateActions(chara);

                var panel = UIUtility.CreatePanel("ShapePanel", blendSetList.content.transform);
                panel.gameObject.AddComponent<LayoutElement>().preferredHeight = elementSize;
                panel.color = new Color(1f, 1f, 1f, 1f);

                var text = UIUtility.CreateText("ShapeName", panel.transform, set.Name);
                text.transform.SetRect(0.01f, 0.01f, 0.44f, 0.99f);
                text.color = Color.black;
                text.alignment = TextAnchor.MiddleCenter;

                var slider = UIUtility.CreateSlider("ShapeSlider", panel.transform);
                slider.transform.SetRect(0.45f, 0.01f, 0.99f, 0.99f);
                slider.maxValue = 100f;
                slider.minValue = 0f;
                slider.value = set.Value;
                slider.onValueChanged.AddListener((value) => set.ChangeValue(value));
            }
        }

        void ClearUI(TreeNodeObject node)
        {
            foreach(var button in categoryList.content.GetComponentsInChildren<Button>())
            {
                button.gameObject.SetActive(false);
                Destroy(button);
            }

            foreach(var button in blendSetList.content.GetComponentsInChildren<Image>())
            {
                button.gameObject.SetActive(false);
                Destroy(button);
            }
        }

        Image CreateUIBase(float width, float height)
        {
            var margin = 4f;
            var title = 25f;

            UISystem = UIUtility.CreateNewUISystem("BlendShaperUI");
            UISystem.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f / UIScale, 1080f / UIScale);
            UISystem.transform.SetParent(gameObject.transform);

            var bg = UIUtility.CreatePanel("Background", UISystem.transform);
            bg.transform.SetRect(0.5f, 0.2f, 0.5f, 0.2f, -width / 2f, -height / 2f, width / 2f, height / 2f);
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 1f);

            var drag = UIUtility.CreatePanel("Drag", bg.transform);
            drag.transform.SetRect(0f, 1f, 1f, 1f, margin, -title, -margin, -margin);
            UIUtility.MakeObjectDraggable(drag.rectTransform, bg.rectTransform);
            drag.color = Color.gray;

            var text = UIUtility.CreateText("Text", drag.transform, "BlendShaper");
            text.transform.SetRect(0f, 0f, 1f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 16;

            var panel = UIUtility.CreatePanel("Panel", bg.transform);
            panel.transform.SetRect(0f, 0f, 1f, 1f, margin, margin, -margin, -title);
            panel.color = new Color(0f, 0f, 0f, 0f);

            return panel;
        }

        ScrollRect CreateList(Transform parent, float anchorLeft, float anchorBottom, float anchorRight, float anchorTop)
        {
            float scrollOffsetX = -15f;

            var scroll = UIUtility.CreateScrollView("List", parent);
            scroll.transform.SetRect(anchorLeft, anchorBottom, anchorRight, anchorTop);
            scroll.gameObject.AddComponent<Mask>();
            scroll.content.gameObject.AddComponent<VerticalLayoutGroup>();
            scroll.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.verticalScrollbar.GetComponent<RectTransform>().offsetMin = new Vector2(scrollOffsetX, 0f);
            scroll.viewport.offsetMax = new Vector2(scrollOffsetX, 0f);
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 20f;
            return scroll;
        }
    }
}
