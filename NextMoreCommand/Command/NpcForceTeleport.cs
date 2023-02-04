using SkySwordKill;
using SkySwordKill.Next;
using System;
using System.Collections.Generic;
using System.Linq;
using Fungus;
using SkySwordKill.Next.DialogEvent;
using SkySwordKill.Next.DialogSystem;
using SkySwordKill.NextMoreCommand.Attribute;
using SkySwordKill.NextMoreCommand.DialogTrigger;
using SkySwordKill.NextMoreCommand.Utils;
using UnityEngine.Events;

namespace SkySwordKill.NextMoreCommand.Command
{
  

  

    [RegisterCommand]
    [DialogEvent("NpcForceTeleport")]
    [DialogEvent("NpcForceMultiTeleport")]
    public class NpcForceTeleport : IDialogEvent
    {
        public bool m_isAdd;
        public List<NpcInfo> NpcInfos = new List<NpcInfo>();

        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            string dialog;
            int npc;
            switch (command.Command)
            {
                case "NpcForceTeleport":
                    npc = NPCEx.NPCIDToNew(command.GetInt(0, -1));
                    dialog = command.GetStr(1);
                    NpcInfos.Add(new NpcInfo(npc, dialog));
                    break;
                case "NpcForceMultiTeleport":
                    if (command.ParamList.Length == 0) break;
                    foreach (var param in command.ParamList)
                    {
                        var optionSep = param.Contains(":");
                        npc = param.ToNpcId();
                        dialog = "";
                        if (optionSep)
                        {
                            var option = param.Split(':');
                            npc = option[0].ToNpcId();
                            dialog = option[1];
                            NpcInfos.Add(new NpcInfo(npc, dialog));
                        }
                        else if (npc > 0)
                        {
                            NpcInfos.Add(new NpcInfo(npc, dialog));
                        }
                    }
                    break;
            }

            foreach (var npcInfo in NpcInfos)
            {
                AddNpc(npcInfo);
            }

            if (m_isAdd && !UiNpcJiaoHuRefreshNowMapNpcPatch.m_isRefresh)
            {
                NpcJieSuanManager.inst.isUpDateNpcList = true;
            }

            m_isAdd = false;
            NpcInfos.Clear();
            callback?.Invoke();
        }

        public void AddNpc(NpcInfo npcInfo)
        {
            if (npcInfo.Id <= 0)
            {
                return;
            }

            if (!UINPCJiaoHu.Inst.TNPCIDList.Contains(npcInfo.Id))
            {
                UINPCJiaoHu.Inst.TNPCIDList.Add(npcInfo.Id);
                m_isAdd = true;
            }

            NpcUtils.BindDialogEvent(npcInfo.Id, npcInfo.Dialog);
        }
    }
}