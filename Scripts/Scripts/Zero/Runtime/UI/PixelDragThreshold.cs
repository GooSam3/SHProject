using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 해상도에 따른 터치 감도 균일화를 위한 컴포넌트
/// </summary>
public class PixelDragThreshold : MonoBehaviour
{
	/// <summary></summary>
	private const float InchToCm = 2.54f;

	[SerializeField]
	private EventSystem eventSystem = null;

	/// <summary>드래그 시작 임계값 최소</summary>
	private float dragThresholdCM = 0.4f;

	void Start()
	{
		if (null == eventSystem)
		{
			eventSystem = GetComponent<EventSystem>();
		}

		SetDragThreshold();
	}

	private void SetDragThreshold()
	{
		if (null != eventSystem)
		{
			eventSystem.pixelDragThreshold = (int)(dragThresholdCM * Screen.dpi / InchToCm);

			ZLog.Log(ZLogChannel.System, $"PixelDragThreshold({eventSystem.pixelDragThreshold}) = dragThresholdCM: {dragThresholdCM} * DPI: {Screen.dpi} / InchToCm: {InchToCm}");
		}
	}
}