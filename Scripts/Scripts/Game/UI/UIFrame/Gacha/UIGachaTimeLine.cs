using System;
using UnityEngine;
using UnityEngine.Playables;

public class UIGachaTimeLine : UIFrameGacha
{
	#region Variable
	private UIFrameGacha Frame = null;

	private int SystemKey = -1;
	#endregion

	protected override void Initialize(ZUIFrameBase _frame)
	{
		base.Initialize(_frame);

		Frame = _frame as UIFrameGacha;
	}

	protected override void OnHide()
	{
		base.OnHide();

		ClearAllTimeLine();
	}

	public void PlayTimeLine(UIGachaEnum.E_TimeLineType _type)
	{
		ClearAllTimeLine();

		ZResourceManager.Instance.Load(UIGachaData.GetTimeLineName(Frame.CurrentGachaStyle, _type), (string _resName, GameObject _timeLine) =>
		{
			if (_timeLine == null)
			{
				UICommon.OpenSystemPopup_One(ZUIString.ERROR,
						"타임라인 파일 로드 실패.", ZUIString.LOCALE_OK_BUTTON);
				return;
			}

			// 예외처리 필요하면 추가
			switch(Frame.CurrentGachaStyle)
			{

			}
			UIManager.Instance.ChangeMainCameraStack(CManagerUIFrameFocusBase.E_UICameraStack.Gacha);
			UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Front, true);
			UIManager.Instance.ActiveUICamera(CManagerUIFrameFocusBase.E_UICamera.Model3D, true);

			GameObject parentObj = Frame.StateObj[Convert.ToInt32(UIGachaEnum.E_StateObjectType.TimeLine)].gameObject;
			GameObject timeLineObj = Instantiate(_timeLine);
			Frame.SetTimeLine(timeLineObj.GetComponent<PlayableDirector>());
			Frame.SetCardLinker(timeLineObj.GetComponent<UIGachaCardLinker>());
			Frame.CardLinker.Initialize(Frame.CurrentGachaStyle, Frame.ListGachaResult, delegate { Frame.TurnAllCardBtn.gameObject.SetActive(true); });
			SystemKey = UIManager.Instance.SetSystemObject(Frame, timeLineObj, parentObj );
			//timeLineObj.gameObject.transform.SetParent(parentObj.transform, false);

			if (Frame.GachaSceneController != null)
				Frame.GachaSceneController.gameObject.SetActive(false);

			parentObj.SetActive(true);
			Frame.TimeLine.Play();
		});
	}

	public void ClearAllTimeLine()
	{
		if (Frame.TimeLine != null)
		{
		
			if(SystemKey>0)
			{
				if(UIManager.Instance.GetOutSystemObject(Frame, SystemKey, Frame.StateObj[Convert.ToInt32(UIGachaEnum.E_StateObjectType.TimeLine)].gameObject))
					SystemKey =  -1;
			}
			Destroy(Frame.TimeLine.gameObject);
			Frame.SetCardLinker(null);
			Frame.SetTimeLine(null);
		}

		Frame.StateObj[Convert.ToInt32(UIGachaEnum.E_StateObjectType.TimeLine)].SetActive(false);
	}
}