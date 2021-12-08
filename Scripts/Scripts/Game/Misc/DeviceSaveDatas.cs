using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZNet.Data;

/// <summary>
/// [wisreal][2019.03.11]
/// 
/// PlayerPref 관리하기 위한 중간역할
/// </summary>

public class DeviceSaveDatas
{
	public static string GameCode = "ICarus";
    public static string UserCode = "";//필요에 따라 조합해서 쓸수 있도록...

	#region KEY
	public const string KEY_LastQuitDate = "LastApplicationQuit_DATE";

    //카메라
    public const string KEY_CAMERA_MOTOR_TYPE = "KEY_CAMERA_MOTOR_TYPE";
	public const string KEY_IS_FIXED_CAM = "KEY_IS_FIXED_CAM";
	public const string KEY_FIXED_CAM_ORBIT_X = "FIXED_CAM_ORBIT_X";
	public const string KEY_FIXED_CAM_ORBIT_Y = "FIXED_CAM_ORBIT_Y";
	public const string KEY_FIXED_CAM_ORBIT_ZOOM = "KEY_FIXED_CAM_ORBIT_ZOOM";

    //성지
    public const string KEY_GODLAND_BATTLERECORD_LIST = "KEY_GODLAND_BATTLERECORD_LIST";
    #endregion

    static string DeviceID{
        get{ return SystemInfo.deviceUniqueIdentifier; }
    }

    static string GetSaveKey(string SaveKey)
    {
        return string.Format("{0}_{1}_{2}_{3}", DeviceID,GameCode, UserCode, SaveKey);
    }

    public static void SaveData(string Key,string Value)
    {
        PlayerPrefs.SetString(GetSaveKey(Key), Value);
    }
    public static void SaveData(string Key, int Value)
    {
        PlayerPrefs.SetInt(GetSaveKey(Key), Value);
    }

    public static void SaveData(string Key, float Value)
    {
        PlayerPrefs.SetFloat(GetSaveKey(Key), Value);
    }

    public static void SaveData(string Key, bool Value)
    {
        PlayerPrefs.SetInt(GetSaveKey(Key), Value ? 1 : 0);
    }

    public static void SaveData(string Key, List<int> list)
    {
        string saveStr = string.Empty;
        for (int i = 0; i < list.Count; ++i) {
            if (i != list.Count - 1) {
                saveStr += list[i] + "*";
            }
            else {
                saveStr += list[i];
            }
        }

        PlayerPrefs.SetString(GetSaveKey(Key), saveStr);
    }

    public static string LoadData(string Key, string DefaultValue)
    {
        return PlayerPrefs.GetString(GetSaveKey(Key), DefaultValue);
    }

    public static int LoadData(string Key, int DefaultValue)
    {
        return PlayerPrefs.GetInt(GetSaveKey(Key), DefaultValue);
    }

    public static float LoadData(string Key, float DefaultValue)
    {
        return PlayerPrefs.GetFloat(GetSaveKey(Key), DefaultValue);
    }

    public static bool LoadData(string Key, bool DefaultValue)
    {
        return PlayerPrefs.GetInt(GetSaveKey(Key), DefaultValue ? 1 : 0) == 1;
    }

    public static List<int> LoadData(string Key)
    {
        List<int> list = new List<int>();
        string data = PlayerPrefs.GetString(GetSaveKey(Key), string.Empty);

        if( string.IsNullOrEmpty(data) ) {
            return list;
        }

        var dataSplit = data.Split("*".ToCharArray());
        for (int i = 0; i < dataSplit.Length; ++i) {
            list.Add(int.Parse(dataSplit[i]));
        }
        return list;
    }

    #region CharacterData
    public static void SaveCharacterData(ulong CharID, string Key, string Value)
    {
        PlayerPrefs.SetString(string.Format("{0}_{1}", GetSaveKey(Key), CharID), Value);
    }
    public static void SaveCharacterData(ulong CharID, string Key, bool Value)
    {
        PlayerPrefs.SetInt(string.Format("{0}_{1}", GetSaveKey(Key), CharID), Value ? 1 : 0);
    }
    public static void SaveCharacterData(ulong CharID, string Key, float Value)
    {
        PlayerPrefs.SetFloat(string.Format("{0}_{1}", GetSaveKey(Key), CharID), Value);
    }
    public static void SaveCharacterData(ulong CharID, string Key, int Value)
    {
        PlayerPrefs.SetInt(string.Format("{0}_{1}", GetSaveKey(Key), CharID), Value);
    }

