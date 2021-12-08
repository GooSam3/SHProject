using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameNumberTag : SHUIFrameBase
{
    public enum ENumberTagType
	{
        None,
        DamageNormal,
        DamageCiritcal,
        Miss,
        HealNormal,
        HealCritical,
        Block,
		DamageHeroTagNormal,
		DamageHeroTagCritical,
		HealHeroTagNormal,
		HealHeroTagCritical,
	}
    [System.Serializable]
    public class SNumberTagInfo
	{
        public ENumberTagType TagType = ENumberTagType.None;
        public CUIWidgetTemplate Template = null;
        public float MoveLength = 200f;
        public float SocketOffsetY = 5f;
        public float TagGapHeight = 20f;
	}
	[SerializeField]
	private SHEffectParticleNormal CriticalDamage = null;
    [SerializeField]
    private GameObject RootCanvas = null;
    [SerializeField]
    private List<SNumberTagInfo> NumberTagTemplate = null;

    private Dictionary<ENumberTagType, LinkedList<SHUIWidgetNumberTag>> m_mapNumberText = new Dictionary<ENumberTagType, LinkedList<SHUIWidgetNumberTag>>();
	//----------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();

        for (int i = 0; i < NumberTagTemplate.Count; i++)
		{
            NumberTagTemplate[i].Template.SetWidgetTemplateReturnEvent(HandleNumberTabReturn);
		}
    }

	//----------------------------------------------------
	public void DoNumberTagDamage(float fValue, bool bCritical,  Vector3 vecWorldPosition)
	{
        if (bCritical)
		{
			PrivNumberTagSocket(vecWorldPosition, fValue, ENumberTagType.DamageCiritcal);
			PrivNumberTagCriticalEffect(vecWorldPosition);
		}
        else
		{
			PrivNumberTagSocket(vecWorldPosition, fValue, ENumberTagType.DamageNormal);
		}
    }

	public void DoNumberTagDamage(float fValue, bool bCritical, Vector3 vecScreenPos, string strHitEffectName)
	{
		if (bCritical)
		{
			PrivNumberTagPrint(fValue, ENumberTagType.DamageNormal, vecScreenPos);
		}
		else
		{
			PrivNumberTagPrint(fValue, ENumberTagType.DamageCiritcal, vecScreenPos);
		}
	}

    public void DoNumberTagHeal(float fValue, bool bCritical, Vector3 vecScreenPos)
	{
		if (bCritical)
		{
			PrivNumberTagPrint(fValue, ENumberTagType.HealCritical, vecScreenPos);
		}
		else
		{
			PrivNumberTagPrint(fValue, ENumberTagType.HealNormal, vecScreenPos);
		}
	}

	public void DoNumberTagMiss(Vector3 vecWorldPosition)
    {
        PrivNumberTagSocket(vecWorldPosition, 0, ENumberTagType.Miss);
    }

    public void DoNumberTagBlock(Vector3 vecWorldPosition)
	{
		PrivNumberTagSocket(vecWorldPosition, 0, ENumberTagType.Block);
	}

	public void DoNumberTagClear()
	{
        int iChildCount = RootCanvas.transform.childCount;
        List<SHUIWidgetNumberTag> pListNumber = new List<SHUIWidgetNumberTag>();
        for (int i = 0; i < iChildCount; i++)
		{
            SHUIWidgetNumberTag pNumberText = RootCanvas.transform.GetChild(i).gameObject.GetComponent<SHUIWidgetNumberTag>();
            if (pNumberText != null)
			{
                pListNumber.Add(pNumberText);
			}
		}

        for (int i = 0; i < pListNumber.Count; i++)
		{
            pListNumber[i].DoTemplateItemReturn();
		}
    }

    //---------------------------------------------------
    private void HandleNumberTabReturn(CUIWidgetTemplateItemBase pItem)
	{
        SHUIWidgetNumberTag pNumberText = pItem as SHUIWidgetNumberTag;
        LinkedList<SHUIWidgetNumberTag> pListNumber = m_mapNumberText[pNumberText.GetNumberTextSocket()];

        pListNumber.Remove(pNumberText);
    }

	//-----------------------------------------------------
    private void PrivNumberTagAdd(SHUIWidgetNumberTag pNumberText, ENumberTagType eTagType, float fTagGapHeight, float fSocketOffsetY)
	{
        LinkedList<SHUIWidgetNumberTag> pListNumber = null;
        if (m_mapNumberText.ContainsKey(eTagType))
		{
            pListNumber = m_mapNumberText[eTagType];
		}
        else
		{
            pListNumber = new LinkedList<SHUIWidgetNumberTag>();
            m_mapNumberText[eTagType] = pListNumber;
		}
        pListNumber.AddFirst(pNumberText);
        PrivNumberTagArrange(pListNumber, fTagGapHeight, fSocketOffsetY);
	}

    private void PrivNumberTagArrange(LinkedList<SHUIWidgetNumberTag> pListNumber, float fTagGapHeight, float fSocketOffsetY)
	{
        LinkedList<SHUIWidgetNumberTag>.Enumerator it = pListNumber.GetEnumerator();

        Vector3 vecOffset = Vector3.zero;
        while (it.MoveNext())
		{
            SHUIWidgetNumberTag pNumberText = it.Current;     
            pNumberText.DoNuberAdjustPosition(vecOffset);
            vecOffset.y = fTagGapHeight + fSocketOffsetY; 
        }
    }

    private SNumberTagInfo FindNumberTag(ENumberTagType eNumberTagType)
	{
        SNumberTagInfo pNumberTag = null;
        for (int i = 0; i < NumberTagTemplate.Count; i++)
		{
            if (NumberTagTemplate[i].TagType == eNumberTagType)
			{
                pNumberTag = NumberTagTemplate[i];
                break;
			}
		}

        return pNumberTag;
	}

    private void PrivNumberTagSocket(Vector3 vecWorldPosition, float fValue, ENumberTagType eNumberTagType)
	{
		Vector3 vecOrigin = WorldToCanvas(vecWorldPosition);
        PrivNumberTagPrint(fValue, eNumberTagType, vecOrigin);
	}

    private void PrivNumberTagPrint(float fValue , ENumberTagType eNumberTagType, Vector3 vecOrigin)
	{
		SNumberTagInfo pNumberTemplate = FindNumberTag(eNumberTagType);
		Vector3 vecDest = vecOrigin;
		vecDest.y += pNumberTemplate.MoveLength;
		SHUIWidgetNumberTag pNumberTag = pNumberTemplate.Template.DoTemplateRequestItem(RootCanvas.transform) as SHUIWidgetNumberTag;
		pNumberTag.DoNumberText((int)fValue, eNumberTagType, vecOrigin, vecDest);
		PrivNumberTagAdd(pNumberTag, eNumberTagType, pNumberTemplate.TagGapHeight, pNumberTemplate.SocketOffsetY);
	}

	private void PrivNumberTagCriticalEffect(Vector3 vecWorldPosition)
	{
		CriticalDamage.DoEffectStart(vecWorldPosition, null);
		SHManagerStage.Instance.GetMgrStageCurrent().DoStageEffectCameraShake(0.5f, 180f, 0.2f);
	}
}
