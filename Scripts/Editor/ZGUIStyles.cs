using UnityEngine;
using UnityEditor;

public static class ZGUIStyles
{
	[InitializeOnLoadMethod()]
	static void Init()
	{
	}

    /// <summary>
    /// Label with small text
    /// </summary>
    private static GUIStyle mTitle = null;
    public static GUIStyle TitleLabel
	{
		get
		{
			if (mTitle == null)
			{
				Font lFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
				if (lFont == null) { lFont = EditorStyles.standardFont; }

				mTitle = new GUIStyle(GUI.skin.label);
				mTitle.font = lFont;
				mTitle.fontSize = 13;
				mTitle.fontStyle = FontStyle.Bold;
				mTitle.normal.textColor = Color.white;
				mTitle.fixedHeight = 22f;
				mTitle.padding = new RectOffset(0, 0, 0, 0);
			}

			return mTitle;
		}
	}

	private static GUIStyle mRichButton;
	public static GUIStyle RichButton
	{
		get
		{
			if (null == mRichButton)
			{
				mRichButton = new GUIStyle(GUI.skin.button);
				mRichButton.richText = true;
			}
			return mRichButton;
		}
	}

	private static GUIStyle mMapButtonStyle;
    public static GUIStyle MapButton
    {
        get
        {
            if (null == mMapButtonStyle)
            {
                mMapButtonStyle = new GUIStyle(GUI.skin.button);
                mMapButtonStyle.fontSize = 14;
                mMapButtonStyle.fontStyle = FontStyle.Bold;
            }
            return mMapButtonStyle;
        }
    }

	private static GUIStyle mBlueLabelButton;
	public static GUIStyle BlueLabelButton
	{
		get
		{
			if (null == mBlueLabelButton)
			{
				mBlueLabelButton = new GUIStyle(GUI.skin.button);
				mBlueLabelButton.fontSize = 13;
				mBlueLabelButton.fontStyle = FontStyle.Bold;
				mBlueLabelButton.normal.textColor = Color.blue;
				mBlueLabelButton.hover.textColor = Color.blue;
			}
			return mBlueLabelButton;
		}
	}

	private static GUIStyle mToolbarButtonStyle;
    public static GUIStyle ToolbarButton
    {
        get
        {
            if (null == mToolbarButtonStyle)
            {
                mToolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
                mToolbarButtonStyle.fontSize = 13;
                mToolbarButtonStyle.fontStyle = FontStyle.Bold;
                mToolbarButtonStyle.alignment = TextAnchor.MiddleCenter;
                //mToolbarButtonStyle.fixedHeight = 40f;
            }
            return mToolbarButtonStyle;
        }
    }

    private static GUIStyle mMiniLabel;
    public static GUIStyle MiniLabel
    {
        get
        {
            if (null == mMiniLabel)
            {
                mMiniLabel = new GUIStyle(EditorStyles.miniLabel);
                mMiniLabel.richText = true;
				mMiniLabel.fontSize = 11;

			}
            return mMiniLabel;
        }
    }

    private static GUIStyle mRichText;
    public static GUIStyle RichText
    {
        get
        {
            if (null == mRichText)
            {
                mRichText = new GUIStyle(EditorStyles.textArea);
                mRichText.richText = true;
				mRichText.alignment = TextAnchor.MiddleCenter;

			}
            return mRichText;
        }
    }

	private static GUIStyle mBlueLabel;
	public static GUIStyle BlueLabel
	{
		get
		{
			if (null == mBlueLabel)
			{
				mBlueLabel = new GUIStyle(EditorStyles.label);
				mBlueLabel.normal.textColor = Color.blue;
			}
			return mBlueLabel;
		}
	}

	private static GUIStyle mBlueBoldLabel;
	public static GUIStyle BlueBoldLabel
	{
		get
		{
			if (null == mBlueBoldLabel)
			{
				mBlueBoldLabel = new GUIStyle(EditorStyles.boldLabel);
				mBlueBoldLabel.normal.textColor = Color.blue;
			}
			return mBlueBoldLabel;
		}
	}

	private static GUIStyle mWarningLabel;
	public static GUIStyle WarningLabel
	{
		get
		{
			if (null == mWarningLabel)
			{
				mWarningLabel = new GUIStyle(EditorStyles.whiteLargeLabel);
				mWarningLabel.normal.textColor = Color.red;
			}
			return mWarningLabel;
		}
	}

	private static GUIStyle mBox1;
	public static GUIStyle BoxWhite
	{
		get
		{
			if (null == mBox1)
			{
				mBox1 = new GUIStyle(GUI.skin.box);
				mBox1.fontStyle = FontStyle.Bold;
				mBox1.normal.textColor = Color.red;
				mBox1.normal.background = Texture2D.whiteTexture;
				mBox1.onNormal.textColor = Color.red;
				mBox1.onNormal.background = Texture2D.whiteTexture;
			}
			return mBox1;
		}
	}

	public static void Separator()
    {
        GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
    }
}