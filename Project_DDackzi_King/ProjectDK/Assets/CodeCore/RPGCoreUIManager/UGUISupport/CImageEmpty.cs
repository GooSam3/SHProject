using UnityEngine;
using UnityEngine.UI;

// 향후 ZImageNull 로 이름 변경 예정 
// 이미지와 같은 쓰임세를 가지나 드로우 자체를 하지 않는다.
// 주로 인비지블 버튼과 같이 영역을 가지고 포인트 체크를 하나 이미지가 필요하지 않을때 사용 

public class CImageEmpty : Graphic
{
    //-----------------------------------------------------
	protected override void OnPopulateMesh(VertexHelper vh)
	{
        vh.Clear();
	}

}
