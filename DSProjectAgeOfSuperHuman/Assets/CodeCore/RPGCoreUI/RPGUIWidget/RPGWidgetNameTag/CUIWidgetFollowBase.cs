using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetFollowBase : CUIWidgetTemplateItemBase
{
    private Transform m_pFollowTransform = null;
	//-----------------------------------------------------
	private void Update()
	{
		gameObject.SetActive(m_pFollowTransform.gameObject.activeSelf);

		if (m_pFollowTransform != null)
		{
			UpdateFollowTarget(m_pFollowTransform);
		}
	}

	//-----------------------------------------------------
	protected void ProtFollowSetTrasform(Transform pTransform)
	{
		m_pFollowTransform = pTransform;
		UpdateFollowTarget(pTransform);
	}

	//-----------------------------------------------------
	private void UpdateFollowTarget(Transform pFollowTarget)
	{
		Vector3 vecScreenPos = Camera.main.WorldToScreenPoint(pFollowTarget.position);
		SetUIPosition(vecScreenPos.x, vecScreenPos.y);
		OnWidgetFollowUpdate(vecScreenPos);
	}

	//-----------------------------------------------------
	protected virtual void OnWidgetFollowUpdate(Vector2 vecScreenPos) { }

}
