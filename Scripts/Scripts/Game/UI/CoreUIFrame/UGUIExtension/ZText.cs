using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("ZUI/ZText", 10)]
public class ZText : Text
{
	[SerializeField]
	public bool Localizing = true;
	[SerializeField]
	public string LocalizingTID = null;
	[SerializeField]
	public int FontSetID = 0;

	public override string text { get { return base.text; } set { SetTextLocalizing(value); } }
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
		
		if (Application.isPlaying)
		{
			if (LocalizingTID != null && LocalizingTID.Length != 0 && CManagerUIPresetBase.Instance != null)
			{
				base.text = CManagerUIPresetBase.Instance.GetUIPresetLocalizingText(LocalizingTID);
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();

	}

	//-------------------------------------------------------------------
	private void SetTextLocalizing(string _Text)
    {		
		if(CManagerUIPresetBase.Instance != null && _Text != null && Localizing == true)
        {
			base.text = CManagerUIPresetBase.Instance.GetUIPresetLocalizingText(_Text);
        }
		else
        {
			base.text = _Text;
        }
	}



}
