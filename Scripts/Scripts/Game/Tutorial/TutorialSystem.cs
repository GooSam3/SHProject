using UnityEngine;
using Zero;

/// <summary> 튜토리얼 시스템 </summary>
public class TutorialSystem : Singleton<TutorialSystem>
{
	public static bool IsPlaying { get { return false != hasInstance && null != Instance.Tutorial /*&& Instance.Tutorial.IsPlaying*/; } }

	public TutorialPlayer Tutorial { get; private set; }

	public void StartTutorial(uint questTid)
	{
		if (IsPlaying)
		{
			ZLog.LogError(ZLogChannel.Quest, $"[{Tutorial.QuestTid}]이미 튜토리얼 진행중이다.");
			return;
		}
		if (null == Tutorial)
		{
			var go = new GameObject($"TutorialPlayer_{questTid}", typeof(TutorialPlayer));
			go.transform.parent = transform;
			Tutorial = go.GetComponent<TutorialPlayer>();
		}

		Tutorial.StartTutorial(questTid);
	}

	/// <summary> 튜토리얼 완료시 호출 </summary>
	public void ClearTutotial(uint questTid)
	{
		if (null == Tutorial)
			return;

		if (Tutorial.QuestTid != questTid)
			return;

		if (null != Tutorial)
			GameObject.Destroy(Tutorial.gameObject);

		Tutorial = null;

	}
	public void TutorialSkip()
	{
		if (null == Tutorial)
			return;

		Tutorial.TutorialSkip();
	}

	public void DestroyTutorial()
	{
		GameObject.Destroy(this.gameObject);
	}
}