using UnityEngine;
using GameDB;
using UnityEngine.UI;
using System;

/// <summary> 튜토리얼 UI Dialogue </summary>
public class UIFrameTutorialDialogue : MonoBehaviour
{
	[SerializeField]
	private Text TextName;

	[SerializeField]
	private Text TextDesc;

	[SerializeField]
	private ZRawImage NpcImage;

	private Action mEventFinish;

	private float mStartTime;

	public void Set(string name, string desc, string resourceName, Action onFinish)
	{
		TextDesc.text = DBLocale.GetText(desc);
		TextName.text = DBLocale.GetText(name);

		mEventFinish = onFinish;

		mStartTime = Time.time;
		// TODO :: NPC image 셋팅 셋팅된 이미지가 다르다면 연출할까
		NpcImage.LoadTexture(resourceName);

		// TODO :: Dialogue 연출

	}

	/// <summary> 다이얼로그 클릭! </summary>
	public void OnClickDialogue()
	{
		//1초는 봐야지
		if(mStartTime + 1f > Time.time)
			return;

		mEventFinish?.Invoke();
	}
}