using UnityEngine;
using UnityEngine.UI;

public class UIFloatingText : FollowTarget
{
	[SerializeField] Image imgBackground;
	[SerializeField] Text txtTarget;

	public bool VisibleBackground
	{
		set { imgBackground.enabled = value; }
	}

	public UIFloatingText Set(Transform target, string info, Color? textColor)
	{
		ZLog.Log(ZLogChannel.UI, $"UIFloatingText.Set() : {info}", target);

		VisibleBackground = true;

		this.Init(target);
		this.txtTarget.text = info;
		this.txtTarget.color = textColor.HasValue ? textColor.Value : Color.white;

		return this;
	}
}
