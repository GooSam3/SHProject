// 주의!! 디자이너 편집용 프레임으로 인게임에 삽입하지 말것 !
public class UIFrameDesignerOnly : ZUIFrameBase
{

	protected override void OnHide()
	{
		base.OnHide();
		ImportShow();
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
	}
}
