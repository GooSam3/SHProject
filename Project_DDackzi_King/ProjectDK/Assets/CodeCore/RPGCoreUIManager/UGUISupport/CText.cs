using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UICustom/CText", 10)]
public class CText : Text
{
	[SerializeField]
	public string LocalizingTID = string.Empty;
	[SerializeField]
	public int FontSetID = 0;

//	public override string text { get { return base.text; } set {  } }
	//---------------------------------------------------------------
	public void SetFontData(FontData _FontData)
	{
		font = _FontData.font;
		fontSize = _FontData.fontSize;
		fontStyle = _FontData.fontStyle;
		lineSpacing = _FontData.lineSpacing;
		supportRichText = _FontData.richText;
		alignment = _FontData.alignment;
		horizontalOverflow = _FontData.horizontalOverflow;
		verticalOverflow = _FontData.verticalOverflow;
		resizeTextForBestFit = _FontData.bestFit;
		alignByGeometry = _FontData.alignByGeometry;
	}

	protected override void Awake()
	{
		base.Awake();	
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	//-------------------------------------------------------------------



}
