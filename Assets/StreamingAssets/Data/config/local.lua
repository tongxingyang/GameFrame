Local = {}
Local.ShowSkillRange = false -- true or false
Local.LogProtocol = true -- 是否开启协议日志
Local.LogManager = true -- logmanager开关
Local.LogTraceback = false --
Local.Status = false --开启效率监控
Local.JailBreak = false
Local.HideVip = false
Local.HideOperationActivity = false
Local.HideZone = true
Local.MoveCheck = false
Local.StringFormatTest = false
Local.LogModuals=
{
    Moduals = false,
    Skill = false, --模块名和开关
    Action = false, --动作
    TraceObject = false,
    UIManager = false, --UI模块
	EffectManager = false, --特效管理
    AnimationManager = false,
    WorkManager = false ,


    SettingManager = false,
    CreatePlayer = false,
    Dress = false,
    Lottery = false,

	Portal=false, --传送
	Team=false,	--组队
	Drop=false,	--掉落
	Ride=false,  --坐骑
	WorldBoss=false, --世界BOSS
	Friend = false,

    Maimai = false,  --脉脉
    Title = false,
    Bag = false,
    Plot = false,
    Camera = false,

    Arena = false,
    TeamFight = false,
    HeroBook = false,
    Navigate = false,

    FlyText = false,
    OperationActivity = false,
    AutoAI  = false,

}


----------------------------------------------------------
--常驻内存UI列表
----------------------------------------------------------
Local.UIPersistentList ={
    "dlgjoystick",
    "dlgopenloading",
    "dlgtask",
    "dlgtasktalk",
    "dlgtaskreward",
    "dlgdialog",
    "dlgflytext",
    "dlguimain",
    "ectype.dlguiectype",
    "dlgcombatpower",
    "dlgloading",
    "dlgmonster_hp",
    "dlgheadtalking",
    "dlghiding",
}

----------------------------------------------------------
--主城页面List
----------------------------------------------------------
Local.MaincityDlgList ={
    "dlgjoystick",
}
Local.EctypeDlgList = {
    "dlgjoystick",
    "ectype.dlguiectype",
}
Local.ClimbingTowerDlgList ={
    "dlgjoystick",
    "ectype.dlgclimbingtower",
}
Local.GuardTowerDlgList ={
    "dlgjoystick",
    "ectype.guardtower.dlgguardtower",
}
Local.TeamFightDlgList = {
    "dlgjoystick",
    "ectype.dlgathletics3v3",
}

----------------------------------------------------------
--scene strpping
----------------------------------------------------------
Local.SceneStripping = {
    EnableStripping = false,

    StrppingMoveInterval = 7,
    RoleActiveAreaRadius = 180,
    CheckActiveAreaDist = 100,

    StrppingThresholdMap = {
        scene_light = 400,
        scene_ground = 300,
        scene_building = 70,
        scene_stairs = 60,
        scene_cloud = 100,
        scene_water = 100,
        scene_tree = 100,
        scene_grass = 50,
        scene_stone = 100,
        scene_sfx = 50,
        scene_sound = 300,
    },

    DefaultStrppingThreshold = 60,
    --StrppingCamRotateInterval = 10,
    --StrppingCamPullInterval = 2,

    IgnoreSceneList = {
        "login",
        "transit",
        "maincity_08",
    },
}
