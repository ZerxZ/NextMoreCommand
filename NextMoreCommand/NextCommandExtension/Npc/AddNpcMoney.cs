using System;
using SkySwordKill.Next.DialogEvent;
using SkySwordKill.Next.DialogSystem;
using SkySwordKill.NextMoreCommand.Attribute;
using SkySwordKill.NextMoreCommand.Utils;

namespace SkySwordKill.NextMoreCommand.NextCommandExtension
{
    [RegisterCommand]
    [DialogEvent("AddNpcMoney")]
    [DialogEvent("增加角色灵石")]
    public class AddNpcMoney : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {
            var npc = command.ToNpcId();
            var money = command.GetInt(1, 0);
            MyLog.LogCommand(command);
            if (npc > 0)
            {
                MyLog.Log(command,$"开始执行增加角色灵石 角色ID:{npc} 角色名:{npc.GetNpcName()} 增加灵石:{money}");
                var data = jsonData.instance.AvatarBackpackJsonData[npc.ToNpcId()];
                var nowMoney = data.GetInt("money");
                var resultMoney = nowMoney + money;
                MyLog.Log(command,$"执行增加角色灵石 当前灵石:{nowMoney} 增加灵石:{money} 灵石结果:{resultMoney}");
                NPCEx.SetMoney(npc,  resultMoney);
            }
            else
            {
                MyLog.Log(command,$"执行失败增加角色灵石 角色ID:{npc} 不能小于 0",true);
            }
            
           
            MyLog.LogCommand(command,false);
            callback?.Invoke();
        }
    }
}