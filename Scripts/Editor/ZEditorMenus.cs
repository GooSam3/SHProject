using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class ZEditorMenus
{
	[MenuItem("ZGame/Misc/DeleteAll PlayerPrefs")]
	static void DeleteAllPlayerPref()
	{
		PlayerPrefs.DeleteAll();
	}

	[MenuItem("ZGame/Misc/CleanCache")]
	public static void CleanCache()
	{
		bool bResult = Caching.ClearCache();

		Debug.Log($"ClearCache | result is {bResult}");
	}

	[MenuItem("GameObject/Zero/Post-process Volume")]
	static void CreatePPVolume(MenuCommand menuCommand)
	{
		var existVolumes = GameObject.FindObjectsOfType<Volume>();
		if (null != existVolumes && existVolumes.Length > 0)
		{
			string message = existVolumes.Length == 1
				? $"이미 맵상에 PostProcessVolume이 존재합니다."
				: $"씬에는 오로지 1개의 Volume만 존재해야합니다. 나머지는 삭제해주세요.";

			if (EditorUtility.DisplayDialog("경고 (문제 발생시 개발자에게 문의)", message, "OK"))
			{
				return;
			}
		}

		var gameObject = new GameObject("Post-process Volume");
		// Layer 강제 설정
		gameObject.layer = UnityConstants.Layers.PostProcessing;
		Volume ppv = gameObject.AddComponent<Volume>();
		// 글로벌 적용
		ppv.isGlobal = true;

		Selection.objects = new[] { gameObject };
		EditorApplication.ExecuteMenuItem("GameObject/Move To View");
	}

	static void Toggle_EditorDevelopmentMode(bool enable)
	{
		UnityEditor.EditorPrefs.SetBool("DeveloperMode", enable);
	}

    [MenuItem("ZGame/Find(한국어)/Find In Code(한국어)")]
    static void FindKoreanInCode()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        builder.Append("스크립트 이름,컴포넌트 이름,한글");

        int Cnt = 0, total = AssetDatabase.GetAllAssetPaths().Length;

        foreach (string str in AssetDatabase.GetAllAssetPaths())
        {
            if (str.Contains(".cs"))
            {
                if (str.Contains("Editor"))
                    continue;

                MonoScript ms = AssetDatabase.LoadAssetAtPath(str, typeof(MonoScript)) as MonoScript;

                if (ms != null && ms.text != null && !string.IsNullOrEmpty(ms.text))
                {
                    var charArr = ms.text.ToCharArray();

                    bool bBeginString = false;
                    string FindEndText = "";
                    bool bCheckIgnoreText = false;
                    System.Text.StringBuilder strbuilder = new System.Text.StringBuilder();

                    foreach (var c in charArr)
                    {
                        if (!bBeginString)
                        {
                            if (!string.IsNullOrEmpty(FindEndText))
                            {
                                if (FindEndText.Length > 0 && FindEndText[0] == c)
                                {
                                    FindEndText = FindEndText.Remove(0, 1);
                                    Debug.LogWarning($"--- FindEndTex {str}, {c}  remain => {FindEndText} ignoremsg {strbuilder}");
                                }
                                else
                                    strbuilder.Append(c);

                                continue;
                            }

                            if (!bCheckIgnoreText && c == '/')
                            {
                                bCheckIgnoreText = true;
                                continue;
                            }

                            if (bCheckIgnoreText)
                            {
                                if (c == '*')
                                {
                                    Debug.LogWarning($"find /* {str}");
                                    FindEndText = "*/";

                                    strbuilder.Clear();
                                }
                                else if (c == '/')
                                {
                                    Debug.LogWarning($"find // {str}");
                                    FindEndText = "\n";

                                    strbuilder.Clear();
                                }

                                bCheckIgnoreText = false;
                                continue;
                            }

                            if (c == '[')
                            {
                                Debug.LogWarning($"find [ {str}");
                                FindEndText = "]";

                                strbuilder.Clear();
                                continue;
                            }

                            if (c == '(')
                            {
                                var strs = strbuilder.ToString();

                                if ((strs.Length >= 8 && strs.Substring(strs.Length - 8, 8) == "ZLog.Log") ||
                                    (strs.Length >= 15 && strs.Substring(strs.Length - 15, 15) == "ZLog.LogWarning") ||
                                    (strs.Length >= 13 && strs.Substring(strs.Length - 13, 13) == "ZLog.LogError") ||
                                    (strs.Length >= 9 && strs.Substring(strs.Length - 9, 9) == "Debug.Log") ||
                                    (strs.Length >= 16 && strs.Substring(strs.Length - 16, 16) == "Debug.LogWarning") ||
                                    (strs.Length >= 14 && strs.Substring(strs.Length - 14, 14) == "Debug.LogError") ||
                                    (strs.Length >= 15 && strs.Substring(strs.Length - 15, 15) == "GUILayout.Label"))
                                {

                                    Debug.LogWarning($"find ( {str} , {strs}");
                                    FindEndText = ");";
                                    strbuilder.Clear();
                                    continue;
                                }
                            }
                        }

                        //find start text
                        if (c == '"')
                        {
                            bBeginString = !bBeginString;

                            if (bBeginString)
                                strbuilder.Clear();
                            else
                            {
                                foreach (var c1 in strbuilder.ToString().ToCharArray())
                                {
                                    if (char.GetUnicodeCategory(c1) == System.Globalization.UnicodeCategory.OtherLetter)
                                    {
                                        //find
                                        builder.Append($"\n{str},{ms.name},\"{strbuilder.ToString()}\"");
                                        break;
                                    }
                                }
                            }

                            continue;
                        }

                        if (bBeginString)
                            strbuilder.Append(c);
                        else
                        {
                            strbuilder.Append(c);
                            if (c == '\n')
                                strbuilder.Clear();
                        }
                    }
                }
            }
            Cnt++;
            EditorUtility.DisplayProgressBar("텍스트 찾는 중...", $"{string.Format("{0}%", ((float)Cnt / (float)total) * 100f)}", (float)Cnt / (float)total);
        }

        EditorUtility.ClearProgressBar();
        System.IO.File.WriteAllText(Application.dataPath + $"/../코드내한글로케일_{System.DateTime.Now.Year}_{System.DateTime.Now.Month}_{System.DateTime.Now.Day}_{System.DateTime.Now.Hour}_{System.DateTime.Now.Minute}_{System.DateTime.Now.Second}.csv", builder.ToString(), System.Text.Encoding.UTF8);
    }

    [MenuItem("ZGame/Find(한국어)/Find In Prefab(한국어)")]
    static void FindKoreanInPrefab()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        builder.Append("프리팹 이름,컴포넌트 이름,한글");

        int Cnt = 0, total = AssetDatabase.GetAllAssetPaths().Length;

        foreach (string str in AssetDatabase.GetAllAssetPaths())
        {
            if (str.Contains(".prefab"))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(str, typeof(GameObject)) as GameObject;

                foreach (UnityEngine.UI.Text t in go.transform.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                {
                    if (t != null && t.text != null && !string.IsNullOrEmpty(t.text))
                    {
                        var charArr = t.text.ToCharArray();

                        foreach (var c in charArr)
                        {
                            if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherLetter)
                            {
                                //find
                                builder.Append($"\n{str},{t.gameObject.name},\"{t.text}\"");
                                break;
                            }
                        }
                    }
                }
            }
            Cnt++;
            EditorUtility.DisplayProgressBar("텍스트 찾는 중...", $"{string.Format("{0}%", ((float)Cnt / (float)total) * 100f)}", (float)Cnt / (float)total);
        }

        EditorUtility.ClearProgressBar();
        System.IO.File.WriteAllText(Application.dataPath + $"/../한글로케일_{System.DateTime.Now.Year}_{System.DateTime.Now.Month}_{System.DateTime.Now.Day}_{System.DateTime.Now.Hour}_{System.DateTime.Now.Minute}_{System.DateTime.Now.Second}.csv", builder.ToString(), System.Text.Encoding.UTF8);
    }

    [MenuItem("ZGame/기본테이블 데이터 갱신(locale)")]
    static void SetDefaultLocale()
    {
        System.Collections.Generic.List<string> CachePath = new System.Collections.Generic.List<string>();
        Caching.GetAllCachePaths(CachePath);

        string localsavePath = "";
        if (CachePath.Count > 0)
            localsavePath = CachePath[0];

        localsavePath += "/bin/";

        Debug.LogWarning(localsavePath+" , " + System.IO.Directory.Exists(localsavePath));

        if (System.IO.Directory.Exists(localsavePath))
        {
            Debug.LogWarning("Application.dataPath : "+ Application.dataPath);

            //locale 
            if (System.IO.File.Exists(localsavePath + "/Locale_Table.bin"))
                System.IO.File.Copy(localsavePath + "/Locale_Table.bin",Application.dataPath+ "/Resources/GameDB/Locale_Table.bytes",true);
        }
        else
            Debug.LogError("로컬 데이터가 없습니다 : ( "+ localsavePath+" )");
    }
}