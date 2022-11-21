﻿using SkySwordKill;
using SkySwordKill.Next;
using System;
using Fungus;
using SkySwordKill.Next.DialogEvent;
using SkySwordKill.Next.DialogSystem;
using SkySwordKill.NextMoreCommand.Attribute;
using SkySwordKill.NextMoreCommand.Utils;
using UnityEngine;

namespace SkySwordKill.NextMoreCommand.Command
{
    [RegisterCommand]
    [DialogEvent("RunFungusTalkItemId")]
    public class RunFungusTalkItemId : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            var talkID = command.GetInt(0,-1);
            var tagBlock = command.GetStr(1);
            var itemId = command.GetInt(2, -1);

            DialogAnalysis.CancelEvent();

            if (!FungusUtils.TryGetTalk(talkID, out  Flowchart flowchart ))
            {
                Main.LogError($"FungusEvent : 对应Talk{talkID.ToString()} flowchart不存在");
                return;
            }
            var index = flowchart.FindIndex(tagBlock, itemId, out Block block);
            if (block == null)
            {
                MyLog.FungusLogError($"Block名称不存在 {tagBlock}");
            }
            else if (index < 0)
            {
                var msg = itemId == -1 ? "ItemId不能为空和字符串" : $"ItemId不存在 {itemId}";
                MyLog.FungusLogError(msg);
            }
            else
            {
                Main.LogInfo($"FungusEvent : 跳转FungusBlock {tagBlock} ItemId:{itemId} index:{index} ");
                flowchart.ExecuteBlock(block, index);
            }

            callback?.Invoke();
        }
    }
}