    public static string LoadCharacterData(ulong CharID, string Key, string DefaultValue)
    {
        return PlayerPrefs.GetString(string.Format("{0}_{1}", GetSaveKey(Key), CharID), DefaultValue);
    }
    public static bool LoadCharacterData(ulong CharID, string Key, bool DefaultValue)
    {
        return PlayerPrefs.GetInt(string.Format("{0}_{1}", GetSaveKey(Key), CharID), DefaultValue ? 1 : 0) == 1;
    }
    public static float LoadCharacterData(ulong CharID, string Key, float DefaultValue)
    {
        return PlayerPrefs.GetFloat(string.Format("{0}_{1}", GetSaveKey(Key), CharID), DefaultValue);
    }
    public static int LoadCharacterData(ulong CharID, string Key, int DefaultValue)
    {
        return PlayerPrefs.GetInt(string.Format("{0}_{1}", GetSaveKey(Key), CharID), DefaultValue);
    }
        
    static Dictionary<ulong, Dictionary<string, List<string>>> CharacterKeyList = new Dictionary<ulong, Dictionary<string, List<string>>>();
    public static List<ulong> GetCharacterKeyValue(ulong CharID, string TypeKey)
    {
        if (!CharacterKeyList.ContainsKey(CharID))
            CharacterKeyList.Add(CharID, new Dictionary<string, List<string>>());
        if (!CharacterKeyList[CharID].ContainsKey(TypeKey))
        {
            CharacterKeyList[CharID].Add(TypeKey, new List<string>());
        }

        return CharacterKeyList[CharID][TypeKey].ConvertAll(item=>ulong.Parse(item));
    }

    public static List<string> GetCharacterKeyStrValue(ulong CharID, string TypeKey)
    {
        if (!CharacterKeyList.ContainsKey(CharID))
            CharacterKeyList.Add(CharID, new Dictionary<string, List<string>>());
        if (!CharacterKeyList[CharID].ContainsKey(TypeKey))
        {
            CharacterKeyList[CharID].Add(TypeKey, new List<string>());
        }

        return CharacterKeyList[CharID][TypeKey];
    }

    public static bool HasCharacterKey(ulong CharID,string TypeKey, ulong ValueKey)
    {
        if (!CharacterKeyList.ContainsKey(CharID))
            CharacterKeyList.Add(CharID, new Dictionary<string, List<string>>());
        if (!CharacterKeyList[CharID].ContainsKey(TypeKey))
        {
            CharacterKeyList[CharID].Add(TypeKey, new List<string>());
        }

        return CharacterKeyList[CharID][TypeKey].Contains(ValueKey.ToString());
    }

    public static bool HasCharacterKey(ulong CharID, string TypeKey, string ValueKey)
    {
        if (!CharacterKeyList.ContainsKey(CharID))
            CharacterKeyList.Add(CharID, new Dictionary<string, List<string>>());
        if (!CharacterKeyList[CharID].ContainsKey(TypeKey))
        {
            CharacterKeyList[CharID].Add(TypeKey, new List<string>());
        }

        return CharacterKeyList[CharID][TypeKey].Contains(ValueKey);
    }

    public static bool AddCharacterKey(ulong CharID, string TypeKey, ulong ValueKey)
    {
        if (!CharacterKeyList.ContainsKey(CharID))
            CharacterKeyList.Add(CharID, new Dictionary<string, List<string>>());
        if (!CharacterKeyList[CharID].ContainsKey(TypeKey))
        {
            CharacterKeyList[CharID].Add(TypeKey, new List<string>());
        }

        if (!CharacterKeyList[CharID][TypeKey].Contains(ValueKey.ToString()))
        {
            CharacterKeyList[CharID][TypeKey].Add(ValueKey.ToString());
            return true;
        }

        return false;
    }

    public static bool AddCharacterKey(ulong CharID, string TypeKey, string ValueKey)
    {
        if (!CharacterKeyList.ContainsKey(CharID))
            CharacterKeyList.Add(CharID, new Dictionary<string, List<string>>());
        if (!CharacterKeyList[CharID].ContainsKey(TypeKey))
        {
            CharacterKeyList[CharID].Add(TypeKey, new List<string>());
        }

        if (!CharacterKeyList[CharID][TypeKey].Contains(ValueKey))
        {
            CharacterKeyList[CharID][TypeKey].Add(ValueKey);
            return true;
        }

        return false;
    }

