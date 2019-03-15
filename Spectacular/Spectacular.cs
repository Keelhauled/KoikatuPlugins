using BepInEx;
using Harmony;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Spectacular
{
    [BepInProcess("Koikatu")]
    [BepInPlugin(GUID, "Spectacular", Version)]
    public class Spectacular : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.spectacular";
        public const string Version = "1.0.0";

        HarmonyInstance harmony;

        void Start()
        {
            MakerAPI.RegisterCustomSubCategories += RegisterCustomSubCategories;
            harmony = HarmonyInstance.Create("keelhauled.spectacular.harmony");
            harmony.PatchAll(typeof(ClothingController));
        }

        void RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        {
            var categories = new List<MakerCategory>
            {
                MakerConstants.Clothes.Top,
                MakerConstants.Clothes.Bottom,
                MakerConstants.Clothes.Bra,
                MakerConstants.Clothes.Panst,
                MakerConstants.Clothes.Gloves,
                MakerConstants.Clothes.Shorts,
                MakerConstants.Clothes.InnerShoes,
                MakerConstants.Clothes.OuterShoes,
                MakerConstants.Clothes.Socks,
            };

            foreach(var category in categories)
            {
                var slider = new MakerSlider(category, "Specularity", 0f, 1f, 0f, this);
                slider.ValueChanged.Subscribe((val) => ClothingController.ChangeSpecularity(val));
                e.AddControl(slider);

                var color = new MakerColor("Spec color", false, category, Color.white, this);
                color.ValueChanged.Subscribe((val) => ClothingController.ChangeSpecColor(val));
                e.AddControl(color);
            }
        }
    }
}
