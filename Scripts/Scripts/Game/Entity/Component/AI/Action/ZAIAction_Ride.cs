using GameDB;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    public class ZAIAction_Ride : ZAIActionBase
    {
		public float DelayTime = 1f;
		private float LastRequestTime = 0f;
		protected override string info
		{
			get { return "탑승 요청"; }
		}
		protected override void OnExecute()
        {
			if (!(agent is ZPawnMyPc myEntity))
			{
				EndAction(false);
				return;
			}
			//TODO :: 일단 예외처리 (자동 전투시 사거리 밖이라는 에러 날라옴. mmo에서 확인 필요할듯)
			EndAction(true);
			return;

			if (false == myEntity.IsRiding)				
            {
				if (LastRequestTime + DelayTime < TimeManager.NowSec)
				{
					uint vehicleTid = ZNet.Data.Me.CurCharData.MainVehicle;
					ulong endCoolTime = ZNet.Data.Me.CurCharData.VehicleEndCoolTime;
					if (0 < vehicleTid && endCoolTime < TimeManager.NowSec)
					{
						//탑승 요청
						ZMmoManager.Instance.Field.REQ_RideVehicle(myEntity.EntityId, vehicleTid);
						LastRequestTime = Time.time;
					}
				}
			}

			EndAction(true);
		}
	}
}