    public static void RemoveCharacterKey(ulong CharID, string TypeKey, ulong ValueKey)
    {
        if (!CharacterKeyList.ContainsKey(CharID))
            CharacterKeyList.Add(CharID, new Dictionary<string, List<string>>());
        if (!CharacterKeyList[CharID].ContainsKey(TypeKey))
        {
            CharacterKeyList[CharID].Add(TypeKey, new List<string>());
        }

        if (CharacterKeyList[CharID][TypeKey].Contains(ValueKey.ToString()))
        {
            CharacterKeyList[CharID][TypeKey].Remove(ValueKey.ToString());
        }
    }

    public static void RemoveCharacterKey(ulong CharID, string TypeKey, string ValueKey)
    {
        if (!CharacterKeyList.ContainsKey(CharID))
            CharacterKeyList.Add(CharID, new Dictionary<string, List<string>>());
        if (!CharacterKeyList[CharID].ContainsKey(TypeKey))
        {
            CharacterKeyList[CharID].Add(TypeKey, new List<string>());
        }

        if (CharacterKeyList[CharID][TypeKey].Contains(ValueKey))
        {
            CharacterKeyList[CharID][TypeKey].Remove(ValueKey);
        }
    }
    #endregion

    #region CurCharacterData
    public static void SaveCurCharData(string Key, string Value)
    {
        SaveCharacterData(Me.CharID, Key, Value);
    }
    public static void SaveCurCharData(string Key, bool Value)
    {
        SaveCharacterData(Me.CharID, Key, Value);
    }
    public static void SaveCurCharData(string Key, float Value)
    {
        SaveCharacterData(Me.CharID, Key, Value);
    }
    public static void SaveCurCharData(string Key, int Value)
    {
        SaveCharacterData(Me.CharID, Key, Value);
    }

    public static string LoadCurCharData(string Key, string DefaultValue)
    {
        return LoadCharacterData(Me.CharID, Key, DefaultValue);
    }
    public static bool LoadCurCharData(string Key, bool bDefaultValue)
    {
        return LoadCharacterData(Me.CharID, Key, bDefaultValue);
    }
    public static float LoadCurCharData(string Key, float DefaultValue)
    {
        return LoadCharacterData(Me.CharID, Key, DefaultValue);
    }
    public static int LoadCurCharData(string Key, int DefaultValue)
    {
        return LoadCharacterData(Me.CharID, Key, DefaultValue);
    }
    #endregion

    #region User
    static Dictionary<ulong, Dictionary<string, List<string>>> UserKeyList = new Dictionary<ulong, Dictionary<string, List<string>>>();
 
    public static bool AddUserKey(string TypeKey, ulong ValueKey)
    {
        if (!UserKeyList.ContainsKey(Me.UserID))
            UserKeyList.Add(Me.UserID, new Dictionary<string, List<string>>());
        if (!UserKeyList[Me.UserID].ContainsKey(TypeKey))
        {
            UserKeyList[Me.UserID].Add(TypeKey, new List<string>());
        }

        if (!UserKeyList[Me.UserID][TypeKey].Contains(ValueKey.ToString()))
        {
            UserKeyList[Me.UserID][TypeKey].Add(ValueKey.ToString());
            return true;
        }

        return false;
    }

    public static bool AddUserKey(string TypeKey, string ValueKey)
    {
        if (!UserKeyList.ContainsKey(Me.UserID))
            UserKeyList.Add(Me.UserID, new Dictionary<string, List<string>>());
        if (!UserKeyList[Me.UserID].ContainsKey(TypeKey))
        {
            UserKeyList[Me.UserID].Add(TypeKey, new List<string>());
        }

        if (!UserKeyList[Me.UserID][TypeKey].Contains(ValueKey))
        {
            UserKeyList[Me.UserID][TypeKey].Add(ValueKey);
            return true;
        }

        return false;
    }

    #endregion

    #region Timer
    public static void SaveDataTimeCheck(string Key, bool Value)
    {
        PlayerPrefs.SetString(GetSaveKey(Key),string.Format("{0}:{1}",System.DateTime.Now.Ticks.ToString(),(Value ? 1 : 0)));
    }

    public static bool LoadDataTimeCheck(string Key,float fTimeOut)
    {
        string readValue = PlayerPrefs.GetString(GetSaveKey(Key), "");

        if (string.IsNullOrEmpty(readValue))
            return false;

        string[] splits = readValue.Split(':');
        System.DateTime ftime = new System.DateTime(long.Parse(splits[0]));
        if (System.DateTime.Now > ftime.AddSeconds(fTimeOut))
        {
            return false;
        }
        return int.Parse(splits[1]) == 1;
    }
    #endregion
}
