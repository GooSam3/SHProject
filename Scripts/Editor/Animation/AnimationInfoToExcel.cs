using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

using System.Linq;

/// <summary> FBX안에 존재하는 이벤트 정보들 저장용 </summary>
class AnimEventData
{
    public string ClipName;
    /// <summary></summary>
	public float Length;
    /// <summary></summary>
	public List<float> InvokeTimings = new List<float>();

    public List<float> EffectTimings = new List<float>();
}

/// <summary> 리소스 테이블의 애니메이션 이름 셋팅 및 애니메이션 테이블에 invoke time 셋팅 툴 </summary>
public class AnimationInfoToExcel : EditorWindow
{
    //1. 리소스 테이블에서 캐릭터 프리펩 이름으로 프리펩 로드
    //2. 로드된 프리펩에서 Animator OverrideContorller에 셋팅된 애니메이션들 얻어옴
    //3. 얻어온 애니메이션을 리소스 테이블에 셋팅
    //4. 얻어온 애니메이션의 Event정보에 Invoke 타이밍을 애니메이션 테이블에 셋팅

    /// <summary> 리소스 테이블 이름 </summary>
    private const string mResourceTableFileName = "Resource_table.xlsx";
    /// <summary> 애니메이션 테이블 이름 </summary>
    private const string mAnimationTableFileName = "Animation_table.xlsx";

    /// <summary><see cref="GameDB.Animation_Table"/> 파일이 있는 경로 </summary>
    private string mTablePath;

    /// <summary>FBX에 설정한 Invoke이벤트 찾을 함수 이름</summary>
	private const string mInvokeFunctionName = "Invoke";

    /// <summary>FBX에 설정한 Effect이벤트 찾을 함수 이름</summary>
	private const string mEffectFunctionName = "Effect";

    private List<AnimEventData> m_listAnimEventData = new List<AnimEventData>();

    /// <summary> 중복된 클립 처리 </summary>
    private List<string> m_listSkipClipName = new List<string>();

    StringBuilder LogBuilder { get; set; } = new StringBuilder();

    [MenuItem("ZGame/Tools/Animation Info Extrator")]
    static void Init()
    {
        var window = (AnimationInfoToExcel)EditorWindow.GetWindow(typeof(AnimationInfoToExcel));
        window.titleContent = new GUIContent("[Extrator] AnimationEvent To Excel");
        window.minSize = new Vector2(450, 470);
    }

    void OnEnable()
    {
        mTablePath = EditorPrefs.GetString($"{nameof(mTablePath)}", string.Empty);
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);

        //EditorGUILayout.LabelField($"읽어올 FBX 경로 [{FBXsPathInProject}]");

        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.TextArea(LogBuilder.ToString(), ZGUIStyles.RichText, GUILayout.Height(300));
        }
        GUILayout.EndHorizontal();

        ZGUIStyles.Separator();

        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("FBX -> Xlsx(GameDB)", EditorStyles.whiteLargeLabel);
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            //if(GUILayout.Button("xxxxxxxxxxxxxxxxxxx"))
            //{
            //    CopyBattleIdle();
            //    return;
            //}
            if (string.IsNullOrEmpty(mTablePath))
            {
                EditorGUILayout.LabelField($"Excel Path : 선택해주세요!");
            }
            else
            {
                EditorGUILayout.LabelField($"Excel Path : {mTablePath}");
            }

            if (GUILayout.Button("파일 선택", GUILayout.Width(100)))
            {
                mTablePath = EditorUtility.OpenFolderPanel($"Select Table Folder ", Application.dataPath, "");
                EditorPrefs.SetString($"{nameof(mTablePath)}", mTablePath);

                AddLog($"테이블 경로 설정 : {mTablePath}");
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(mTablePath));
        {
            if (GUILayout.Button(string.Format($"AnimationInfo to Excel"), GUILayout.Height(80)))
            {
                ExportAnimationToTable();
            }
        }


        EditorGUI.EndDisabledGroup();

        GUILayout.EndVertical();
    }
