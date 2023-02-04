﻿using HarmonyLib;
using KBEngine;
using Newtonsoft.Json.Linq;
using SkySwordKill.Next;
using SkySwordKill.Next.DialogSystem;
using SkySwordKill.NextMoreCommand.Custom.SkillCombo;
using SkySwordKill.NextMoreCommand.Utils;

namespace SkySwordKill.NextMoreCommand.DialogTrigger
{
    internal static class RoundUtils
    {
        public static DialogEnvironment NewEnv => GetEnv();

        public static DialogEnvironment GetEnv()
        {
            var env = new DialogEnvironment()
            {
                fightTags = Next.DialogEvent.StartFight.FightTags,
                roleID = Tools.instance.MonstarID,
            };
            env.customData.Add("Monstar", Tools.instance.getPlayer().OtherAvatar);
            return env;
        }

        #region 触发器

        private static bool TryTrigger(params string[] param) => TryTrigger(NewEnv, true, param);

        private static bool TryTrigger(DialogEnvironment env, params string[] param) =>
            DialogAnalysis.TryTrigger(param, env);

        private static bool TryTrigger(bool triggerAll, params string[] param) =>
            DialogAnalysis.TryTrigger(param, NewEnv, triggerAll);

        private static bool TryTrigger(DialogEnvironment env, bool triggerAll, params string[] param)
        {
            Main.LogInfo($"进入触发器 {param[1]}");
            var result = DialogAnalysis.TryTrigger(param, env, triggerAll);
            if (SkillComboManager.SkillCombos.Count == 0)
            {
                return result;
            }

            if (SkillComboManager.TryTrigger(env, false, param))
            {
                return true;
            }
            else if (SkillComboManager.TryTriggerSkill(env, false, param))
            {
                return true;
            }

            return result;
        }

        #endregion

        #region 触发器类型

        public static bool StartRound(bool isBefore = true) => isBefore ? StartRoundBefore() : StartRoundAfter();
        public static bool EndRound(bool isBefore = true) => isBefore ? EndRoundBefore() : EndRoundAfter();

        public static bool PlayerEndRound(bool isBefore = true) =>
            isBefore ? PlayerEndRoundBefore() : PlayerEndRoundAfter();

        public static bool UseSkill(DialogEnvironment env = null, bool isBefore = true) =>
            isBefore ? PlayerUseSkillBefore(env) : PlayerUseSkillAfter(env);

        private static bool PlayerUseSkillBefore(DialogEnvironment env = null) =>
            TryTrigger(env ?? NewEnv, true, "PlayerUseSkillBefore", "玩家技能使用前");

        public static bool BreakPlayerUseSkillBefore(DialogEnvironment env = null) =>
            TryTrigger(env ?? NewEnv, true, "BreakPlayerUseSkillBefore", "打断玩家技能使用");

        private static bool PlayerUseSkillAfter(DialogEnvironment env = null) =>
            TryTrigger(env ?? NewEnv, true, "PlayerUseSkillAfter", "玩家技能使用后");

        public static bool FightFinish() => TryTrigger("FightFinish", "结束战斗");
        public static bool FightStart() => TryTrigger("FightStart", "开始战斗");
        private static bool StartRoundBefore() => TryTrigger("StartRoundBefore", "回合开始前");
        private static bool StartRoundAfter() => TryTrigger("StartRoundAfter", "回合开始后");
        private static bool EndRoundBefore() => TryTrigger("EndRoundBefore", "回合结束前");
        private static bool EndRoundAfter() => TryTrigger("EndtRoundAfter", "回合结束后");
        private static bool PlayerEndRoundBefore() => TryTrigger("PlayerEndRoundBefore", "玩家回合结束前");
        private static bool PlayerEndRoundAfter() => TryTrigger("PlayerEndRoundAfter", "玩家回合结束后");

        #endregion
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.startRound))]
    public static class OnStartRound
    {
        public static Avatar Avatar;

        public static void Prefix(RoundManager __instance, Entity _avater)
        {
            Avatar = (Avatar)_avater;
            if (__instance.StaticRoundNum == 0)
            {
                if (RoundUtils.FightStart())
                {
                    MyLog.FungusLog("进入开始战斗");
                }
            }
            else
            {
                if (RoundUtils.StartRound())
                {
                    MyLog.FungusLog("进入开始回合之前");
                }
            }
        }

        public static void Postfix()
        {
            if (RoundUtils.StartRound(false))
            {
                MyLog.FungusLog("进入开始回合之后");
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.PlayerEndRound))]
    public static class OnPlayerEndRound
    {
        public static void Prefix()
        {
            MyLog.FungusLog("进入玩家结束回合之前");
            if (RoundUtils.PlayerEndRound())
            {
                MyLog.FungusLog("进入玩家结束回合之前");
            }

        }

        public static void Postfix()
        {
            MyLog.FungusLog("进入玩家结束回合之后");
            if (RoundUtils.PlayerEndRound(false))
            {
                MyLog.FungusLog("进入玩家结束回合之后");
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.endRound))]
    public static class OnEndRound
    {
        public static void Prefix(ref Entity _avater)
        {
            // var avatar = (Avatar)_avater;
            // var isPlayer = avatar != null && avatar.isPlayer();
            // if (isPlayer && RoundUtils.PlayerEndRound())
            // {
            //     MyLog.FungusLog("进入玩家结束回合之前");
            // }

            if (RoundUtils.EndRound())
            {
                MyLog.FungusLog("进入结束回合之前");
            }
        }

        public static void Postfix(ref Entity _avater)
        {
            // var avatar = (Avatar)_avater;
            // var isPlayer = avatar != null && avatar.isPlayer();
            // if (isPlayer && RoundUtils.PlayerEndRound(false))
            // {
            //     MyLog.FungusLog("进入玩家结束回合之前");
            // }
            if (RoundUtils.EndRound(false))
            {
                MyLog.FungusLog("进入结束回合之后");
            }
        }
    }

    [HarmonyPatch(typeof(RoundManager), "OnDestroy")]
    public static class OnFinishFight
    {
        public static void Postfix()
        {
            SkillComboManager.CacheSkillCombos.Clear();
            if (RoundUtils.FightFinish())
            {
                MyLog.FungusLog("进入结束战斗触发器");
            }
        }
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.UseSkill))]
    public static class OnUseSkill
    {
        private static RoundManager instance => RoundManager.instance;

        public static bool Prefix()
        {
            var env = RoundUtils.NewEnv;
            env.customData.Add("CurSkill", instance.ChoiceSkill);
            if (RoundUtils.BreakPlayerUseSkillBefore(env))
            {
                MyLog.FungusLog("进入打断玩家技能使用前触发器");
                instance.OnPlayerEndRoundQiZhiLingQiCancelClick();
                return false;
            }
            else if (RoundUtils.UseSkill(env))
            {
                MyLog.FungusLog("进入玩家技能使用前触发器");
            }

            return true;
        }

        public static void Postfix()
        {
            var env = RoundUtils.NewEnv;
            env.customData.Add("CurSkill", instance.CurSkill);
            if (RoundUtils.UseSkill(env, false))
            {
                MyLog.FungusLog("进入玩家技能使用后触发器");
            }

            OnStartRound.Avatar = null;
        }
    }
}