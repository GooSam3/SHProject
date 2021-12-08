using System.Collections.Generic;
using System.Text;
using System;
using UnityEditor;
using UnityEngine;
using GameDB;


public class MyPcStatInfoLog : EditorWindow
{
    private class StatInfo
    {
        public E_AbilityType AbilityType;
        public float BaseValue;
        public float CurrentValue;
        public float PreValue;
        public float OffsetValue;

        private StringBuilder StrBuilder = new StringBuilder();

        public StatInfo(E_AbilityType abilityType, float value)
        {
            AbilityType = abilityType;
            BaseValue = value;
            CurrentValue = value;
            OffsetValue = 0f;
            StrBuilder.Clear();
            Set(CurrentValue);
        }

        public bool Set(float currentValue)
        {
            PreValue = CurrentValue;
            OffsetValue = currentValue - CurrentValue;
            CurrentValue = currentValue;

            MakeString();

            return OffsetValue != 0;
        }

        public string GetString()
        {
            return StrBuilder.ToString();
        }

        private void MakeString()
        {
            StrBuilder.Clear();
            StrBuilder.Append($"<{AbilityType}> : ");
            StrBuilder.Append($"기본 [<color=#{ColorUtility.ToHtmlStringRGB(Color.cyan)}>{CurrentValue}</color>], ");
            StrBuilder.Append($"현재 [<color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>{CurrentValue}</color>({PreValue})], ");
            StrBuilder.Append($"변화량 [<color=#{ColorUtility.ToHtmlStringRGB(0 < OffsetValue ? Color.green : Color.red)}>{OffsetValue}</color>]");

            if(AbilityType == E_AbilityType.FINAL_MAX_MAGIC_ATTACK)
            {
                StrBuilder.Append($", 마법 공격력 표시용 계수 [<color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>{DBConfig.MagicAttackViewValue}</color>], ");
            }
        }
    }
    private GUIStyle mUIStyle;
    
    public GUIStyle UIStyle
    {
        get
        {
            if (null == mUIStyle)
            {
                mUIStyle = new GUIStyle(EditorStyles.textArea);
                mUIStyle.richText = true;
                mUIStyle.alignment = TextAnchor.MiddleLeft;

            }
            return mUIStyle;
        }
    }
    private StringBuilder DefaultLogBuilder { get; set; } = new StringBuilder();

    private StringBuilder OrderLogBuilder { get; set; } = new StringBuilder();

    private ZPawnManager PawnManager;
    private ZPawnMyPc MyPc;

    /// <summary> 새로운 데이터가 맨 위로 갱신되는 로그 </summary>
    private List<StatInfo> OrderLogs = new List<StatInfo>();

    [MenuItem("ZGame/Stat/Stat Info For My Pc")]
    static void Init()
    {        
        var window = (MyPcStatInfoLog)EditorWindow.GetWindow(typeof(MyPcStatInfoLog));
        window.titleContent = new GUIContent("Stat Info For My Pc");
        window.minSize = new Vector2(600, 600);
    }

#if UNITY_EDITOR

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeChanged;

        if(ZPawnManager.hasInstance)
            ZPawnManager.Instance.DoRemoveEventCreateMyEntity(HangeEventCreateMyEntity);
    }

    private void HandlePlayModeChanged(PlayModeStateChange mode)
    {
        if(mode != PlayModeStateChange.EnteredPlayMode)
        {
            ExitPlayMode();
        }
    }

    private void ExitPlayMode()
    {
        PawnManager = null;
        MyPc = null;
        DefaultLogBuilder.Clear();        

        OrderLogBuilder.Clear();
        OrderLogs.Clear();
    }
    Vector2 DefaultScrollPos;
    Vector2 OrderScrollPos;
    private void OnGUI()
    {
        if (false == Application.isPlaying)
            return;

        GUILayout.BeginVertical(GUILayout.MinHeight(600));
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        {
            DefaultScrollPos = GUILayout.BeginScrollView(DefaultScrollPos, GUILayout.Height(500));
            {
                EditorGUILayout.TextArea(DefaultLogBuilder.ToString(), UIStyle);
            }
            GUILayout.EndScrollView();
            OrderScrollPos = GUILayout.BeginScrollView(OrderScrollPos, GUILayout.Height(500));
            {
                EditorGUILayout.TextArea(OrderLogBuilder.ToString(), UIStyle);
            }            
            GUILayout.EndScrollView();                    
        }
        GUILayout.EndHorizontal();

        ZGUIStyles.Separator();

        if(null != MyPc)
        {
            if (GUILayout.Button("Clear", GUILayout.Height(80)))
            {
                DefaultLogBuilder.Clear();

                OrderLogBuilder.Clear();
                OrderLogs.Clear();
            }
        }

        EditorGUI.BeginDisabledGroup(false == ZPawnManager.hasInstance || null != PawnManager);
        {
            if (GUILayout.Button("시작!!", GUILayout.Height(80)))
            {
                SetPawnManager();
            }
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.EndVertical();
    }

    private void SetPawnManager()
    {
        if (false == ZPawnManager.hasInstance)
            return;

        DefaultLogBuilder.Clear();
        OrderLogBuilder.Clear();
        PawnManager = ZPawnManager.Instance;

        PawnManager.DoAddEventCreateMyEntity(HangeEventCreateMyEntity);
    }

    private void HangeEventCreateMyEntity()
    {
        DefaultLogBuilder.Clear();
        MyPc = ZPawnManager.Instance.MyEntity;        

        MyPc.DoAddEventStatUpdated(HandleStatUpdated, true);
    }

    private void HandleStatUpdated(Dictionary<E_AbilityType, float> stats)
    {
        foreach(var stat in stats)
        {
            bool bChangeValue = SetOrderStat(stat.Key, stat.Value);
            if(bChangeValue)
            {
                DefaultLogBuilder.AppendLine(OrderLogs[0].GetString());
            }   
        }

        UpdateOrderLog();
    }

    private void UpdateOrderLog()
    {
        OrderLogBuilder.Clear();
        for(int i = 0; i < OrderLogs.Count; ++i)        
        {
            OrderLogBuilder.AppendLine(OrderLogs[i].GetString());                        
        }
    }

    private bool SetOrderStat(E_AbilityType abilityType, float value)
    {
        bool bChangeValue = false;
        int index = OrderLogs.FindIndex(0, (item) =>
        {
            return item.AbilityType == abilityType;
        });

        if (0 <= index)
        {
            var data = OrderLogs[index];
            bChangeValue = data.Set(value);
            if(bChangeValue)
            {
                OrderLogs.Insert(0, data);
                OrderLogs.RemoveAt(index + 1);
            }
        }
        else
        {
            OrderLogs.Insert(0, new StatInfo(abilityType, value));
            bChangeValue = true;
        }

        return bChangeValue;
    }
#endif
}