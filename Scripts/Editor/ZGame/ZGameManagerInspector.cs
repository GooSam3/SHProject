using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ZGameManager))]
public class ZGameManagerInspector : Editor
{
    private string mTutorialTid = string.Empty;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (false == Application.isPlaying)
            return;

        if (false == ZPawnManager.hasInstance)
            return;

        if(null != ZPawnManager.Instance.MyEntity)
		{
            GUILayout.Label("튜토리얼 테스트 (퀘스트 Id 입력)");
            string value = GUILayout.TextField(mTutorialTid);
            if (mTutorialTid != value)
            {
                mTutorialTid = value;
            }

            if(uint.TryParse(mTutorialTid, out var tutorialTid))
			{
                if (GUILayout.Button($"[{mTutorialTid}] 튜토리얼 시작"))
                {
                    TutorialSystem.Instance.StartTutorial(tutorialTid);
                }
            }

            if(GUILayout.Button($"튜토리얼 스킵"))
			{
                if(true == TutorialSystem.hasInstance)
                    TutorialSystem.Instance.TutorialSkip();
            }
        }
    }
}