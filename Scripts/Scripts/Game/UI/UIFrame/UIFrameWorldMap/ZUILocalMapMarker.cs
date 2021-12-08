using GameDB;
using System.Collections.Generic;
using UnityEngine;

public class ZUILocalMapMarker : CUGUIWidgetTemplateItemBase
{
    private List<ZUILocalMapMarkerSlotItem> m_listMapMarker = new List<ZUILocalMapMarkerSlotItem>();
    //-------------------------------------------------------------
    public void DoMapMarkerLocate(Vector2Int _LocalPosition, uint _PortalID, string _PortalName, E_PortalType _PortalType)
    {
        ZUILocalMapMarkerSlotItem Item = ProtUIScrollSlotItemRequest() as ZUILocalMapMarkerSlotItem;
        Item.transform.localPosition = new Vector3(_LocalPosition.x, _LocalPosition.y, 0);
        Item.SetLocalMapMakerInfo(_PortalID, _PortalName, _PortalType);
        Item.SetFocus(false);
        m_listMapMarker.Add(Item);
    }

    public void SetFocus(uint tid)
    {
        for (int i = 0; i < m_listMapMarker.Count; i++)
		{
            if (m_listMapMarker[i].pPortalID == tid)
			{
                m_listMapMarker[i].SetFocus(true);
            }
            else
			{
                m_listMapMarker[i].SetFocus(false);
            }
		}
    }

    public ZUILocalMapMarkerSlotItem FindMarker(uint tid)
    {
        return m_listMapMarker.Find((item) => item.pPortalID == tid);
    }

    public void ClearFocus()
    {
        foreach (var iter in m_listMapMarker)
        {
            iter.SetFocus(false);
            break;
        }
    }

    public void DoMapMarkerClear()
    {
        for (int i = 0; i < m_listMapMarker.Count; i++)
        {
            ProtUIScrollSlotItemReturn(m_listMapMarker[i]);
        }
        m_listMapMarker.Clear();
    }
}