#endif

    void ExportAnimationToTable()
    {
        if (string.IsNullOrEmpty(mTablePath))
        {
            AddLog($"{mTablePath} 경로가 존재하지 않습니다.", Color.red);
            return;
        }

        m_listAnimEventData.Clear();
        m_listSkipClipName.Clear();
        //리소스 테이블 읽기 및 쓰기
        ReadNWriteInfoToResourceExcelFile();
        //애니메이션 테이블에 저장
        WriteInfoToAnimationExcelFile();

        EditorUtility.ClearProgressBar();
    }
        
    private ExcelPackage GetExcelPackage(string fileName)
    {
        string excelFilePath = $"{mTablePath}/{fileName}";

        if (FileHelper.IsFileinUse(new FileInfo(excelFilePath)))
        {
            AddLog($"{excelFilePath} 이 수정가능한지 확인바람!!(파일이 열려있을 가능성 높음)", Color.red);
            return null;
        }
        var exp = new ExcelPackage(new FileInfo(excelFilePath));
        if (exp.Workbook.Worksheets.Count <= 0)
        {
            AddLog($"Worksheet가 존재하지 않습니다.", Color.red);
            return null;
        }

        return exp;
    }

    /// <summary> 배틀 idle 카피용. </summary>
    private void CopyBattleIdle()
    {
        var assetPaths = Directory.GetFiles("Assets/_ZAssetBundle/Character/", "*.prefab", SearchOption.AllDirectories);
        string animFilePath = "Assets/IcarusSource/Character";
        List<string> animFilePaths = Directory.GetFiles(animFilePath, "*.anim", SearchOption.AllDirectories).ToList();
        foreach (var path in assetPaths)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var go = (asset as GameObject);
            var animator = go.GetComponent<Animator>();

            if (null == animator)
                continue;

            var controller = animator.runtimeAnimatorController as AnimatorOverrideController;

            if (null == controller)
                continue;

            List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            controller.GetOverrides(overrideClips);
            
            var idleClip = controller[E_AnimStateName.Idle_001.ToString()];            
            var battleIdleClip = controller[E_AnimStateName.BattleIdle_001.ToString()];
            
            if (null == idleClip)
                continue;
            string clipName = idleClip.name;
            
            clipName = clipName.Replace("_Idle", "_BattleIdle");
                        
            string battleIdlePath = animFilePaths.Find((animPath) => animPath.Contains(clipName));

            if (string.IsNullOrEmpty(battleIdlePath))
                continue;

            for(int i = 0; i < overrideClips.Count; ++i)
            {
                if (false == overrideClips[i].Key.name.Contains(E_AnimStateName.BattleIdle_001.ToString()))
                    continue;

                var animationClip = AssetDatabase.LoadAssetAtPath(battleIdlePath, typeof(AnimationClip)) as AnimationClip;

                overrideClips[i] = new KeyValuePair<AnimationClip, AnimationClip>(battleIdleClip, animationClip);
            }
            
            controller.ApplyOverrides(overrideClips);

        }          
    }

    void ReadNWriteInfoToResourceExcelFile()
    {
        ExcelPackage exp = GetExcelPackage(mResourceTableFileName);

        if (null == exp)
            return;

        int resourceFile = 0;
        int attack_001 = 0;
        int attack_002 = 0;
        int attack_003 = 0;
        int skill_001 = 0;
        int skill_002 = 0;
        int casting = 0;
        int rush = 0;
        int leap = 0;
        int pull = 0;

        int buff_001 = 0;        

        EditorUtility.DisplayProgressBar("AnimationInfo Write to Resource Table", "Progressing..", 0f);

        Dictionary<string, List<int>> rowInfos = new Dictionary<string, List<int>>();

        //모든 프리펩 로드
        var assetPaths = Directory.GetFiles("Assets/_ZAssetBundle/Character/", "*.prefab", SearchOption.AllDirectories);

        foreach (ExcelWorksheet sheet in exp.Workbook.Worksheets)
        {
            if (!sheet.Name.Equals("Resource_Table"))
                continue;

            ExcelCellAddress startAddr = sheet.Dimension.Start;
            ExcelCellAddress endAddr = sheet.Dimension.End;
            
            for (int row = startAddr.Row; row <= endAddr.Row; row++)
            {
                if (row == startAddr.Row)
                {
                    for (int col = startAddr.Column; col <= endAddr.Column; col++)
                    {
                        string colName = sheet.GetValue<string>(row, col);
                        if (string.IsNullOrEmpty(colName))
                            continue;

                        switch (colName)
                        {
                            case "ResourceFile": resourceFile = col; break;
                            case "AttackAni_01": attack_001 = col; break;
                            case "AttackAni_02": attack_002 = col; break;
                            case "AttackAni_03": attack_003 = col; break;
                            case "SkillAni_01": skill_001 = col; break;
                            case "SkillAni_02": skill_002 = col; break;
                            case "BuffAni_01": buff_001 = col; break;
                            case "CastingAni": casting = col; break;
                            case "RushAni": rush = col; break;
                            case "LeapAni": leap = col; break;
                            case "PullAni": pull = col; break;
                        }
                    }
                }
                else
                {
                    string resourceFileName = sheet.GetValue<string>(row, resourceFile);

                    if (string.IsNullOrEmpty(resourceFileName))
                        continue;

                    foreach(var path in assetPaths)
                    {
                        if(path.Contains(resourceFileName + ".prefab"))
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                            var go = (asset as GameObject);
                            var animator = go.GetComponent<Animator>();

                            if(null == animator)
							{
                                AddLog($"{asset.name} Animator가 셋팅되어있지 않습니다.", Color.yellow);
                                continue;
							}

                            var controller = animator.runtimeAnimatorController as AnimatorOverrideController;

                            if(null == controller)
                            {
                                AddLog($"{asset.name} 애니메이션 컨트롤러가 셋팅되어있지 않습니다.", Color.yellow);
                                continue;
                            }
                            List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                            controller.GetOverrides(overrideClips);
                            
                            SaveClip(sheet, row, attack_001, controller, E_AnimStateName.Attack_001, ref overrideClips);
                            SaveClip(sheet, row, attack_002, controller, E_AnimStateName.Attack_002, ref overrideClips);
                            SaveClip(sheet, row, attack_003, controller, E_AnimStateName.Attack_003, ref overrideClips);                            
                            SaveClip(sheet, row, skill_001, controller, E_AnimStateName.Skill_001, ref overrideClips);
                            SaveClip(sheet, row, skill_002, controller, E_AnimStateName.Skill_002, ref overrideClips);                            
                            SaveClip(sheet, row, buff_001, controller, E_AnimStateName.Buff_001, ref overrideClips);

                            SaveClip(sheet, row, casting, controller, E_AnimStateName.SkillCast_End_001, ref overrideClips);
                            SaveClip(sheet, row, rush, controller, E_AnimStateName.SkillRush_End_001, ref overrideClips);
                            SaveClip(sheet, row, leap, controller, E_AnimStateName.SkillLeap_End_001, ref overrideClips);
                            SaveClip(sheet, row, pull, controller, E_AnimStateName.SkillPull_001, ref overrideClips);

                            break;
                        }
                    }
                }

                EditorUtility.DisplayProgressBar("AnimationInfo Write to Resource Table", "Progressing..", (float)row / (float)endAddr.Row);
            }
        }

        exp.Save();
        exp.Dispose();
    }
        
    void SaveClip(ExcelWorksheet sheet, int row, int col, AnimatorOverrideController controller, E_AnimStateName name, ref List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips)
    {
        var clip = controller[name.ToString()];

        string clipName = "";

        //오버라이드되지 않은 클립은 스킵
        if (false == overrideClips.Contains(new KeyValuePair<AnimationClip, AnimationClip>(clip, null)))
        {            
            if (null != clip)
            {
                clipName = clip.name;

                if (m_listSkipClipName.Contains(clipName))
                {
                    sheet.SetValue(row, col, clipName);
                    return;
                }

                m_listSkipClipName.Add(clipName);

                AnimationEvent[] animEvents = AnimationUtility.GetAnimationEvents(clip);
                var eventData = new AnimEventData();
                eventData.ClipName = clipName;
                eventData.Length = clip.length;

                if (0 < animEvents.Length)
                {
                    // 타이밍 빠른순으로 정렬해서 넣기
                    System.Array.Sort(animEvents, (AnimationEvent a1, AnimationEvent a2) => { return a1.time.CompareTo(a2.time); });

                    foreach (AnimationEvent animEvt in animEvents)
                    {
                        Debug.Log($"FBX파일 애니메이션에 존재하는 이벤트 | AnimName: {eventData.ClipName}, AnimLength: {eventData.Length}, EventFunctionName: {animEvt.functionName}, Timing: {animEvt.time}");

                        if (animEvt.functionName.Equals(mInvokeFunctionName, System.StringComparison.CurrentCultureIgnoreCase))
                        {
                            eventData.InvokeTimings.Add(animEvt.time);
                        }
                        else if(animEvt.functionName.Equals(mEffectFunctionName, System.StringComparison.CurrentCultureIgnoreCase))
                        {
                            eventData.EffectTimings.Add(animEvt.time);
                        }
                        else
                        {
                            AddLog($"{clipName} FBX에 약속되지 않은 이벤트 Function : {animEvt.functionName}이 존재합니다.", Color.yellow);
                        }
                    }
                }

                m_listAnimEventData.Add(eventData);
            }
        }
        

        sheet.SetValue(row, col, clipName);
    }

    void WriteInfoToAnimationExcelFile()
    {
        if (m_listAnimEventData.Count <= 0)
        {
            AddLog($"이벤트 정보가 하나도 없습니다.", Color.red);
            return;
        }

        ExcelPackage exp = GetExcelPackage(mAnimationTableFileName);

        if (null == exp)
            return;

        //컬럼의 인덱스
        int nameColunmIdx = 0;
        int animLengthColunmIdx = 0;
        int invokeCountColunmIdx = 0;
        int invokeTime1ColunmIdx = 0;
        int effectTimingColunmIdx = 0;

        EditorUtility.DisplayProgressBar("AnimationEvent Write to Excel", "Progressing..", 0f);

        Dictionary<string, List<int>> rowInfos = new Dictionary<string, List<int>>();
        foreach (ExcelWorksheet sheet in exp.Workbook.Worksheets)
        {
            if (!sheet.Name.Equals("Animation_Table"))
                continue;

            ExcelCellAddress startAddr = sheet.Dimension.Start;
            ExcelCellAddress endAddr = sheet.Dimension.End;

            sheet.DeleteRow(startAddr.Row + 1, endAddr.Row - startAddr.Row);
            sheet.InsertRow(startAddr.Row + 1, m_listAnimEventData.Count);
            endAddr = sheet.Dimension.End;

            for (int row = startAddr.Row; row <= startAddr.Row + m_listAnimEventData.Count; row++)
            {
                if (row == startAddr.Row)
                {
                    for (int col = startAddr.Column; col <= endAddr.Column; col++)
                    {
                        string colName = sheet.GetValue<string>(row, col);
                        if (string.IsNullOrEmpty(colName))
                            continue;

                        switch (colName)
                        {
                            case "AnimationID": nameColunmIdx = col; break;
                            case "AnimationLength": animLengthColunmIdx = col; break;
                            case "InvokeCount": invokeCountColunmIdx = col; break;
                            case "InvokeTiming_01": invokeTime1ColunmIdx = col; break;
                            case "EffectTiming": effectTimingColunmIdx = col; break;
                        }
                    }
                }
                else
                {
                    var eventData = m_listAnimEventData[row - startAddr.Row - 1];

                    sheet.SetValue(row, nameColunmIdx, eventData.ClipName);
                    sheet.SetValue(row, animLengthColunmIdx, eventData.Length);
                    sheet.SetValue(row, invokeCountColunmIdx, eventData.InvokeTimings.Count);                    
                    for (int i = 0; i < 5; i++)
                    {
                        if (i < eventData.InvokeTimings.Count)
                        {
                            // 소수점 5자리까지만 넣기
                            double invokeTime = System.Math.Round(eventData.InvokeTimings[i], 5);
                            sheet.SetValue(row, invokeTime1ColunmIdx + i, invokeTime);
                        }
                        else
                        {
                            sheet.SetValue(row, invokeTime1ColunmIdx + i, null);
                        }
                    }
                    StringBuilder strBuilder = new StringBuilder();
                    foreach(var timing in eventData.EffectTimings)
                    {
                        strBuilder.Append(timing);
                        strBuilder.Append(",");
                    }

                    if(strBuilder.Length > 0)
                    {
                        strBuilder.Replace(",", "", strBuilder.Length - 1, 1);
                        sheet.SetValue(row, effectTimingColunmIdx, strBuilder.ToString());
                    }   
                }

                EditorUtility.DisplayProgressBar("AnimationEvent Write to Excel", "Progressing..", (float)row / (float)endAddr.Row);
            }
        }

        exp.Save();
        exp.Dispose();

        LogBuilder.Clear();

        AddLog($"저장 성공!", Color.cyan);
    }

    void AddLog(string context, Color? color = null)
    {
        if (color.HasValue)
        {
            LogBuilder.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(color.Value)}>{context}</color>");
        }
        else
        {
            LogBuilder.AppendLine(context);
        }
    }
}