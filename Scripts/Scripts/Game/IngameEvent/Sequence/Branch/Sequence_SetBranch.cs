using UnityEngine;
using System.Collections;

namespace IngameEvent
{
	/// <summary> 분기 저장 </summary>
	public class Sequence_SetBranch : IngameEventSequenceBase
	{
		[Header("분기 저장 키 (로컬)")]
		[SerializeField]
		private string BranchKey = string.Empty;

		[Header("해당 키에 대한 값!")]
		[SerializeField]
		private string BranchValue = string.Empty;

		protected override void BeginEventImpl()
		{
			//저장할 키와 값이 있다면 저장!
			if(false == string.IsNullOrEmpty(BranchKey) && false == string.IsNullOrEmpty(BranchValue))
			{
				DeviceSaveDatas.SaveCurCharData(BranchKey, BranchValue);
			}

			EndEvent();
		}

		protected override void EndEventImpl()
		{
		}
	}
}
