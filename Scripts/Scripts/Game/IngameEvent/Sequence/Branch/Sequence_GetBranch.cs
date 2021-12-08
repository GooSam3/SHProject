using UnityEngine;
using System.Collections.Generic;

namespace IngameEvent
{
	/// <summary> 분기 저장 값 가지고 와서 Event Group 셋팅 </summary>
	public class Sequence_GetBranch : IngameEventSequenceBase
	{
		[Header("분기 저장 찾을 키 (로컬)")]
		[SerializeField]
		private string BranchKey = string.Empty;

		[Header("해당 키에 대한 비교 값 및 변경할 Event Group")]
		[SerializeField]
		private List<IngameEventBranchValue> BranchValues;

		protected override void BeginEventImpl()
		{
			//저장할 키와 값이 있다면 저장!
			if(false == string.IsNullOrEmpty(BranchKey))
			{
				string value = DeviceSaveDatas.LoadCurCharData(BranchKey, "");

				foreach (var branchValue in BranchValues)
				{
					if (true == string.IsNullOrEmpty(branchValue.Value))
						continue;

					if (false == branchValue.Value.Equals(value))
						continue;

					//그룹 변경!
					EventPlayer.ChangeEventGroup(branchValue.SelectEventGroup);
					break;
				}
				
			}

			EndEvent();
		}

		protected override void EndEventImpl()
		{
		}
	}
}
