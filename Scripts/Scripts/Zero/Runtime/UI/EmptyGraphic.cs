using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hit영역용
/// </summary>
/// <remarks>
/// 메시 생성은 막고, Raycast에서 Detection만 되도록 해서 인풋 막아지도록 하기.
/// </remarks>
[AddComponentMenu("UI/EmptyGraphic")]
public class EmptyGraphic : Graphic
{
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}

	//// 만약 충돌검출체크에서 까지 최적화를 하고 싶다면 아래 코드를 주석해제바람
	//// 주의 사항 : 해당 히트영역 부모에 버튼 or또는 다른 히트영역이 존재한다면 원하는 동작이 제대로 되는지 체크 필요
	//public override bool Raycast(Vector2 sp, Camera eventCamera)
	//{
	//	return isActiveAndEnabled;
	//}
}