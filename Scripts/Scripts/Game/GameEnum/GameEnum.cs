using System;
using UnityEngine;

/// <summary>
/// <see cref="QualitySettings.SetQualityLevel(int)"/>에 설정된 Index와 매칭시켜서 사용해야함!
/// </summary>
public enum E_Quality
{
	Creation = 0,
	Low = 1,
	Midium = 2,
	High = 3,
	VeryHigh = 4,
}

public enum E_DefaultCharacterType
{
    None = 0,
    PlayerWomen,
    PlayerMen,
    MonsterNormal_1,
    MonsterBig_1,
}

public enum E_StageID
{
    None = 0,
    TownMap101_Main,
    TownMap101_Sub1,

    Map101_Main,
    Map101_Sub1,

    Temple_0001_Main,
    Temple_0001_Sub1,

    Temple_0002_Main,
    Temple_0002_Sub1,

    Temple_0003_Main,
    Temple_0003_Sub1,

    Temple_0004_Main,
    Temple_0004_Sub1,

    Temple_1001_Main,
    Temple_1001_Sub1,

    Temple_1002_Main,
    Temple_1002_Sub1,

    Temple_1003_Main,
    Temple_1003_Sub1,

    Temple_1004_Main,
    Temple_1004_Sub1,

    Temple_1007_Main,
    Temple_1007_Sub1,

    Temple_1008_Main,
    Temple_1008_Sub1,

    Temple_1009_Main,
    Temple_1009_Sub1,

    Temple_1011_Main,
    Temple_1011_Sub1,

    Temple_1013_Main,
    Temple_1013_Sub1,

    Temple_3000_Main,
    Temple_3000_Sub1,

	Map103_Main,
	Map103_Sub1,

    DespairTower101_Main,
    DespairTower101_Sub1,

    Map102_Main,
    Map102_Sub1,

    PvPMap101_Main,
    PvPMap101_Sub1,

    SparringMap101_Main,
    SparringMap101_Sub1,

    Temple_1014_Main,
    Temple_1014_Sub1,

    Temple_2000_Main,
    Temple_2000_Sub1,

    Temple_1017_Main,
    Temple_1017_Sub1,

    Temple_1018_Main,
    Temple_1018_Sub1,

    Temple_1019_Main,
    Temple_1019_Sub1,

    TrialTower101_Main,
    TrialTower101_Sub1,

}

public enum E_WorldID
{
    None = 0,
    World_TownTest,
}


public enum E_SceneType
{
    None,
    TestSeamlessMain,
}

public enum E_SeamlessDirection
{
    Left = 0,
    Right,
    Up,
    Down,
}

public enum E_MapMoveType
{
    Walk,
    Teleport_Danger,
    Teleport_Safe,
}

public enum E_PoolType
{
    Default,
    Effect,
    Character,
    UI,
    UI_NameTag,  // 네임 태그 전용 : 메모리 언로드 필요없음
}


public class GameEnum 
{
    public static ENUM ConvertEnum<ENUM>(string String)
    {
        try
        {
            return (ENUM)Enum.Parse(typeof(ENUM), String, false);
        }
        catch (ArgumentException)
        {
            Debug.Log("[Enum]=== Convert Fail : " + String);
            object Dummy = 0;
            return (ENUM)Dummy;
        }
    }
}
