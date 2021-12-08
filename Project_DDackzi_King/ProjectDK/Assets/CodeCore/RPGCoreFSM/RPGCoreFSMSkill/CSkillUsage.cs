using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ��ų �ߵ��� �־����� �����ͷ� ��� ������ ��� �ִ�.

abstract public class CSkillUsage
{
	public uint			UsageSkillID		= 0;
	public Vector3		UsageOrigin		= Vector3.zero;		// ������ ��ġ
	public Vector3		UsagePosition		= Vector3.zero;		// ���� ��ġ ��κ� Ŭ������Ʈ�� �� ���̴�.
	public Vector3		UsageDirection	= Vector3.zero;		// ���� ����
	public ISkillProcessor	UsageTarget		= null;				// ���� ���
	public List<object>	UsageDynamicParam = new List<object>(); // �� �׽�ũ�� ������ �߰� ������
}
