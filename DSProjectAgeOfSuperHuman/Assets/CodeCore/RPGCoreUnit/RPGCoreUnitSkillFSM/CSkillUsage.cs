using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스킬 발동시 주어지는 데이터로 사용 정보를 담고 있다.

abstract public class CSkillUsage
{
	public uint			UsageSkillID		= 0;
	public Vector3		UsageOrigin		= Vector3.zero;		// 시전자 위치
	public Vector3		UsagePosition		= Vector3.zero;		// 시전 위치 대부분 클릭포인트가 될 것이다.
	public Vector3		UsageDirection	= Vector3.zero;		// 시전 방향
	public ISkillProcessor	UsageTarget		= null;				// 시전 대상
	public List<object>	UsageDynamicParam = new List<object>(); // 각 테스크별 별도의 추가 데이터
}
