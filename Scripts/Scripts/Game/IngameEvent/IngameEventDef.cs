using System;
using UnityEngine;

namespace IngameEvent
{

	/// <summary> 인게임 분기 처리용 이벤트 그룹 </summary>
	[Flags]
	public enum E_IngameEventGroup
	{
		None = 0,
		Group_01 = 1 << 0,
		Group_02 = 1 << 1,
		Group_03 = 1 << 2,
		Group_04 = 1 << 3,
		Group_05 = 1 << 4,
		Group_06 = 1 << 5,
		Group_07 = 1 << 6,
		Group_08 = 1 << 7,
		Group_09 = 1 << 8,
		Group_10 = 1 << 9,
	}

	public enum E_ActorActionMoveType
	{
		OneShot,
		Loop,
		PingPong,
	}


	[Serializable]
	public class IngameEventDialogChoice
	{
		[Header("선택지 텍스트 id")]
		public string LocaleId;

		[Header("선택 후 분기 처리될 그룹")]
		public E_IngameEventGroup SelectEventGroup;
	}

	[Serializable]
	public class IngameEventBranchValue
	{
		[Header("분기 저장된 키에 대한 값")]
		public string Value;

		[Header("분기 저장된 키의 값과 같다면 변경할 Event Group")]
		public E_IngameEventGroup SelectEventGroup;
	}

	[Serializable]
	public class IngameEventAnimation
	{
		[Header("애니메이션 키")]
		public string AnimationKey;

		[Header("실행할 애니메이션 스테이트")]
		public E_AnimStateName AnimState;

		[Header("실행할 파라미터")]
		public E_AnimParameter AnimParameter;

		[Header("애니메이션 클립")]
		public AnimationClip Clip;
	}
}
