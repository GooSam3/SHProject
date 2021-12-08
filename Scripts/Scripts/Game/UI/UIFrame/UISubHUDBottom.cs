using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UISubHUDBottom : ZUIFrameBase
{
    private const string SPRITE_TENDANCY_DEVIL = "icon_hud_disposition_devil";
    private const string SPRITE_TENDANCY_ANGLE = "icon_hud_disposition_devil";
    private const string SPRITE_TENDANCY_MIDDLE = "icon_hud_disposition_devil";

    #region UI Variable
    [SerializeField] private Image TendancyIcon;
    [SerializeField] private Text CurrentLevel;
    [SerializeField] private Text CurrentExp;
	[SerializeField] private Slider CurrentExpSlider;
	[SerializeField] private Text CurrentTime;
    [SerializeField] private Image CurrentBattery;
    [SerializeField] private Text CurrentTendencyText;
    [SerializeField] private GameObject TendencyObj;
	#endregion UI Variable

	#region System Variable
	#endregion SystemVariable

	#region CameraMenu
	private enum E_CameraTabType
	{
		None,
		Mode,
		Free,
		Direction,
	}

	[SerializeField] private ZImage cameraModeImg;
	[SerializeField] private ZImage cameraFreeImg;
	[SerializeField] private ZImage cameraDirImg;
	[SerializeField] private ZText cameraModeText;
	[SerializeField] private ZText cameraDirText;
	[SerializeField] private ZText cameraTitleText;

	private E_CameraTabType cameraTabType = E_CameraTabType.Free;
	private E_CameraMotorType modeCamMotorType = E_CameraMotorType.Quarter;
	private E_CameraMotorType dirCamMotorType = E_CameraMotorType.North;
	private Sequence tweenCamText;
	#endregion

	protected override void OnInitialize()
    {
        base.OnInitialize();

		OnUpdateLevel(ZNet.Data.Me.CurCharData.Level, ZNet.Data.Me.CurCharData.Level);
		OnUpdateExp(ZNet.Data.Me.CurCharData.Exp, ZNet.Data.Me.CurCharData.Exp, false);
		OnUpdateTendancy(ZNet.Data.Me.CurCharData.Tendency, ZNet.Data.Me.CurCharData.Tendency);
		CameraManager.Instance.DoAddEventChangeCameraMotor( RefreshCameraUI );
	}

	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();

        ZNet.Data.Me.CurCharData.LevelUpdated += OnUpdateLevel;
		ZNet.Data.Me.CurCharData.ExpUpdated += OnUpdateExp;
		ZNet.Data.Me.CurCharData.TendencyUpdated += OnUpdateTendancy;
	}

	protected override void OnUnityDestroy()
	{
		base.OnUnityDestroy();

        ZNet.Data.Me.CurCharData.LevelUpdated -= OnUpdateLevel;
		ZNet.Data.Me.CurCharData.ExpUpdated -= OnUpdateExp;
		ZNet.Data.Me.CurCharData.TendencyUpdated -= OnUpdateTendancy;

		if(CameraManager.hasInstance)
			CameraManager.Instance.DoRemoveEventChangeCameraMotor( RefreshCameraUI );
	}

	protected override void OnRemove()
    {
        base.OnRemove();
    }

    public void OnClickTendency()
    {
        TendencyObj.SetActive(true);
        TimeInvoker.Instance.RequestInvoke(delegate { TendencyObj.SetActive(false); },3.0f);
    }

    private void OnUpdateTendancy(int _preTendancy, int _newTendancy)
    {
        CurrentTendencyText.text = _newTendancy.ToString();

        if(DBPKBuff.GetTableByTendencyValue(_newTendancy, out var table))
		{
            TendancyIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.TendencyIconType.ToString());
        }
    }

    private void OnUpdateLevel(uint _preLevel, uint _newLevel)
    {
        CurrentLevel.text = _newLevel.ToString();
    }

    private void OnUpdateExp(ulong _preExp, ulong _newExp, bool _isMonsterKill)
    {
        ulong curlevelupExp = DBLevel.GetExp(DBCharacter.GetClassTypeByTid(ZNet.Data.Me.CurCharData.TID), ZNet.Data.Me.CurCharData.Level);

        float expRate = DBLevel.GetExpRate(_newExp - curlevelupExp, ZNet.Data.Me.CurCharData.Level, ZNet.Data.Me.CurCharData.TID);

		/// 201102(이윤선 수정) : 레벨업이 됐을때 1 이상의 값이 들어오므로 Clamp 처리 추가 .
		if (expRate >= 1)
			expRate -= 1;

		CurrentExp.text = $"({expRate.ToString("P2")})";
		CurrentExpSlider.value = expRate;
	}

    private void OnUpdateTime(long _time)
    {
        System.DateTime dateTime = TimeHelper.ParseTimeStamp(_time);

        CurrentTime.text = $"{dateTime.Hour}:{dateTime.Minute}";
    }

    private void OnUpdateBattery(float _battery)
    {
		if (SystemInfo.batteryLevel < 0)
		{
			// 배터리 상태 미지원
		}
		else
		{
			// 0 ~ 1 사이값.
			CurrentBattery.fillAmount = SystemInfo.batteryLevel;
		}
    }

	#region CameraMenu
	// 모드캠 버튼클릭
	public void OnChangeModeCam()
	{
		if( cameraTabType == E_CameraTabType.Mode ) {
			E_CameraMotorType type = CameraManager.Instance.CurrentMotorType;
			switch( type ) {
				case E_CameraMotorType.Quarter: type = E_CameraMotorType.Top; break;
				case E_CameraMotorType.Top: type = E_CameraMotorType.Shoulder; break;
				case E_CameraMotorType.Shoulder: type = E_CameraMotorType.Quarter; break;
				default: ZLog.LogError( ZLogChannel.Default, $"예상하지 않은 캠타입이 들어옴, {type}" ); break;
			}

			modeCamMotorType = type;
			CameraManager.Instance.DoChangeCameraMotor( type );
		}
		else {
			CameraManager.Instance.DoChangeCameraMotor( modeCamMotorType );
		}

		CameraManager.Instance.SaveCameraMotorType();
	}

	// 프리캠 버튼클릭
	public void OnChangeFreeCam()
	{
		CameraManager.Instance.DoChangeCameraMotor( E_CameraMotorType.Free );

		// 이미 프리카메라 일경우는 fix / free 스왑을 한다
		if( cameraTabType == E_CameraTabType.Free ) {
			var motorFree = CameraManager.Instance.CurrentMotor as CameraMotorFree;
			motorFree.SetFixedCamera( !motorFree.IsFixed, true );

			// 위 DoChangeCameraMotor() 로 인해 이미 ui가 갱신되었기 때문에  스왑을 반영한 갱신을 한번더 해야한다.
			RefreshCameraUI( E_CameraMotorType.Free );
		}

		CameraManager.Instance.SaveCameraMotorType();
	}

	// 방향캠 버튼클릭
	public void OnChangeDirectionCam()
	{
		if( cameraTabType == E_CameraTabType.Direction ) {
			E_CameraMotorType type = CameraManager.Instance.CurrentMotorType;
			switch( type ) {
				case E_CameraMotorType.North: type = E_CameraMotorType.East; break;
				case E_CameraMotorType.East: type = E_CameraMotorType.South; break;
				case E_CameraMotorType.South: type = E_CameraMotorType.West; break;
				case E_CameraMotorType.West: type = E_CameraMotorType.North; break;
				default: ZLog.LogError( ZLogChannel.Default, $"예상치 않은 캠타입이 들어옴, {type}" ); break;
			}

			dirCamMotorType = type;
			CameraManager.Instance.DoChangeCameraMotor( type );
		}
		else {
			CameraManager.Instance.DoChangeCameraMotor( dirCamMotorType );
		}

		CameraManager.Instance.SaveCameraMotorType();
	}

	private void RefreshCameraUI( E_CameraMotorType motorType )
	{
		E_CameraMotorType type = CameraManager.Instance.CurrentMotorType;
		cameraTabType = GetCameraTabType( type );

		if( cameraTabType == E_CameraTabType.None ) {
			return;
		}

		// 현재 카메라 텍스트 애니
		if( CameraManager.Instance.CurrentMotorType == E_CameraMotorType.Free ) {
			var motorFree = CameraManager.Instance.CurrentMotor as CameraMotorFree;
			cameraTitleText.text = string.Format( "{0} Cam", motorFree.IsFixed ? "Fix" : "Free" );
		}
		else {
			cameraTitleText.text = string.Format( "{0} Cam", CameraManager.Instance.CurrentMotorType );
		}

		tweenCamText?.Kill( true );
		tweenCamText = DOTween.Sequence().
			   Join( DOTween.ToAlpha( () => cameraTitleText.color, x => cameraTitleText.color = x, 1, 0 ) ).
			   AppendInterval( 1 ).
			   Append( DOTween.ToAlpha( () => cameraTitleText.color, x => cameraTitleText.color = x, 0, 1 ) ).
			   Play();

		const float TO = 0.15f;
		const float DURATION = 0.2f;

		// 버튼 이미지 활성
		switch( cameraTabType ) {
			case E_CameraTabType.Mode: {
				DOTween.ToAlpha( () => cameraModeImg.color, x => cameraModeImg.color = x, 1, DURATION );
				DOTween.ToAlpha( () => cameraFreeImg.color, x => cameraFreeImg.color = x, TO, DURATION );
				DOTween.ToAlpha( () => cameraDirImg.color, x => cameraDirImg.color = x, TO, DURATION );
				DOTween.ToAlpha( () => cameraModeText.color, x => cameraModeText.color = x, 1, DURATION );
				DOTween.ToAlpha( () => cameraDirText.color, x => cameraDirText.color = x, TO, DURATION );
				break;
			}
			case E_CameraTabType.Free: {
				DOTween.ToAlpha( () => cameraModeImg.color, x => cameraModeImg.color = x, TO, DURATION );
				DOTween.ToAlpha( () => cameraFreeImg.color, x => cameraFreeImg.color = x, 1, DURATION );
				DOTween.ToAlpha( () => cameraDirImg.color, x => cameraDirImg.color = x, TO, DURATION );
				DOTween.ToAlpha( () => cameraModeText.color, x => cameraModeText.color = x, TO, DURATION );
				DOTween.ToAlpha( () => cameraDirText.color, x => cameraDirText.color = x, TO, DURATION );
				break;
			}
			case E_CameraTabType.Direction: {
				DOTween.ToAlpha( () => cameraModeImg.color, x => cameraModeImg.color = x, TO, DURATION );
				DOTween.ToAlpha( () => cameraFreeImg.color, x => cameraFreeImg.color = x, TO, DURATION );
				DOTween.ToAlpha( () => cameraDirImg.color, x => cameraDirImg.color = x, 1, DURATION );
				DOTween.ToAlpha( () => cameraModeText.color, x => cameraModeText.color = x, TO, DURATION );
				DOTween.ToAlpha( () => cameraDirText.color, x => cameraDirText.color = x, 1, DURATION );
				break;
			}
		}

		// 카메라 아이콘 텍스트
		switch( CameraManager.Instance.CurrentMotorType ) {
			// 프리캠
			case E_CameraMotorType.Free: {
				var motorFree = CameraManager.Instance.CurrentMotor as CameraMotorFree;
				if( motorFree.IsFixed ) {
					cameraFreeImg.sprite = ZManagerUIPreset.Instance.GetSprite( "icon_hud_freecam_lock" );
				}
				else {
					cameraFreeImg.sprite = ZManagerUIPreset.Instance.GetSprite( "icon_hud_freecam_open" );
				}
				break;
			}
			// 모드카메라
			case E_CameraMotorType.Quarter: cameraModeText.text = "Q"; break;
			case E_CameraMotorType.Top: cameraModeText.text = "T"; break;
			case E_CameraMotorType.Shoulder: cameraModeText.text = "S"; break;
			// 방향카메라
			case E_CameraMotorType.North: cameraDirText.text = "N"; break;
			case E_CameraMotorType.East: cameraDirText.text = "E"; break;
			case E_CameraMotorType.South: cameraDirText.text = "S"; break;
			case E_CameraMotorType.West: cameraDirText.text = "W"; break;
		}
	}

	private E_CameraTabType GetCameraTabType( E_CameraMotorType motorType )
	{
		switch( motorType ) {
			case E_CameraMotorType.Quarter:
			case E_CameraMotorType.Top:
			case E_CameraMotorType.Shoulder: return E_CameraTabType.Mode;

			case E_CameraMotorType.Free: return E_CameraTabType.Free;

			case E_CameraMotorType.North:
			case E_CameraMotorType.East:
			case E_CameraMotorType.South:
			case E_CameraMotorType.West: return E_CameraTabType.Direction;
		}
		return E_CameraTabType.None;
	}
	#endregion
}