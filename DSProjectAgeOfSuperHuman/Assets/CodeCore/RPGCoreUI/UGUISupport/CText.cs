using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UICustom/CText", 10)]
[System.Serializable]
public class CText : Text
{
	public delegate string TextReference(string strText);  // 로컬라이징 테이블이나 차트같이 다양한 참조 변환을 위한 브릿지

	[SerializeField]
	public int	  FontSetID = 0;
	[SerializeField]
	public string LocalizeingKey;

	private TextReference m_delTextRef = null;
	//----------------------------------------------------------------
	public override string text 
	{
		get { return base.text; } 
		set {  
			if (m_delTextRef != null)
			{
				base.text = m_delTextRef(value);
			}
			else
			{
				base.text = value;
			}
		}
	}
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

	public void SetTextReference(TextReference pTextRef)
	{
		m_delTextRef = pTextRef;
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
