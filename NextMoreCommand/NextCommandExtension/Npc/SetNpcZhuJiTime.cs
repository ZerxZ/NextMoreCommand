using System;
using SkySwordKill.Next.DialogEvent;
using SkySwordKill.Next.DialogSystem;
using SkySwordKill.NextMoreCommand.Attribute;

namespace SkySwordKill.NextMoreCommand.NextCommandExtension
{
    [RegisterCommand]
    [DialogEvent("SetNpcZhuJiTime")]
    [DialogEvent("保送角色筑基时间")]
    public class SetNpcZhuJiTime : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            var npc = NPCEx.NPCIDToNew(command.GetInt(0, -1));
            var year = command.GetInt(1, 5000);
            var mouth = command.GetInt(2, 1);
            var day = command.GetInt(2, 1);
         
            var data = NpcJieSuanManager.inst.GetNpcData(npc);
            data.SetField("ZhuJiTime",$"{year:0000}-{mouth:00}-{day:00}");
            callback?.Invoke();
        }
    }
}