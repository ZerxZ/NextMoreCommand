using SkySwordKill;
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
    [DialogEvent("RunFungusTalk")]
    public class RunFungusTalk : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            var talkID = command.GetInt(0, -1);
            var tagBlock = command.GetStr(1);

            Main.LogInfo($"FungusEvent : RunFungusTalk");
          
            if (FungusUtils.GetTalk(talkID) == null)
            {
                Main.LogError($"FungusEvent : 对应Talk{talkID.ToString()} flowchart不存在");
            }
            else
            {
                FungusUtils.TalkBlockName = tagBlock;
                FungusUtils.TalkFunc = flowchart => { return flowchart.ExecuteIfHasBlock(FungusUtils.TalkBlockName); };
                FungusUtils.TalkOnComplete =
                    () => Main.LogInfo($"FungusEvent : 跳转FungusBlock {FungusUtils.TalkBlockName}");
                FungusUtils.TalkOnFailed = () => MyLog.FungusLogError($"Block名称不存在 {FungusUtils.TalkBlockName}");
                FungusUtils.isTalkActive = true;
            }


            callback?.Invoke();
        }
    }
}