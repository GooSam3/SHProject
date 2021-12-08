using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using Zero;

public abstract class UIEventContentBase : MonoBehaviour
{
	[SerializeField] protected RawImage imgBG;

	private bool isLoadSuccess = false;
	private Action onLoadSuccess;

	protected bool IsLoadSuccess
	{
		get { return isLoadSuccess; }
		
		set
		{
			isLoadSuccess = value;
			if (isLoadSuccess) 
				OnSuccess();
		}
	}

	public void SetContent(IngameEventInfoConvert _eventData, Action _onLoadSuccess)
	{
		isLoadSuccess = false;
		  //tests 수집이벤트 작업중
		//if (_eventData == null)
		//{
		//	ZLog.Log(ZLogChannel.Event, $"이벤트 데이터 없음 this : {this.name}");
		//	return;
		//}

		onLoadSuccess = _onLoadSuccess;
		if (_eventData != null)
		{
			if (string.IsNullOrEmpty(_eventData.bgUrl))
			{
				ZLog.Log(ZLogChannel.Event, $"BgUrl is Empty!! this type : {_eventData.SubCategory}");
				imgBG.texture = null;	
			}
			else
			{
				ZResourceManager.Instance.GetTexture2DFromUrl(_eventData.bgUrl,
									_eventData.bgHash,
									(tex) => imgBG.texture = tex, $"{nameof(E_ServerEventCategory)}_{_eventData.Category}");
			}
		}

		IsLoadSuccess = SetContent(_eventData);
	}

	/// <summary>
	/// 컨텐츠 설정함
	/// </summary>
	/// <param name="_eventData"></param>
	/// <returns>완료 여부</returns>
	protected abstract bool SetContent(IngameEventInfoConvert _eventData);

	protected abstract void ReleaseContent();

	private void OnSuccess()
	{
		Open();
		onLoadSuccess?.Invoke();
	}

	// 왠만하면 이벤트 등록 | 해제만
	public virtual void Open()
	{
		this.gameObject.SetActive(true);
	}

	// 왠만하면 이벤트 등록 | 해제만
	public virtual void Close()
	{
		if (this.gameObject.activeSelf)
			this.gameObject.SetActive(false);
	}

	public void Release()
	{
		ReleaseContent();
		Close();
		ZPoolManager.Instance.Return(this.gameObject);
	}
}
