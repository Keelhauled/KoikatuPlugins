using BepInEx;
using Harmony;
using Studio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;

namespace PosePng
{
    [BepInProcess("CharaStudio")]
    [BepInPlugin("keelhauled.posepng", "PosePng", Version)]
    class PosePng : BaseUnityPlugin
    {
        public const string Version = "1.0.0";
        const string PngExt = ".png";
        HarmonyInstance harmony;

        void Start()
        {
            harmony = HarmonyInstance.Create("keelhauled.posepng.harmony");
            harmony.PatchAll(GetType());
        }

        #if DEBUG
        void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
        }
        #endif

        [HarmonyPrefix, HarmonyPatch(typeof(PauseCtrl), nameof(PauseCtrl.Save))]
        public static bool PoseSavePatch(OCIChar _ociChar, ref string _name)
        {
            var path = UserData.Create(PauseCtrl.savePath) + $"{_name}_{DateTime.Now.ToString("yyyy_MMdd_HHmm_ss_fff")}.png";
            var fileInfo = new PauseCtrl.FileInfo(_ociChar);
            using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using(var binaryWriter = new BinaryWriter(fileStream))
                {
                    var buffer = Studio.Studio.Instance.gameScreenShot.CreatePngScreen(320, 180, false, false);
                    binaryWriter.Write(buffer);
                    binaryWriter.Write(PauseCtrl.saveIdentifyingCode);
                    binaryWriter.Write(PauseCtrl.saveVersion);
                    binaryWriter.Write(_ociChar.oiCharInfo.sex);
                    binaryWriter.Write(_name);
                    fileInfo.Save(binaryWriter);
                }
            }

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PauseCtrl), nameof(PauseCtrl.Load))]
        public static bool PoseLoadPatch(OCIChar _ociChar, ref string _path, ref bool __result)
        {
            if(Path.GetExtension(_path).ToLower() == PngExt)
            {
                var fileInfo = new PauseCtrl.FileInfo(null);
                using(var fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using(var binaryReader = new BinaryReader(fileStream))
                    {
                        PngFile.SkipPng(binaryReader);

                        if(string.Compare(binaryReader.ReadString(), PauseCtrl.saveIdentifyingCode) != 0)
                        {
                            __result = false;
                            return false;
                        }

                        int ver = binaryReader.ReadInt32();
                        binaryReader.ReadInt32();
                        binaryReader.ReadString();
                        fileInfo.Load(binaryReader, ver);
                    }
                }

                fileInfo.Apply(_ociChar);
                __result = true;
                return false;
            }

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PauseCtrl), nameof(PauseCtrl.LoadName))]
        public static bool PoseLoadNamePatch(ref string _path, ref string __result)
        {
            if(Path.GetExtension(_path).ToLower() == PngExt)
            {
                using(var fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using(var binaryReader = new BinaryReader(fileStream))
                    {
                        PngFile.SkipPng(binaryReader);

                        if(string.Compare(binaryReader.ReadString(), PauseCtrl.saveIdentifyingCode) != 0)
                        {
                            __result = "";
                        }
                        else
                        {
                            binaryReader.ReadInt32();
                            binaryReader.ReadInt32();
                            __result = binaryReader.ReadString();
                        }
                    }
                }
                
                return false;
            }

            return true;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(PauseRegistrationList), "InitList")]
        public static IEnumerable<CodeInstruction> PoseListInitPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var magic = "System.Collections.Generic.List`1[System.String] listPath";
            
            for(int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if(code.opcode == OpCodes.Ldfld && code.operand.ToString() == magic)
                {
                    var method = AccessTools.Method(typeof(PosePng), nameof(AddPngToPauseRegistrationList));
                    Console.WriteLine(method);
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, method));
                    Console.WriteLine("Patched InitList");
                    break;
                }
            }

            return codes;
        }

        static void AddPngToPauseRegistrationList()
        {
            var pauseRegistrationList = FindObjectOfType<PauseRegistrationList>();
            var traverse = Traverse.Create(pauseRegistrationList);
            var sex = traverse.Field("m_OCIChar").GetValue<OCIChar>().oiCharInfo.sex;
            var pngFiles = Directory.GetFiles(UserData.Create(PauseCtrl.savePath), $"*{PngExt}").Where(x => CheckIdentifyingCode(x, sex));
            var listPath = traverse.Field("listPath").GetValue<List<string>>();
            foreach(var item in pngFiles)
            {
                Console.WriteLine(item);
                listPath.Add(item);
            }

            Console.WriteLine("Png added to PauseRegistrationList");
        }

        public static bool CheckIdentifyingCode(string _path, int _sex)
        {
            using(var fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using(var binaryReader = new BinaryReader(fileStream))
                {
                    PngFile.SkipPng(binaryReader);

                    if(string.Compare(binaryReader.ReadString(), PauseCtrl.saveIdentifyingCode) != 0)
                        return false;

                    binaryReader.ReadInt32();

                    if(_sex != binaryReader.ReadInt32())
                        return false;
                }
            }

            return true;
        }
    }
}
