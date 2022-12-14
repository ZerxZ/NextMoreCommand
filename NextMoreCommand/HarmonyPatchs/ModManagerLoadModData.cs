using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JSONClass;
using KBEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkySwordKill.Next;
using SkySwordKill.Next.DialogEvent;
using SkySwordKill.Next.DialogSystem;
using SkySwordKill.Next.Extension;
using SkySwordKill.Next.Mod;
using SkySwordKill.NextMoreCommand.Custom;
using SkySwordKill.NextMoreCommand.Custom.NPC;
using SkySwordKill.NextMoreCommand.Utils;

namespace SkySwordKill.NextMoreCommand.HarmonyPatchs;

[HarmonyPatch(typeof(ModManager), "LoadModData")]
public static class ModManagerLoadModData
{
    public static bool HasPath(this string path) => Directory.Exists(path);
    public static string CombinePath(this string path, string folder) => Path.Combine(path, folder);

    public static bool HasDirectory(params string[] paths)
    {
        var result = false;
        foreach (var path in paths)
        {
            if (path.HasPath())
            {
                result = true;
            }
        }

        return result;
    }

    private static void CreateTemplateTest(string dir)
    {
        Main.LogInfo("创建警花测试");
        var npc = new CustomNpc()
        {
            Id = 8000,
            Name = "天宫警花",
            Title = "天宫仙子",
            SexType = 2,
            CustomWujiang = new List<CustomWujiang>() { new CustomWujiang(), new CustomWujiang() },
            CustomNpcImportantDate = new CustomNpcImportantDate(),
            CustomBackPack = new CustomBackPack(),
        };
        npc.CustomNpcAvatar.Add(new CustomNpcAvatar());
        npc.CustomNpcAvatar.Add(new CustomNpcAvatar());
        npc.Init();
        //npc.Init();
        File.WriteAllText(dir.CombinePath("天宫警花.json"),
            JObject.FromObject(npc).ToString(Formatting.Indented));
        // MiniExcel.SaveAsAsync(dir.CombinePath("CustomNpc.xlsx"), new List<CustomNpc> { npc }, excelType: ExcelType.XLSX,
        //  overwriteFile: true);
    }


    public static void Prefix(ModConfig modConfig)
    {
        var modNDataDirDir = modConfig.GetNDataDir();

        if (HasPath(modNDataDirDir.CombinePath("CustomNpc")))
        {
            // if (modConfig.Name.Contains("天宫镜花")) CreateTemplateTest(modNDataDirDir.CombinePath("CustomNpc"));
            Main.LogInfo($"=================== NextMore开始加载 =====================");
            Main.LogIndent = 2;
            try
            {
                LoadCustomNpcData(modNDataDirDir, modConfig);
            }
            catch (Exception)
            {
                modConfig.State = ModState.LoadFail;
                throw;
            }

            modConfig.State = ModState.LoadSuccess;
            Main.LogIndent = 0;
           

            Main.LogInfo($"=================== NextMore结束加载 =====================");
        }
    }

    public static void LoadData(string modDir, string folder, ModConfig modConfig, Action<string, ModConfig> onComplete)
    {
        if (HasPath(modDir.CombinePath(folder)))
        {
            onComplete.Invoke(modDir.CombinePath(folder), modConfig);
        }
    }

    // ReSharper disable once HeapView.DelegateAllocation
    public static void LoadCustomNpcData(string modDir, ModConfig modConfig) =>
        LoadData(modDir, "CustomNpc", modConfig, LoadCustomNpc);

    public static void LoadCustomNpc(string path, ModConfig modConfig)
    {
        var customNpcs = new Dictionary<string, CustomNpc>();

        foreach (var filePath in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                string json = File.ReadAllText(filePath);

                var npc = JObject.Parse(json)?.ToObject<CustomNpc>();
                if (npc == null) continue;
                if (npc.Id >= 20000)
                {
                    Main.LogWarning($"NpcId:{npc.Id.ToString()} 已经超过2万值 建议改小2万以内值");
                    continue;
                }
                var id = npc.Id.ToString();
                customNpcs[id] = npc;
                CustomNpc.CustomNpcs[id] = npc;
                Main.LogInfo(string.Format("ModManager.LoadData".I18N(),
                    filePath));
            }
            catch (Exception e)
            {
                throw new ModLoadException(string.Format("CustomNpc {0} 加载失败。".I18NTodo(), filePath), e);
            }
        }

        SaveCustomNpc(customNpcs, modConfig);
    }

    delegate void SaveJsonAction(KeyValuePair<string, CustomNpc> npc, Dictionary<string, JObject> dict);

    private static void SaveCustomNpc(Dictionary<string, CustomNpc> customNpcs, ModConfig modConfig)
    {
        if (customNpcs.Count == 0)
        {
            return;
        }

        var modData = modConfig.GetDataDir();
        SaveJson(modData.CombinePath("AvatarJsonData.json"), customNpcs, SaveAvatar);
        SaveJson(modData.CombinePath("NPCImportantDate.json"), customNpcs, SaveNpcImportant);
        SaveJson(modData.CombinePath("WuJiangBangDing.json"), customNpcs, SaveWuJiang);
        SaveJson(modData.CombinePath("BackpackJsonData.json"), customNpcs, SaveBackPack);
    }

    private static void SaveAvatar(KeyValuePair<string, CustomNpc> npc, Dictionary<string, JObject> dict)
    {
        if (npc.Value.CustomNpcAvatar.Count == 0)
        {
            return;
        }

        var list = npc.Value.ToAvatarJsonDataList();
        foreach (var item in list)
        {
            dict[item.Id.ToString()] = item.ToJObject();
        }
    }

    private static void SaveNpcImportant(KeyValuePair<string, CustomNpc> npc, Dictionary<string, JObject> dict)
    {
     
        var value = npc.Value.ToNpcImportantDate();
        if (value != null)
        {
            dict[npc.Key] = value;
        }
    }


    private static void SaveWuJiang(KeyValuePair<string, CustomNpc> npc, Dictionary<string, JObject> dict)
    {
        var count = npc.Value.CustomWujiang?.Count;
        if (count is 0 or null)
        {
            return;
        }
        
        foreach (var value in npc.Value.ToWuJiangBindingList())
        {
            dict[value.Id.ToString()] = value.ToJObject();
        }
    }

    private static void SaveBackPack(KeyValuePair<string, CustomNpc> npc, Dictionary<string, JObject> dict)
    {
        var value = npc.Value.ToBackPack();
        if (value != null)
        {
            dict[npc.Key] = value;
        }
    }

    private static void SaveJson(string filename, Dictionary<string, CustomNpc> customNpcs, SaveJsonAction func)
    {
        
        var dictionary = new Dictionary<string, JObject>();
        foreach (var npc in customNpcs)
        {
            func?.Invoke(npc, dictionary);
        }

        if (dictionary.Count == 0)
        {
            return;
        }
        ;
       
        if (File.Exists(filename))
        {
            var jObject = JObject.Parse(File.ReadAllText(filename));
            var file= jObject.ToObject<Dictionary<string,JObject>>();
            if (file != null)
            {
                Main.LogInfo(jObject);
            
                foreach (var item in file)
                {
                    if (!dictionary.ContainsKey(item.Key))
                    {
                        Main.LogInfo(item.Value);
                        dictionary.Add(item.Key,item.Value);
                    }
          
                }
            }
          
        }
        var json = JObject.FromObject(dictionary).ToString(Formatting.Indented);
        File.WriteAllText(filename, json);
    }
}