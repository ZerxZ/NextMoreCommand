using System;
using System.Collections;
using Fungus;
using HarmonyLib;
using JiaoYi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkySwordKill.Next.DialogSystem;
using SkySwordKill.NextMoreCommand.NextCommandExtension.Utils;
using SkySwordKill.NextMoreCommand.Utils;
using Spine;
using Spine.Unity;
using Tab;
using UnityEngine;
using Avatar = KBEngine.Avatar;
using GameObject = UnityEngine.GameObject;
using Object = UnityEngine.Object;

namespace SkySwordKill.NextMoreCommand.Patchs
{

    enum EStateType
    {
        None,
        Image,
        SpriteRenderer
    }

    public class CustomImage : MonoBehaviour
    {
        public  Transform         spine;
        private SpriteRenderer    _spriteRenderer;
        public  SkeletonAnimation skeletonAnimation;
        // private Image _image;
        // private EStateType _stateType;

        public void SetSpine(PlayerSetRandomFace playerSetRandom)
        {
            spine = playerSetRandom.transform;

            skeletonAnimation = playerSetRandom.GetComponent<SkeletonAnimation>();
            _spriteRenderer = gameObject.AddMissingComponent<SpriteRenderer>();
            var transform1 = transform;
            transform1.SetParent(spine.parent);
            transform1.localRotation = spine.localRotation;
            transform1.localPosition = new Vector3(0, 3,    0);
            transform1.localScale = new Vector3(0.5f, 0.5f, 0);

        }
        private static Avatar Player => PlayerEx.Player;
        public void SetAvatarId(int monstarID)
        {
            StartCoroutine(SetAvatar(monstarID));
        }

        public IEnumerator SetAvatar(int avatar)
        {
            string faceId;
            string lihui;
            if (avatar == 1)
            {
                faceId = Player.Face.ToString();
                lihui = Player.FaceWorkshop;
            }
            else
            {
                var npcJson = avatar.NPCJson();
                faceId = npcJson["face"].I.ToString();
                lihui = npcJson.HasField("workshoplihui") ? npcJson["workshoplihui"].str : string.Empty;
            }

            if (string.IsNullOrWhiteSpace(lihui) && faceId == "0")
            {
                DestroyImmediate(this);
                yield break;
            }

            var path = $"workshop_{lihui}_{faceId}";
            if (string.IsNullOrWhiteSpace(lihui))
            {
                path = $"Effect/Prefab/gameEntity/Avater/Avater{faceId}/{faceId}";
                if (AssetsUtils.GetCustomImageConfig("Res/" + path + ".png", out CustomSpineOption customSpineOption))
                {
                    var transform1  = transform;
                    var nowRotation = transform1.localRotation;
                    customSpineOption?.SetTransform(transform1);
                    transform.localRotation = nowRotation;
                }
            }

            var sprite = ModResources.LoadSprite(path);

            if (sprite == null)
            {
                if (skeletonAnimation) skeletonAnimation.maskInteraction = SpriteMaskInteraction.None;
                DestroyImmediate(gameObject);
                yield break;
            }

            // _spriteRenderer 是通过 AddMissingComponent 获取的，所以不会为 null
            _spriteRenderer.sprite = sprite;

            if (skeletonAnimation) skeletonAnimation.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        private void OnDestroy()
        {

            spine = null;

            // if (_spriteRenderer != null)
            // {
            //     DestroyImmediate(_spriteRenderer);
            // }
            skeletonAnimation = null;
        }
    }

    public enum ESpineType
    {
        None,
        UINpcSvItem,
        SayDialog,
        UINpcJiaoHuPop,
        UINpcInfoPanel,
        FightAvatar,
        JiaoYiUIMag,
        FpUIMag,
        LunDaoManager,
        CGManager,
        UiHeadPanel,
        TabUiMag,
        Custom
    }

    [JsonObject]
    public class CustomSpinePos
    {
        public static CustomSpinePos Zero => new CustomSpinePos();
        public static CustomSpinePos One  => new CustomSpinePos(1, 1, 1);
        public        float          X    { get; set; } = 0;
        public        float          Y    { get; set; } = 0;
        public        float          Z    { get; set; } = 0;
        public CustomSpinePos() : this(0f, 0f)
        {
        }
        public CustomSpinePos(float x, float y, float z = 0f)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
    }

