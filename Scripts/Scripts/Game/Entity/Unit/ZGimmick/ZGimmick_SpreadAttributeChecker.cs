using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 확산되는 속성을 체크 하고 추가 기능을 발동하게 하기 위한 클래스 </summary>
public class ZGimmick_SpreadAttributeChecker : MonoBehaviour
{
    [Header("체크할 속성")]
    [SerializeField]
    private E_UnitAttributeType AttributeType = E_UnitAttributeType.Fire;

    [Header("체크할 거리")]
    [SerializeField]
    private float CheckDistance = 5f;

    //[Header("체크할 개수")]
    //[SerializeField]
    private const int MAX_CHECK_COUNT = 3;

    [Header("조건 만족시 생성되는 기믹 ")]
    [SerializeField]
    private GameObject GimmickPrefab = null;

    [Header("생성되는 기믹의 위치 offset")]
    [SerializeField]
    private Vector3 Offset = default;

    [Header("생성되는 기믹의 회전값")]
    [SerializeField]
    private Quaternion Rotate = Quaternion.identity;

    private bool CheckDirty = false;
    
    private List<ZGimmick> m_listGimmick = new List<ZGimmick>();

    [NonSerialized]
    public bool IsDefault = false;

    public void Start()
    {
        if(false == IsDefault)
        {
            ZGimmickManager.Instance.AddSpreadAttributeChecker(this);
        }
        
        ZMonoManager.Instance.AddUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, Check);
    }

    public void OnDestroy()
    {
        if (false == IsDefault)
        {
            ZGimmickManager.Instance.RemoveSpreadAttributeChecker(this);
        }
        
        if(ZMonoManager.hasInstance)
        {
            ZMonoManager.Instance.RemoveUpdateCall(ZMonoManager.UpdateMode.NormalUpdate, Check);
        }
    }

    public void Add(ZGimmick gimmick, E_UnitAttributeType attributeType)
    {
        if (false == AttributeType.HasFlag(attributeType))
        {
            return;
        }
            
        if (m_listGimmick.Contains(gimmick))
            return;

        m_listGimmick.Add(gimmick);
        CheckDirty = true;
    }

    public void Remove(ZGimmick gimmick)
    {
        if (false == m_listGimmick.Contains(gimmick))
        {
            return;
        }

        m_listGimmick.Remove(gimmick);
        CheckDirty = true;
    }

    public void Clear()
    {
        m_listGimmick.Clear();
        m_listInvokedData.Clear();
        m_listInvokedGimmick.Clear();
    }

    private void Check()
    {
        if (false == CheckDirty)
        {
            return;
        }

        CheckDirty = false;

        float sqrtDistance = CheckDistance * CheckDistance;

        //조건에 만족하는 기믹 연결. 인덱스 순서로 한방향으로만 연결 시키자.
        List<SpreadLink> links = new List<SpreadLink>();
        for (int i = 0; i < m_listGimmick.Count; ++i)
        {
            if(null == m_listGimmick[i])
            {
                continue;
            }

            Vector3 position = m_listGimmick[i].Position;

            var link = new SpreadLink();
            link.Index = i;
            for (int j = i+1; j < m_listGimmick.Count; ++j)
            {
                if(null == m_listGimmick[j])
                {
                    continue;
                }

                float calcSqrtDistance = Vector3.SqrMagnitude(position - m_listGimmick[j].Position);

                if (calcSqrtDistance > sqrtDistance)
                    continue;

                link.Linked.Add(j);
            }
            links.Add(link);
        }       
        
        for(int i = 0; i < links.Count; ++i)
        {
            SpreadLink link = links[i];
            for (int j = 0; j < link.Linked.Count; ++j)
            {
                int linkIndex = link.Linked[j];                

                var linked = links.Find((item) => { return item.Index == linkIndex; });

                var findedList = linked.Linked.FindAll(checkIndex => 
                {                    
                    return link.Linked.Contains(checkIndex);
                });

                foreach(var findedIndex in findedList)
                {
                    bool bEquals = false;
                    List<ZGimmick> list = new List<ZGimmick>();

                    list.Add(m_listGimmick[i]);
                    list.Add(m_listGimmick[linkIndex]);
                    list.Add(m_listGimmick[findedIndex]);

                    foreach(var gimmick in m_listInvokedGimmick)
                    {
                        if(list.Contains(gimmick))
                        {
                            bEquals = true;
                            break;
                        }
                    }

                    if(bEquals)
                    {
                        foreach (var invokedData in m_listInvokedData)
                        {
                            if (invokedData.Equals(ref list))
                            {
                                bEquals = true;
                                break;
                            }
                        }
                    }

                    if(false == bEquals)
                    {
                        m_listInvokedData.Add(new InvokedData(list));
                        m_listInvokedGimmick.AddRange(list);
                    }
                }
            }
        }

        InvokeSpreadAttribute();
    }

    private void InvokeSpreadAttribute()
    {
        foreach(var data in m_listInvokedData)
        {
            if(data.IsInvoked)
            {
                continue;
            }

            data.IsInvoked = true;

            float minX = 100000;//loat.MaxValue;
            float minY = 100000;//loat.MaxValue;
            float minZ = 100000;//float.MaxValue;

            float maxX = -100000;//float.MaxValue;
            float maxY = -100000;//float.MaxValue;
            float maxZ = -100000;//-float.MaxValue;

            foreach (var gimmick in data.GimmickList)
            {
                Vector3 position = gimmick.Position;

                minX = Mathf.Min(position.x, minX);
                minY = Mathf.Min(position.y, minY);
                minZ = Mathf.Min(position.z, minZ);

                maxX = Mathf.Max(position.x, maxX);
                maxY = Mathf.Max(position.y, maxY);
                maxZ = Mathf.Max(position.z, maxZ);
            }

            float positionX = minX + ((maxX - minX) / 2);
            float positionY = minY + ((maxY - minY) / 2);
            float positionZ = minZ + ((maxZ - minZ) / 2);

            GameObject.Instantiate(GimmickPrefab, new Vector3(positionX, positionY, positionZ) + Offset, Rotate);             
        }
    }

    private class SpreadLink
    {
        public int Index;
        public List<int> Linked = new List<int>();        
    }

    /// <summary> 발동 기믹 모음 </summary>
    private List<InvokedData> m_listInvokedData = new List<InvokedData>();
    private List<ZGimmick> m_listInvokedGimmick = new List<ZGimmick>();

    private class InvokedData
    {
        public List<ZGimmick> GimmickList = new List<ZGimmick>();

        public bool IsInvoked = false;

        public InvokedData(List<ZGimmick> list)
        {
            GimmickList.AddRange(list);
        }

        public bool Equals(ref List<ZGimmick> gimmicks)
        {
            foreach(var gimmick in GimmickList)
            {
                if (false == gimmicks.Contains(gimmick))
                    return false;
            }

            return true;
        }
    }
}