using System;
using System.Collections.Generic;
using SkySwordKill.Next;
using SkySwordKill.Next.DialogSystem;

namespace SkySwordKill.NextMoreCommand.Utils
{
    public static class NpcUtils
    {
        public static Dictionary<int, string> SelfNameDict = new Dictionary<int, string>();
        public static JSONObject AvatarRandomJsonData => jsonData.instance.AvatarRandomJsonData;
        public static JSONObject AvatarJsonData => jsonData.instance.AvatarJsonData;
        public static bool IsNpc(int id) => NPCEx.NPCIDToNew(id) <= 1;
        public static bool IsNpc(string id) => IsNpc(Convert.ToInt32(id));

        public static JSONObject GetNpcData(string npcId)
        {
            var num = Convert.ToInt32(npcId);
            if (IsNpc(npcId))
            {
                return null;
            }

            return GetNpcData(num);
        }
        
        public static JSONObject GetNpcData(int npcId)
        {
            var num = NPCEx.NPCIDToNew(npcId);
            if (num <= 1)
            {
                return null;
            }

            var id = num.ToString();
            JSONObject jsonObject = null;
            if (jsonData.instance != null)
            {
                if (AvatarRandomJsonData.HasField(id))
                {
                    jsonObject = AvatarRandomJsonData[id];
                }
                else if (AvatarJsonData.HasField(id))
                {
                    jsonObject = AvatarJsonData[id];
                }
            }
            return jsonObject;
        }

        public static bool SetNpcName(string id, string name) => IsNpc(id) && SetNpcName(id, name);
        public static bool SetNpcName(int id, string name)
        {
            var npc = GetNpcData(id);
            if (npc == null)
            {
                return false;
            }
            npc.SetField("Name",name);
            return true;
        }

        public static string GetSelfName(string id)
        {
            var npcId = Convert.ToInt32(id);
            if (npcId == 0)
            {
                return "";
            }

            return GetSelfName(npcId);
        }

        public static string GetSelfName(int id)
        {
            if (SelfNameDict.TryGetValue(id, out string value))
            {
                return value;
            }

            return "";
        }

        public static bool SetSelfName(string id, string name)
        {
            var npcId = Convert.ToInt32(id);
            if (npcId == 0)
            {
                return false;
            }

            return SetSelfName(npcId, name);
        }

        public static bool SetSelfName(int id, string name)
        {
            SelfNameDict[id] = name;
            return true;
        }

        public static List<int> GetNpcList(DialogCommand command, int count) => GetNpcList(command, count, count);

        public static List<int> GetNpcList(DialogCommand command, int minCount, int restCount)
        {
            var list = new List<int>();
            var paramCount = command.ParamList.Length;
            if (paramCount < minCount)
            {
                return list;
            }

            if (paramCount == restCount)
            {
                var npcId = command.GetStr(1, string.Empty);
                var npcArr = npcId.Split(',');
                foreach (var npc in npcArr)
                {
                    Main.LogInfo($"添加NPCID: [{npc}]");
                    list.Add(NPCEx.NPCIDToNew(Convert.ToInt32(npc)));
                }
            }
            else
            {
                for (int i = restCount - 1; i < command.ParamList.Length; i++)
                {
                    var npc = NPCEx.NPCIDToNew(command.GetInt(i, -1));
                    if (npc >= 0)
                    {
                        Main.LogInfo($"添加NPCID: [{npc}]");
                        list.Add(npc);
                    }
                }
            }

            return list;
        }
    }
}