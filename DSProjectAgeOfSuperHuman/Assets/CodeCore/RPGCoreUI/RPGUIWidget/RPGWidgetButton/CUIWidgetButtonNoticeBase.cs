using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetButtonNoticeBase : CUIWidgetButtonSingleBase
{
	[SerializeField][Header("[Button Notice]")]
	private int NoticeGroup = 0;
	[SerializeField]
	private CImage NoticeImgae = null;
}
