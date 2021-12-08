using GameDB;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using uTools;

public class UIRide : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private GameObject RideImage = null;
    [SerializeField] private GameObject UnRideImage = null;
    [SerializeField] private GameObject DisabledImage = null;
    //[SerializeField] private GameObject AutoActiveImage = null;
    [SerializeField] private uTweenSlider CoolTime = null;
    [SerializeField] private Text RemainTimeText = null;
    #endregion

    private ulong RideTime;

    private const ulong RIDE_DELAY_TIME = 1;

    protected void OnEnable()
    {
        float remainCoolTime = ZNet.Data.Me.CurCharData.VehicleEndCoolTime - TimeManager.NowSec;

        if(remainCoolTime > 0)
        {
            UpdateCoolTime();
        }
    }

    protected void OnDisable()
    {
        CoolTimeEnd();
        StopCoroutine("RemainCoolTimeText");
    }

    public void DoAddEvent()
    {
        ZPawnManager.Instance.DoAddEventUpdateRideVehicle(UpdateRideVehicle);
        ZPawnManager.Instance.DoAddEventSceneLoaded(ChangeScene);
    }

    public void DoRemoveEvent()
    {
        ZPawnManager.Instance.DoRemoveUpdateRideVehicle(UpdateRideVehicle);
        ZPawnManager.Instance.DoRemoveEventSceneLoaded(ChangeScene);
        ZPawnManager.Instance.DoRemoveEventCreateMyEntity(UpdateRideButton);
    }

    public void OnClickRiding()
    {
        if(false == ZPawnManager.Instance.MyEntity.IsPossibleRide)
        {
            //탑승 불가 상태, 유적에서 바닥에 붙어있는 경우에만 호출하게 하기 위해 추가.
            return;
        }

        uint vehicleTid = ZNet.Data.Me.CurCharData.MainVehicle;
        ulong endCoolTime = ZNet.Data.Me.CurCharData.VehicleEndCoolTime;

        if (null != ZGameModeManager.Instance.Table)
        {
            if (ZGameModeManager.Instance.Table.RidingType != E_RidingType.Riding)
            {
                ZLog.Log(ZLogChannel.UI, $"Not Ride in Stage {ZGameModeManager.Instance.Table.RidingType}");
                return;
            }
        }

        if (0 >= vehicleTid)
        {
            UICommon.SetNoticeMessage(DBLocale.GetText("Not_Enter_Vehicle_Text"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        if (endCoolTime > TimeManager.NowSec)
        {
            UICommon.SetNoticeMessage(string.Format(DBLocale.GetText("Vehicle_Cooltime_Message"), endCoolTime - TimeManager.NowSec), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        if (true == ZPawnManager.Instance.MyEntity.IsRiding)
        {
            //탑승 해제 요청시 탑승후 RIDE_DELAY_TIME이 지난뒤 다시 탑승 해제 요청을 할 수 있다. (더블 클릭 방지)
            if (RideTime + RIDE_DELAY_TIME > TimeManager.NowSec)
                return;
            
            vehicleTid = 0;
        }

        if (ZPawnManager.Instance.MyEntity.IsSkillAction)
        {
            //전투 중에 탑승할 수 없습니다.
            UICommon.SetNoticeMessage(DBLocale.GetText("Vehicle_Ride_Error_Battle"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }
        else if (ZPawnManager.Instance.MyEntity.IsMezState(E_ConditionControl.NotRide))
        {
            //Vehicle_Ride_Error
            UICommon.SetNoticeMessage(DBLocale.GetText("Vehicle_Ride_Error"), Color.red, 1f, UIMessageNoticeEnum.E_MessageType.BackNotice);
            return;
        }

        //탑승 시간 저장
        if (0 < vehicleTid)
        {
            RideTime = TimeManager.NowSec;
        }

        ZMmoManager.Instance.Field.REQ_RideVehicle(ZPawnManager.Instance.MyEntityId, vehicleTid);
    }

    private void UpdateRideVehicle(bool _isRide)
    {
        if (this.gameObject.activeSelf)
        {
            RideImage.SetActive(_isRide);
            UnRideImage.SetActive(!_isRide);

            if (!_isRide)
            {
                UpdateCoolTime();
            }
        }
    }

    private void UpdateCoolTime()
    {
        if (ZNet.Data.Me.CurCharData.VehicleEndCoolTime <= TimeManager.NowSec)
            return;

        float remainTime = ZNet.Data.Me.CurCharData.VehicleEndCoolTime - TimeManager.NowSec;

        CoolTime.gameObject.SetActive(true);
        CoolTime.enabled = true;
        CoolTime.duration = remainTime;
        CoolTime.ResetToBeginning();
        CoolTime.Play(true);

        if(true == gameObject.activeInHierarchy && 
           true == gameObject.activeSelf)
        {
            StartCoroutine(RemainCoolTimeText(remainTime));
        }
    }

    IEnumerator RemainCoolTimeText(float _remainTime)
    {
        while(_remainTime > 0)
        {
            RemainTimeText.text = _remainTime.ToString();

            yield return new WaitForSeconds(1.0f);

            --_remainTime;
        }
    }

    public void CoolTimeEnd()
    {
        CoolTime.gameObject.SetActive(false);
    }

    private void ChangeScene()
    {
        ZPawnManager.Instance.DoAddEventCreateMyEntity(UpdateRideButton);
    }

    private void UpdateRideButton()
    {
        if (DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var stageTable))
        {
            if (stageTable.RidingType == E_RidingType.NotRiding)
            {
                RideImage.SetActive(false);
                UnRideImage.SetActive(true);
                //AutoActiveImage.SetActive(false);
                CoolTime.gameObject.SetActive(false);
                DisabledImage.SetActive(true);
            }
            else
            {
                if (ZPawnManager.Instance.MyEntity.IsRiding)
                {
                    RideImage.SetActive(true);
                    UnRideImage.SetActive(false);
                    //AutoActiveImage.SetActive(false);
                    DisabledImage.SetActive(false);
                    CoolTime.gameObject.SetActive(false);
                }
                else
                {
                    RideImage.SetActive(false);
                    UnRideImage.SetActive(true);
                    //AutoActiveImage.SetActive(false);
                    DisabledImage.SetActive(false);
                    CoolTime.gameObject.SetActive(false);

                    UpdateCoolTime();
                }
            }
        }
    }
}
