using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TimeManager))]
public class TimeMgrInspector : Editor
{
	TimeManager mManager;

	private long compareTimeUTS;
	private long diffUtsMs;
	private long diffUtsSec;

    private long convertTimeUTS;
    private System.DateTime convertDateTime;
    private System.TimeSpan difftimeSpan;

	private void OnEnable()
	{
		mManager = this.target as TimeManager;
	}

	private void OnDisable()
	{
		mManager = null;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (null == mManager)
			return;

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Server Now ", TimeManager.NowMs.ToString());
			EditorGUILayout.LabelField("Server Now(Sec) ", TimeManager.NowSec.ToString());
			EditorGUILayout.EndVertical();

			if (GUILayout.Button("CopyToClipboard", GUILayout.Width(114f)))
			{
				GUIUtility.systemCopyBuffer = TimeManager.NowMs.ToString();
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField("DateTime", TimeHelper.Time2DateTimeMs(TimeManager.NowMs).ToString());

		EditorGUILayout.BeginVertical("box");
		{
			EditorGUILayout.LabelField("HalfRTT ", TimeManager.GetHalfRTT().ToString());
		}
		EditorGUILayout.EndVertical();

		//Draw_Separator
		GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

		EditorGUI.BeginChangeCheck();
		{
			EditorGUILayout.HelpBox("현재 시간과 비교할 값을 넣어주세요. :: ComparisonTime - ServerTime", MessageType.Info);
			compareTimeUTS = EditorGUILayout.LongField(new GUIContent("Comparison Time", "현재 시간과 비교할 값을 넣어주세요."), compareTimeUTS);

			EditorGUILayout.LabelField("Difference for Unit(ms)", diffUtsMs.ToString());
			EditorGUILayout.LabelField("Difference for Unit(sec)", diffUtsSec.ToString());

			if (EditorGUI.EndChangeCheck())
			{
				diffUtsMs = compareTimeUTS - (long)TimeManager.NowMs;
				diffUtsSec = compareTimeUTS - (long)TimeManager.NowSec;
			}
		}

        EditorGUI.BeginChangeCheck();
        {
            EditorGUILayout.HelpBox("변환 할 시간. :: ConvertDateTime", MessageType.Info);
            convertTimeUTS = EditorGUILayout.LongField(new GUIContent("ConvertDateTime", "변환할 값을 넣어주세요."), convertTimeUTS);

            EditorGUILayout.LabelField("Convert Date Time",string.Format("{0}/{1}/{2} {3:00}:{4:00}:{5:00}", convertDateTime.Year, convertDateTime.Month, convertDateTime.Day, convertDateTime.Hour, convertDateTime.Minute, convertDateTime.Second));
            EditorGUILayout.LabelField("DiffrentTime(Now)", string.Format("{0} {1:00}:{2:00}:{3:00}", difftimeSpan.Days, difftimeSpan.Hours, difftimeSpan.Minutes, difftimeSpan.Seconds));

            if (EditorGUI.EndChangeCheck())
            {
                convertDateTime = TimeHelper.ParseTimeStamp(convertTimeUTS);
                difftimeSpan = convertDateTime - System.DateTime.Now;
            }
        }
        this.Repaint();
	}
}
