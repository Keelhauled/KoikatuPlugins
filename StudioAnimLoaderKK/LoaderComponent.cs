using BepInEx;
using BepInEx.Logging;
using Studio;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace StudioAnimLoader
{
    public class LoaderComponent : MonoBehaviour
    {
        private const string fileSuffixGrpF = "FemaleAnimeGroup_";
        private const string fileSuffixCatF = "FemaleAnimeCategory_";
        private const string fileSuffixAnimF = "FemaleAnime_";
        private const string fileSuffixHAnimF = "FemaleHAnime_";
        private const string fileSuffixGrpM = "MaleAnimeGroup_";
        private const string fileSuffixCatM = "MaleAnimeCategory_";
        private const string fileSuffixAnimM = "MaleAnime_";
        private const string fileSuffixHAnimM = "MaleHAnime_";
        private const string fileSuffixVoiceGroup = "VoiceGroup_";
        private const string fileSuffixVoiceCategory = "VoiceCategory_";
        private const string fileSuffixVoice = "Voice_";

        private Dictionary<string, Dictionary<string, List<string[]>>> dicAllFileArgs = new Dictionary<string, Dictionary<string, List<string[]>>>();
        private Info info;
        private int groupOffset;
        private bool force;
        private string dir;
        private string extDir;
        private string groupSuffix;

        private void Start()
        {
            Hooks.Patch();

            dir = Path.Combine(Paths.PluginPath, "StudioAnimLoader");
            extDir = Path.Combine(StudioAnimLoader.OtherGameDir.Value, "abdata");
            groupOffset = StudioAnimLoader.GroupOffset.Value;
            groupSuffix = StudioAnimLoader.GroupSuffix.Value;
            force = StudioAnimLoader.Overwrite.Value;
            info = Info.Instance;

            if(!Directory.Exists(dir) || !Directory.Exists(StudioAnimLoader.OtherGameDir.Value))
            {
                Logger.Log(LogLevel.Message, "StudioAnimLoader Aborted: Problem with InfoDir or OtherGameDir settings.");
                return;
            }

            LoadAll();
        }

        private void OnDestroy()
        {
            Hooks.Unpatch();
        }

        private void LoadAll()
        {
            Logger.Log(LogLevel.Debug, "StudioAnimLoader: Animation Load Start");

            LoadFiles();
            LoadGroup(AnimeGroupList.SEX.Female, true);
            LoadGroup(0, true);
            LoadCategory(AnimeGroupList.SEX.Female, true);
            LoadCategory(0, true);
            LoadAnim(AnimeGroupList.SEX.Female, false);
            LoadAnim(0, false);
            LoadAnim(AnimeGroupList.SEX.Female, true);
            LoadAnim(0, true);
            LoadGroup(AnimeGroupList.SEX.Female, false);
            LoadCategory(AnimeGroupList.SEX.Female, false);
            LoadVoice();
            dicAllFileArgs = null;

            Logger.Log(LogLevel.Debug, "StudioAnimLoader: Animation Load Complete");
        }

        private void LoadFiles()
        {
            string[] array = new string[]
            {
                "FemaleAnimeGroup_",
                "MaleAnimeGroup_",
                "FemaleAnimeCategory_",
                "MaleAnimeCategory_",
                "FemaleAnime_",
                "MaleAnime_",
                "FemaleHAnime_",
                "MaleHAnime_",
                "VoiceGroup_",
                "VoiceCategory_",
                "Voice_"
            };

            for(int i = 0; i < array.Length; i++)
            {
                dicAllFileArgs.Add(array[i], _LoadFiles(dir, array[i] + "*.MonoBehaviour"));
            }
        }

        private string[] ParseSB3UList(string text)
        {
            string[] result;
            try
            {
                result = text.Replace(">", "").Remove(0, 1).Split(new string[] { "<" }, StringSplitOptions.None);
            }
            catch
            {
                result = null;
            }

            return result;
        }

        private Dictionary<string, List<string[]>> _LoadFiles(string dir, string pattern)
        {
            Dictionary<string, List<string[]>> dictionary = new Dictionary<string, List<string[]>>();

            string[] files = Directory.GetFiles(dir, pattern);
            if(files == null)
                return null;

            foreach(string path in files)
            {
                if(File.Exists(path))
                {
                    List<string[]> list = new List<string[]>();
                    using(StreamReader streamReader = File.OpenText(path))
                    {
                        string text;
                        while((text = streamReader.ReadLine()) != null)
                        {
                            try
                            {
                                string[] array2 = ParseSB3UList(text);
                                if(array2 != null && int.TryParse(array2[0], out int num) && array2.Length > 1)
                                {
                                    list.Add(array2);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                    dictionary.Add(Path.GetFileNameWithoutExtension(path), list);
                }
            }

            return dictionary;
        }

        private void LoadGroup(AnimeGroupList.SEX sex, bool isAnim)
        {
            Dictionary<int, Info.GroupInfo> dictionary;
            string key;
            if(sex != null)
            {
                if(sex != AnimeGroupList.SEX.Female)
                    return;

                dictionary = info.dicAGroupCategory;
                key = "FemaleAnimeGroup_";
            }
            else
            {
                dictionary = info.dicAGroupCategory;
                key = "MaleAnimeGroup_";
            }

            if(!isAnim)
            {
                dictionary = info.dicVoiceGroupCategory;
                key = "VoiceGroup_";
            }

            Dictionary<string, List<string[]>> dictionary2 = dicAllFileArgs[key];
            if(dictionary2 == null)
                return;

            foreach(string key2 in dictionary2.Keys)
            {
                foreach(string[] array in dictionary2[key2])
                {
                    if(int.TryParse(array[0], out int num))
                    {
                        int key3 = num + groupOffset;
                        if(dictionary.ContainsKey(key3))
                        {
                            if(force)
                            {
                                dictionary[key3].name = groupSuffix + array[1];
                            }
                        }
                        else
                        {
                            dictionary.Add(key3, new Info.GroupInfo
                            {
                                name = groupSuffix + array[1],
                                dicCategory = new Dictionary<int, string>()
                            });
                        }
                    }
                }
            }
        }

        private void LoadCategory(AnimeGroupList.SEX sex, bool isAnim)
        {
            Dictionary<int, Info.GroupInfo> dictionary;
            string text;
            if(sex != null)
            {
                if(sex != AnimeGroupList.SEX.Female)
                    return;

                dictionary = info.dicAGroupCategory;
                text = "FemaleAnimeCategory_";
            }
            else
            {
                dictionary = info.dicAGroupCategory;
                text = "MaleAnimeCategory_";
            }

            if(!isAnim)
            {
                dictionary = info.dicVoiceGroupCategory;
                text = "VoiceCategory_";
            }

            Dictionary<string, List<string[]>> dictionary2 = dicAllFileArgs[text];
            if(dictionary2 == null)
                return;

            foreach(string text2 in dictionary2.Keys)
            {
                if(int.TryParse(text2.Replace(text, "").Split(new string[] { "_" }, StringSplitOptions.None)[0], out int num))
                {
                    int key = num + groupOffset;
                    foreach(string[] array in dictionary2[text2])
                    {
                        if(int.TryParse(array[0], out int key2) && dictionary.ContainsKey(key))
                        {
                            if(dictionary[key].dicCategory.ContainsKey(key2))
                            {
                                if(force)
                                {
                                    dictionary[key].dicCategory[key2] = array[1];
                                }
                            }
                            else
                            {
                                dictionary[key].dicCategory.Add(key2, array[1]);
                            }
                        }
                    }
                }
            }
        }

        private void LoadAnim(AnimeGroupList.SEX sex, bool isH)
        {
            Dictionary<int, Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>> dictionary;
            string text;
            if(sex != null)
            {
                if(sex != AnimeGroupList.SEX.Female)
                    return;

                dictionary = info.dicAnimeLoadInfo;
                text = isH ? "FemaleHAnime_" : "FemaleAnime_";
            }
            else
            {
                dictionary = info.dicAnimeLoadInfo;
                text = isH ? "MaleHAnime_" : "MaleAnime_";

            }

            Dictionary<string, List<string[]>> dictionary2 = dicAllFileArgs[text];
            if(dictionary2 == null)
                return;

            foreach(string text2 in dictionary2.Keys)
            {
                string[] array = text2.Replace(text, "").Split(new string[] { "_" }, StringSplitOptions.None);

                if(int.TryParse(array[0], out int num) && int.TryParse(array[1], out int key))
                {
                    int key2 = num + groupOffset;
                    foreach(string[] array2 in dictionary2[text2])
                    {
                        if(int.TryParse(array2[0], out int key3))
                        {
                            if(!dictionary.ContainsKey(key2))
                            {
                                dictionary.Add(key2, new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>());
                            }

                            if(!dictionary[key2].ContainsKey(key))
                            {
                                dictionary[key2].Add(key, new Dictionary<int, Info.AnimeLoadInfo>());
                            }

                            Info.AnimeLoadInfo value = null;
                            try
                            {
                                value = new Info.AnimeLoadInfo
                                {
                                    name = array2[3],
                                    bundlePath = Path.Combine(extDir, array2[4]),
                                    fileName = array2[5],
                                    clip = array2[6],
                                    //isBreastLayer = (array2[7] == "True"),
                                    //isMotion = true,
                                    //isHAnime = isH,
                                    //isScale = false
                                };
                            }
                            catch
                            {
                                value = null;
                            }

                            if(dictionary[key2][key].ContainsKey(key3))
                            {
                                if(force)
                                {
                                    dictionary[key2][key][key3] = value;
                                }
                            }
                            else
                            {
                                dictionary[key2][key].Add(key3, value);
                            }
                        }
                    }
                }
            }
        }

        private void LoadVoice()
        {
            Dictionary<int, Dictionary<int, Dictionary<int, Info.LoadCommonInfo>>> dicVoiceLoadInfo = info.dicVoiceLoadInfo;
            string text = "Voice_";

            Dictionary<string, List<string[]>> dictionary = dicAllFileArgs[text];
            if(dictionary == null)
                return;

            foreach(string text2 in dictionary.Keys)
            {
                string[] array = text2.Replace(text, "").Split(new string[] { "_" }, StringSplitOptions.None);

                if(int.TryParse(array[0], out int num) && int.TryParse(array[1], out int key))
                {
                    int key2 = num + groupOffset;
                    foreach(string[] array2 in dictionary[text2])
                    {
                        if(int.TryParse(array2[0], out int key3))
                        {
                            if(!dicVoiceLoadInfo.ContainsKey(key2))
                            {
                                dicVoiceLoadInfo.Add(key2, new Dictionary<int, Dictionary<int, Info.LoadCommonInfo>>());
                            }

                            if(!dicVoiceLoadInfo[key2].ContainsKey(key))
                            {
                                dicVoiceLoadInfo[key2].Add(key, new Dictionary<int, Info.LoadCommonInfo>());
                            }

                            Info.LoadCommonInfo value = null;
                            try
                            {
                                value = new Info.LoadCommonInfo
                                {
                                    name = array2[3],
                                    bundlePath = Path.Combine(extDir, array2[4]),
                                    fileName = array2[5]
                                };
                            }
                            catch
                            {
                                value = null;
                            }

                            if(dicVoiceLoadInfo[key2][key].ContainsKey(key3))
                            {
                                if(force)
                                {
                                    dicVoiceLoadInfo[key2][key][key3] = value;
                                }
                            }
                            else
                            {
                                dicVoiceLoadInfo[key2][key].Add(key3, value);
                            }
                        }
                    }
                }
            }
        }
    }
}