    [JsonObject]
    public class CustomSpineOption
    {
        public static readonly CustomSpineOption UINpcSvItemPos    = new CustomSpineOption(new CustomSpinePos(0,      -888),       new CustomSpinePos(1,    1,    1));
        public static readonly CustomSpineOption SayDialogPos      = new CustomSpineOption(new CustomSpinePos(0,      -250.6f),    new CustomSpinePos(1,    1,    1));
        public static readonly CustomSpineOption UINpcInfoPanelPos = new CustomSpineOption(new CustomSpinePos(0,      -800),       new CustomSpinePos(1,    1,    1));
        public static readonly CustomSpineOption FightAvatarPos    = new CustomSpineOption(new CustomSpinePos(0,      0),          new CustomSpinePos(0.4f, 0.4f, 1));
        public static readonly CustomSpineOption UINpcJiaoHuPopPos = new CustomSpineOption(new CustomSpinePos(0,      -750),       new CustomSpinePos(1,    1,    1));
        public static readonly CustomSpineOption FpUIMagPos        = new CustomSpineOption(new CustomSpinePos(0,      -800),       new CustomSpinePos(1,    1,    1));
        public static readonly CustomSpineOption LunDaoManagerPos  = new CustomSpineOption(new CustomSpinePos(0,      -750),       new CustomSpinePos(1,    1,    1));
        public static readonly CustomSpineOption UiHeadPanelPos    = new CustomSpineOption(new CustomSpinePos(-13,    -890),       new CustomSpinePos(1,    1,    1));
        public static readonly CustomSpineOption TabUiMagPos       = new CustomSpineOption(new CustomSpinePos(262.8f, -891.7998f), new CustomSpinePos(0.8f, 0.8f, 1));

        public CustomSpineOption()
        {

        }
        public CustomSpineOption(CustomSpinePos position, CustomSpinePos scale, CustomSpinePos rotation = null)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation ?? CustomSpinePos.Zero;
        }
        [JsonProperty("位置", NullValueHandling = NullValueHandling.Ignore)]
        public CustomSpinePos Position { get; set; } = CustomSpinePos.Zero;
        [JsonProperty("缩放", NullValueHandling = NullValueHandling.Ignore)]
        public CustomSpinePos Scale { get; set; } = CustomSpinePos.One;
        [JsonProperty("角度", NullValueHandling = NullValueHandling.Ignore)]
        public CustomSpinePos Rotation { get; set; } = CustomSpinePos.Zero;
        public void SetTransform(Transform transform)
        {
            transform.localPosition = Position.ToVector3();
            transform.localEulerAngles = Rotation.ToVector3();
            transform.localScale = Scale.ToVector3();
        }
        public CustomSpineOption Clone()
        {
            return new CustomSpineOption(Position, Scale, Rotation);
        }
        public override string ToString()
        {
            return JObject.FromObject(this).ToString(Formatting.Indented);
        }
    }

    public static class CustomSpineManager
    {

    }

    public class CustomSpine : MonoBehaviour
    {
        private UINPCSVItem _uiNpcSvItem;
        private SayDialog   _sayDialog;
        public  ESpineType  spineType = ESpineType.None;
        // private CustomSpineOption _customSpineOption = null;
        private UINPCJiaoHuPop    _uiNpcJiaoHuPop;
        private UINPCInfoPanel    _uiNpcInfoPanel;
        private InitAvatar        _fightAvatar;
        private JiaoYiUIMag       _jiaoYiUIMag;
        private FpUIMag           _fpUIMag;
        private CustomSpineOption customSpineOption;
        private CustomSpineOption defaultSpineOption;
        private LunDaoManager     _lunDaoManager;
        private int               _avatar;
        private CGSpineManager    _cgManager;
        private UIHeadPanel       _uiHeadPanel;
        private TabUIMag          _tabUiMag;

