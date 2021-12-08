using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 메테리얼의 _EmissionPower 의 값을 증감 시킨다. (도민님 요청사항) </summary>
public class ZEmissionPowerUpdate : MonoBehaviour
{
    [SerializeField]
    private float MinPowerValue = 0.25f;
    [SerializeField]
    private float MaxPowerValue = 1f;
    [SerializeField]
    private float Duration = 1f;
    [SerializeField]
    private Ease EaseType = Ease.InCubic;

    private int PropertyId = Shader.PropertyToID("_EmissionPower");

    private List<float> m_listOriginalValue = new List<float>();
    private List<Renderer> m_listCachedRenderers = new List<Renderer>();

    private Tweener TweenPower = null;

    private void Start()
    {
        m_listCachedRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>(true));
        SetOriginalValue();
    }

    private void OnEnable()
    {
        OnIncrease();
    }

    private void OnDisable()
    {
        KillTweener();
        ResetOriginalValue();
    }

    private void SetOriginalValue()
    {
        m_listOriginalValue.Clear();

        foreach (Renderer r in m_listCachedRenderers)
        {
            foreach (var mat in r.materials)
            {
                float value = 0f;

                if (true == mat.HasProperty(PropertyId))
                {
                    value = mat.GetFloat(PropertyId);      
                }

                m_listOriginalValue.Add(value);
            }
        }
    }

    private void ResetOriginalValue()
    {
        int index = 0;
        foreach (Renderer r in m_listCachedRenderers)
        {
            if (null == r)
                continue;

            foreach (var mat in r.materials)
            {
                if (true == mat.HasProperty(PropertyId))
                {
                    mat.SetFloat(PropertyId, m_listOriginalValue[index]);
                }
                ++index;
            }
        }        
    }

    private void ChangeMaterialFloat(float power)
    {
        int index = 0;
        foreach (Renderer r in m_listCachedRenderers)
        {
            foreach (var mat in r.materials)
            {                
                ChangeMaterialFloat(mat, m_listOriginalValue[index] * power);
            }
        }
    }

    private void ChangeMaterialFloat(Material srcMat, float newValue)
    {        
        if (!srcMat.HasProperty(PropertyId))
            return;

        srcMat.SetFloat(PropertyId, newValue);        
    }

    private void OnIncrease()
    {
        KillTweener();
        TweenPower = DOTween.To(()=> MinPowerValue, value => ChangeMaterialFloat(value),  MaxPowerValue, Duration).OnComplete(() =>
        {
            OnDecrease();
        }).SetEase(EaseType);
    }

    private void OnDecrease()
    {
        KillTweener();
        TweenPower = DOTween.To(() => MaxPowerValue, value => ChangeMaterialFloat(value), MinPowerValue, Duration).OnComplete(() =>
        {
            OnIncrease();
        }).SetEase(EaseType);
    }

    private void KillTweener()
    {
        if(null != TweenPower)
        {
            TweenPower.Kill();
        }

        TweenPower = null;
    }
}