        private void OnEnable()
        {
            switch (spineType)
            {
                case ESpineType.UINpcSvItem:
                    customSpineOption?.SetTransform(transform);
                    break;
                case ESpineType.None:
                case ESpineType.Custom:
                    break;
                case ESpineType.SayDialog:
                    break;
                case ESpineType.UINpcJiaoHuPop:
                    break;
                case ESpineType.UINpcInfoPanel:
                    break;
                case ESpineType.FightAvatar:
                    break;
                case ESpineType.JiaoYiUIMag:
                    break;
                case ESpineType.FpUIMag:
                    break;
                case ESpineType.LunDaoManager:
                    break;
                case ESpineType.CGManager:
                    break;
                case ESpineType.UiHeadPanel:
                    break;
                case ESpineType.TabUiMag:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Init()
        {
            if (spineType == ESpineType.Custom)
            {
                return;
            }
            spineType = ESpineType.None;
            _uiNpcSvItem = GetComponentInParent<UINPCSVItem>();
            _sayDialog = GetComponentInParent<SayDialog>();
            _uiNpcJiaoHuPop = GetComponentInParent<UINPCJiaoHuPop>();
            _uiNpcInfoPanel = GetComponentInParent<UINPCInfoPanel>();
            _fightAvatar = GetComponentInParent<InitAvatar>();
            _jiaoYiUIMag = GetComponentInParent<JiaoYiUIMag>();
            _fpUIMag = GetComponentInParent<FpUIMag>();
            _lunDaoManager = GetComponentInParent<LunDaoManager>();
            _cgManager = GetComponentInParent<CGSpineManager>();
            _uiHeadPanel = GetComponentInParent<UIHeadPanel>();
            _tabUiMag = GetComponentInParent<TabUIMag>();
            if (_uiNpcSvItem != null)
            {
                spineType = ESpineType.UINpcSvItem;
            }
            else if (_sayDialog != null)
            {
                spineType = ESpineType.SayDialog;
            }
            else if (_uiNpcJiaoHuPop != null)
            {
                spineType = ESpineType.UINpcJiaoHuPop;
            }
            else if (_uiNpcInfoPanel != null)
            {
                spineType = ESpineType.UINpcInfoPanel;
            }
            else if (_fightAvatar != null)
            {
                spineType = ESpineType.FightAvatar;
            }
            else if (_jiaoYiUIMag != null)
            {
                spineType = ESpineType.JiaoYiUIMag;
            }
            else if (_fpUIMag != null)
            {
                spineType = ESpineType.FpUIMag;
            }
            else if (_lunDaoManager != null)
            {
                spineType = ESpineType.LunDaoManager;
            }
            else if (_cgManager != null)
            {
                spineType = ESpineType.CGManager;
            }
            else if (_uiHeadPanel != null)
            {
                spineType = ESpineType.UiHeadPanel;
            }
            else if (_tabUiMag != null)
            {
                spineType = ESpineType.TabUiMag;
            }
            Reset();
        }

        // public void SetAvatar(int avatar, bool isSay = false)
        public void SetAvatar(int avatar, bool isSay = false)
        {
            _avatar = avatar;
            SetAvatar(avatar.ToString(), isSay);
        }
        public void SetAvatarCustom(string avatar, string option, ESpineAssetType spineAssetType = ESpineAssetType.Avatar)
        {
            if (string.IsNullOrWhiteSpace(avatar))
            {
                return;
            }
            spineType = ESpineType.Custom;
            AssetsUtils.GetCustomSpineConfig(avatar, out var customSpineConfig, spineAssetType);
            customSpineConfig.CustomSpineOptions.TryGetValue(option, out customSpineOption);
            if (customSpineOption is null)
            {
                Reset();
            }
            else
            {
                customSpineOption.SetTransform(transform);
            }
        }
        public void SetAvatar(string avatar, bool isSay = false)
        {
            if (isSay)
            {
                spineType = ESpineType.SayDialog;
                Reset();
            }
            else
            {
                Init();
            }

            if (string.IsNullOrWhiteSpace(avatar))
            {
                return;
            }
            customSpineOption = null;

            MyPluginMain.LogInfo($"avatar:{avatar} spineType:{spineType.GetName()}");
            AssetsUtils.GetCustomSpineOption(avatar, spineType, out customSpineOption, spineType == ESpineType.CGManager ? ESpineAssetType.Cg : ESpineAssetType.Avatar);
            MyPluginMain.LogInfo($"customSpineOption:\n{customSpineOption}");
            if (customSpineOption is null)
            {
                Reset();
            }
            else
            {
                customSpineOption.SetTransform(transform);
            }

        }
        public void Refresh()
        {
            customSpineOption?.SetTransform(transform);
        }
        private void OnDisable()
        {
            Init();
        }
        public void Reset()
        {
            defaultSpineOption = null;
            switch (spineType)
            {
                case ESpineType.None:
                case ESpineType.JiaoYiUIMag:
                    break;
                case ESpineType.SayDialog:
                    defaultSpineOption = CustomSpineOption.SayDialogPos;
                    break;
                case ESpineType.UINpcSvItem:
                    defaultSpineOption = CustomSpineOption.UINpcSvItemPos;
                    break;
                case ESpineType.UINpcJiaoHuPop:
                    defaultSpineOption = CustomSpineOption.UINpcJiaoHuPopPos;
                    break;
                case ESpineType.UINpcInfoPanel:
                    defaultSpineOption = CustomSpineOption.UINpcInfoPanelPos;
                    break;
                case ESpineType.FightAvatar:
                    defaultSpineOption = CustomSpineOption.FightAvatarPos;
                    break;
                case ESpineType.FpUIMag:
                    defaultSpineOption = CustomSpineOption.FpUIMagPos;
                    break;
                case ESpineType.LunDaoManager:
                    defaultSpineOption = CustomSpineOption.FpUIMagPos;
                    break;
                case ESpineType.CGManager:
                    break;
                case ESpineType.UiHeadPanel:
                    defaultSpineOption = CustomSpineOption.UiHeadPanelPos;
                    break;
                case ESpineType.TabUiMag:
                    // defaultSpineOption = CustomSpineOption.TabUiMagPos;
                    break;
                case ESpineType.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            defaultSpineOption?.SetTransform(transform);
        }
        private void OnDestroy()
        {
            customSpineOption = null;
            Reset();
        }
    }

    public class SpineAvatarInfo
    {
        public void Init(int id)
        {
            Id = id;
            FaceSpine = GetFaceSpine();
            var (result, skeletonData) = GetSkeletonData();
            Result = result && skeletonData != null;
            SkeletonDataAsset = skeletonData;
            GetFaceSpineSkin();
        }
        public void Refresh()
        {
            FaceSpine ??= GetFaceSpine();
            var (result, skeletonData) = GetSkeletonData();
            Result = result && skeletonData != null;
            SkeletonDataAsset = skeletonData;
            GetFaceSpineSkin();
        }
        public void SetSpine(string spine, bool force = false)
        {
            FaceSpine = spine;
            if (!force)
            {
                return;
            }
            if (Id == 1)
            {
                DialogAnalysis.SetStr("PLAYER_SPINE", spine);
            }
            else
            {
                NpcUtils.SetNpcFaceSpine(Id, spine);

            }
  
        }
        public void SetSkin(string skin, string defaultSkin, bool force = false)
        {

            Skin = skin;
            SkinDefault = defaultSkin;
            if (!force)
            {
                return;
            }
            if (Id == 1)
            {

                DialogAnalysis.SetStr("PLAYER_SPINE_SKIN", skin);

            }
            else
            {

                NpcUtils.SetNpcSkinSpine(Id, skin);
                NpcUtils.SetNpcDefaultSkinSpine(Id, defaultSkin);

            }
        }
        public Action<TrackEntry, SpineAvatarInfo> OnStartAnimator;
        public void InitPlayer()
        {
            Id = 1;
            SkeletonDataAsset skeletonData;
            SkinDefault = "default";
            var spine     = DialogAnalysis.GetStr("PLAYER_SPINE");
            var spineSkin = DialogAnalysis.GetStr("PLAYER_SPINE_SKIN");
            if (int.TryParse(spine, out var id))
            {
                if (AssetsUtils.GetSkeletonData(id, out skeletonData))
                {
                    FaceSpine = id.ToString();
                    Skin = AssetsUtils.CheckSkin(id, spineSkin) ? spineSkin : "default";

                }
            }
            else
            {
                if (AssetsUtils.GetSkeletonData(spine, out skeletonData))
                {
                    FaceSpine = spine;
                    Skin = AssetsUtils.CheckSkin(spine, spineSkin) ? spineSkin : "default";
                }
            }
            Result = skeletonData == null;
            SkeletonDataAsset = skeletonData;
        }
        public void StartAnimator(TrackEntry entry)
        {
            if (skeletonAnimation is null) return;
            if (OnStartAnimator != null)
            {
                OnStartAnimator(entry, this);
                return;
            }
            var name         = entry.Animation.Name;
            var hasAnimation = AssetsUtils.CheckAnimation(FaceSpine, name, out var isIdle);
            if (!hasAnimation) return;
            var trackEntry = skeletonAnimation.AnimationState.GetCurrent(0);
            if (trackEntry != null)
            {
                skeletonAnimation.AnimationState.ClearTrack(0);
            }
            skeletonAnimation.AnimationState.SetAnimation(0, name, isIdle);
        }
        public bool IsFightScene => NpcUtils.IsFightScene;

        public SkeletonAnimation skeletonAnimation { get; set; }
        public bool              Result;
        public int               Id                { get; set; } = 0;
        public string            FaceSpine         { get; set; } = null;
        public SkeletonDataAsset SkeletonDataAsset { get; set; }
        public string            Skin              { get; set; } = null;
        public string            SkinDefault       { get; set; } = null;
        public void GetFaceSpineSkin()
        {
            SkinDefault ??= NpcUtils.GetNpcDefaultSkinSpine(Id);
            Skin ??= NpcUtils.GetNpcSkinSpine(Id);
            Skin = AssetsUtils.CheckSkin(FaceSpine, Skin) ? Skin : SkinDefault;
        }
        public string                    GetFaceSpine()    => NpcUtils.GetNpcFaceSpine(Id);
        public (bool, SkeletonDataAsset) GetSkeletonData() => AssetsUtils.GetSkeletonData(FaceSpine, out var skeletonData) ? (true, skeletonData) : (false, null);

        public bool HasFightSpine() => NpcUtils.GetNpcFightSpine(Id);
    }


    [HarmonyPatch(typeof(PlayerSetRandomFace), nameof(PlayerSetRandomFace.randomAvatar))]
    public static class PlayerSetRandomFaceRandomAvatarPatch
    {
        public static int             m_avartarID;
        public static bool            m_customSpine;
        public static SkeletonGraphic skeletonGraphic;
        public static int             avartarID;
        public static CustomSpine     customSpine;
        // public static List<int> CustomNpc = new List<int>()
        // {
        //     8471,
        //     9740,
        //     7200
        // };
        public static bool                          PlayerInit;
        public static event Action<SpineAvatarInfo> OnSetSpineAvatar;
        public static void CustomSpineXXX()
        {
            PlayerSetRandomFaceRandomAvatarPatch.OnSetSpineAvatar += (spineAvatar) =>
            {
                // 判断是否是自定义的NPC
                if (spineAvatar.Id == 7200 && spineAvatar.IsFightScene)
                {
                    //设置骨骼文件
                    spineAvatar.SetSpine("7200");
                    //设置皮肤
                    spineAvatar.SetSkin("幼年小江", "成人小江");
                    //设置动画逻辑
                    spineAvatar.OnStartAnimator = (entry, info) =>
                    {
                        //修改动画逻辑
                    };
                    //刷新
                    spineAvatar.Refresh();
                }
            };
        }

        public static bool SetSpine(PlayerSetRandomFace __instance)
        {


            if (NpcUtils.IsFightScene && avartarID == 1)
            {
                PlayerInit = true;
            }

            var               skeletonAnimation = __instance.GetComponent<SkeletonAnimation>();
            var               skin              = string.Empty;
            SkeletonDataAsset skeletonData;
            var               key         = string.Empty;
            var               spineAvatar = new SpineAvatarInfo();
            if (avartarID == 1)
            {
                spineAvatar.InitPlayer();
                OnSetSpineAvatar?.Invoke(spineAvatar);
                if (spineAvatar.Result)
                {
                    return false;
                }
                key = spineAvatar.FaceSpine;
                skin = spineAvatar.Skin;
                skeletonData = spineAvatar.SkeletonDataAsset;

            }
            else

            {
                spineAvatar.Init(avartarID);
                OnSetSpineAvatar?.Invoke(spineAvatar);
                if (!spineAvatar.HasFightSpine() || !spineAvatar.Result)
                {
                    return false;
                }
                key = spineAvatar.FaceSpine;
                skeletonData = spineAvatar.SkeletonDataAsset;
                MyLog.Log($"key:{key}");
                skin = spineAvatar.Skin;
                MyLog.Log($"key:{key} skinName:{skin} skin:{skin}");
            }


            if (skeletonGraphic != null)
            {

                skeletonGraphic.skeletonDataAsset = skeletonData;
                skeletonGraphic.initialSkinName = skin;
                skeletonGraphic.startingAnimation = "Idle_0";
                skeletonGraphic.Initialize(true);
                var component    = __instance.gameObject.AddMissingComponent<CustomSpine>();
                var sayTransform = __instance.transform;
                var say          = false;
                for (var i = 0; i < 3; i++)
                {
                    sayTransform = sayTransform.parent;
                    if (sayTransform == null)
                    {
                        break;
                    }
                    if (i == 2)
                    {
                        say = sayTransform.name == "SayDialog";
                    }
                }
                component.SetAvatar(key, say);
                var jiaoYiUI = __instance.GetComponentInParent<JiaoYiUIMag>() == null;
                m_customSpine = jiaoYiUI;
            }
            else if (skeletonAnimation != null)
            {
                if (AssetsUtils.GetSkeletonAnimation(key, out var skeletonAnimationGo))
                {
                    skeletonAnimation.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    var gameObject         = Object.Instantiate(skeletonAnimationGo, skeletonAnimation.transform.parent);
                    var skeletonAnimation1 = gameObject.GetComponent<SkeletonAnimation>();
                    skeletonAnimation1.skeletonDataAsset = skeletonData;
                    skeletonAnimation1.initialSkinName = skin;
                    skeletonAnimation1.AnimationName = "Idle_0";
                    skeletonAnimation1.Initialize(true);
                    spineAvatar.skeletonAnimation = skeletonAnimation1;
                    skeletonAnimation.AnimationState.Start += spineAvatar.StartAnimator;
                    m_customSpine = true;
                    customSpine = gameObject.AddMissingComponent<CustomSpine>();
                    customSpine.SetAvatar(key);
                }
            }
            var baseSpine = __instance.BaseSpine;
            if (baseSpine != null)
            {
                baseSpine.SetActive(m_customSpine);
            }
            var baseImage = __instance.BaseImage;
            if (baseImage != null)
            {
                baseImage.SetActive(!m_customSpine);
            }
            return m_customSpine;
        }
        public static bool Prefix(PlayerSetRandomFace __instance, int monstarID)
        {
            m_customSpine = false;
            m_avartarID = monstarID;
            avartarID = NPCEx.NPCIDToOld(m_avartarID);
            customSpine = null;
            skeletonGraphic = null;

            // 逻辑运算遵循“短路原则” 因此最好是合并到一起 并且调整合适的顺序
            if (
                SceneEx.NowSceneName == "MainMenu" ||
                __instance.GetComponentInParent<JiaoYiUIMag>() != null ||
                m_avartarID is > 1 and < 20000
            ) return true;

            if (NpcUtils.IsFightScene && avartarID == 1 && PlayerInit)
            {
                return true;
            }

            skeletonGraphic = __instance.GetComponent<SkeletonGraphic>();
            if (SetSpine(__instance))
            {
                return false;
            }

            // IsFightScene 是一个 static 数据 不需要额外引用
            // 逻辑运算遵循“短路原则” 因此最好是合并到一起 并且调整合适的顺序
            if (!NpcUtils.IsFightScene ||
                __instance.BaseImage != null ||
                !NpcUtils.GetNpcFightFace(avartarID)
               ) return true;

            // 在创建 GameObject 的时候，如果使用 type 传递组件，内部会自动调用 AddComponent 但是返回的是 GameObject 对象
            // 想要获取 Component 对象，还要额外 GetComponent ，而使用 AddMissingComponent 方法，内部也是调用 GetComponent 同时还要进行判空
            // 综上所述，建议采用这种写法
            var customImage = new GameObject("Image").AddComponent<CustomImage>();
            customImage.SetSpine(__instance);
            customImage.SetAvatarId(m_avartarID);

            return true;
        }
    }
}