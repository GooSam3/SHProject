using DG.Tweening;
using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> entity 모델 관련 처리 </summary>
public abstract class EntityComponentModelBase : EntityComponentBase<EntityBase>
{
    private Action<GameObject> mEventLoadedModel;

    public GameObject ModelGo { get; private set; }

    public Resource_Table ResourceTable { get; protected set; }

    protected Dictionary<E_ModelSocket, Transform> m_dicSocket = new Dictionary<E_ModelSocket, Transform>();
    
    protected string mAssetName = string.Empty;

    protected LODGroup mLODGroup = null;

    public float ModelScaleFactor { get; protected set; } = 1f;

    protected override void OnInitializeComponentImpl()
    {
        InitSocket();
        OnSetTable();
    }

    protected override void OnDestroyImpl()
    {
        base.OnDestroyImpl();        
        DestroyModel(true);
    }

    /// <summary> 모든 소켓 root로 초기화 </summary>
    private void InitSocket()
    {
        m_dicSocket.Clear();

        foreach(E_ModelSocket socket in Enum.GetValues(typeof(E_ModelSocket)))
        {
            m_dicSocket.Add(socket, Owner.transform);
        }
    }

    /// <summary> 로드된 model gameobject로 소켓 셋팅 </summary>
    protected virtual void SetSocket()
    {
        foreach (E_ModelSocket socket in Enum.GetValues(typeof(E_ModelSocket)))
        {
            Transform socketTrans = ModelGo.transform.FindTransform($"Socket_{socket}");

            if (null == socketTrans)
                continue;

            m_dicSocket[socket] = socketTrans;
        }
    }

    /// <summary> 해당 소켓 얻어옴 </summary>
    public Transform GetSocket(E_ModelSocket socket)
    {
        return m_dicSocket[socket];
    }

    /// <summary> 모델 로드전 테이블 셋팅 </summary>
    protected abstract void OnSetTable();
    
    /// <summary> 모델 로드 후 처리 </summary>
    protected abstract void OnPostSetModel();

    public void SetModel()
    {
        if (Owner.EntityType == E_UnitType.None)
            return;

        OnSetTable();

        SetAssetName();
        SetModel(mAssetName);
    }

    protected virtual void SetAssetName()
    {
        if (null != ResourceTable && false == string.IsNullOrEmpty(ResourceTable.ResourceFile))
        {
            if(false == string.IsNullOrEmpty(mAssetName) && false == mAssetName.Equals(ResourceTable.ResourceFile))
            {
                //기존 모델 제거
                DestroyModel();
            }
            mAssetName = ResourceTable.ResourceFile;
        }
    }

    public void SetModel(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            //프리펩 자체에 붙어있는 경우 예외처리 (기믹)
            SetModelRenderers();
            return;
        }   

        ZPoolManager.Instance.Spawn(E_PoolType.Character, assetName, (modelGo) =>
        {
            if(null == modelGo)
            {
                ZLog.LogError(ZLogChannel.Model, $"모델이 Null이다. assetName = ({assetName})");
                return;
            }

            if (this == null)
            {
                ZPoolManager.Instance.Return(modelGo);
                return;
            }

            DestroyModel();

            mAssetName = assetName;
            ModelGo = modelGo;
            ModelGo.transform.parent = CachedTransform;
            ModelGo.transform.localPosition = Vector3.zero;
            ModelGo.transform.localRotation = Quaternion.identity;

            //레이어 변경
            if (true == Owner.IsMyPc || ((Owner is ZVehicle vehicle) && true == vehicle.OwnerCharacter.IsMyPc))
            {
                //내 캐릭터고 내 캐릭터의 탈것이라면 player로 셋팅. 파티클은 무조건 entity
                Owner.gameObject.SetLayersRecursively<ParticleSystem>(UnityConstants.Layers.Player, UnityConstants.Layers.Entity);
            }
            else
            {
                Owner.gameObject.SetLayersRecursively(UnityConstants.Layers.Entity);
            }   

            SetSocket();

            mLODGroup = modelGo.GetComponent<LODGroup>();

            SetModelRenderers();

            OnPostSetModel();
            mEventLoadedModel?.Invoke(ModelGo);
        });
    }

    protected void DestroyModel(bool bDestroy = false)
    {
        if (null != ModelGo)
        {  
            if(ZPoolManager.Instance)
            {
                ZPoolManager.Instance.Return(ModelGo);
            }

            InitSocket();
        }

        ModelGo = null;

        ResetRenderers();
    }

    #region ===== :: Renderer 관련 :: =====
    /// <summary> 렌더러 캐싱 </summary>
    private List<Renderer> m_listCachedRenderers = new List<Renderer>();

    private const string DISSOLVE_KEYWORD = "_USEDISSOLVE_ON";
    private int DISSOLVE_ID = Shader.PropertyToID("_Dissolve_Progress");
    private bool IsInactiveRenderers = false;

    private const string ATTRIBUTE_COLOR_KEY = "_USEOBJECTEFF_ON";
    private int ATTRIBUTE_ID = Shader.PropertyToID("_UseEffectColor");

    /// <summary> 디졸브 연출 </summary>
    private Tweener TweenDissolve;

    /// <summary> 히트림 연출 </summary>
    private Sequence TweenHitStrength;
    private int HIT_RIM_COLOR_ID = Shader.PropertyToID( "_HitColor" );
    private int HIT_RIM_STRENGTH_ID = Shader.PropertyToID( "_HitStrength" );

    private void SetModelRenderers()
    {
        m_listCachedRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>(true));        
        ResetDissolve();

        //비활성화 상태라면 안보이게 처리
        if (true == IsInactiveRenderers)
            SetActiveRenderers(false);
    }

    /// <summary> 렌더러 on/off </summary>
    public void SetActiveRenderers(bool bActive)
    {
        IsInactiveRenderers = !bActive;
        foreach (Renderer r in m_listCachedRenderers)
        {
            if (null == r)
                continue;

            r.enabled = bActive;
        }
    }

    /// <summary> 그림자 on/off </summary>
    public void SetShadowCasting( bool bActive )
    {
        foreach( Renderer r in m_listCachedRenderers ) {
            if( null == r )
                continue;

            if( bActive ) {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            else {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }

    /// <summary> 림 효과 </summary>
    public void HitRimLight( Color color, float startHitStrenth, float duration, bool showImmediately )
    {
        TweenHitStrength?.Kill( true );

        ChangeMaterialColor( HIT_RIM_COLOR_ID, color );

        if( showImmediately ) {
            TweenHitStrength = DOTween.Sequence().
                Join( DOTween.To( () => startHitStrenth, ( value ) => ChangeMaterialFloat( HIT_RIM_STRENGTH_ID, value ), 0f, duration * 0.7f ).SetEase( Ease.InCubic ) ).Play();
        }
        else {
            TweenHitStrength = DOTween.Sequence().
                Join( DOTween.To( () => 0f, ( value ) => ChangeMaterialFloat( HIT_RIM_STRENGTH_ID, value ), startHitStrenth, duration * 0.1f ).SetEase( Ease.OutCubic ) ).
                AppendInterval( duration * 0.2f ).
                Append( DOTween.To( () => startHitStrenth, ( value ) => ChangeMaterialFloat( HIT_RIM_STRENGTH_ID, value ), 0f, duration * 0.7f ).SetEase( Ease.InCubic ) ).
                Play();
        }
	}

    public void ResetRimLight()
    {
        TweenHitStrength?.Kill( true );
        ChangeMaterialFloat( HIT_RIM_STRENGTH_ID, 0 );
    }

    public void Attribute_Effect(E_UnitAttributeType color)
	{
        if(color == E_UnitAttributeType.None)
		{
            DisableKeywordForRenderer(ATTRIBUTE_COLOR_KEY);
            return;
		}
        
        EnableKeywordForRenderer(ATTRIBUTE_COLOR_KEY);
        switch(color)
		{
            case E_UnitAttributeType.Water: ChangeMaterialColor(ATTRIBUTE_ID, ResourceSetManager.Instance.SettingRes.Palette.Attribute_Ice); break;
            case E_UnitAttributeType.Fire: ChangeMaterialColor(ATTRIBUTE_ID, ResourceSetManager.Instance.SettingRes.Palette.Attribute_Fire); break;
            case E_UnitAttributeType.Electric: ChangeMaterialColor(ATTRIBUTE_ID, ResourceSetManager.Instance.SettingRes.Palette.Attribute_Electric); break;
            default: DisableKeywordForRenderer(ATTRIBUTE_COLOR_KEY);  break;
        }   
    }

    /// <summary> 디졸브 연출 </summary>
    public void Dissolve(bool bDissolve, float duration = 3f, float delayTime = 0f, Action onFinish = null)
    {
        TweenDissolve?.Kill(true);

        //디졸브 관련 키워드 활성화
        EnableKeywordForRenderer(DISSOLVE_KEYWORD);

        if (bDissolve)
        {
            //사라지기
            TweenDissolve = DOTween.To( () => 0f, ( value ) => ChangeMaterialFloat( DISSOLVE_ID, value ), 1f, duration ).OnComplete( () => 
            {
                onFinish?.Invoke();
            } ).SetDelay( delayTime ).OnStart( () => {
                // 그림자 꺼주기
                SetShadowCasting( false );
            } );
        }
        else
        {
            //나타나기
            TweenDissolve = DOTween.To(() => 1f, (value) => ChangeMaterialFloat(DISSOLVE_ID, value), 0f, duration).OnComplete(() =>
            {
                //디졸브 비활성화
                ResetDissolve();
                onFinish?.Invoke();
            });
        }
    }

    /// <summary> 렌더러 관련 처리 리셋 </summary>
    private void ResetRenderers()
    {
        SetActiveRenderers(true);        
        ResetDissolve();
        m_listCachedRenderers.Clear();
    }

    /// <summary> 디졸브 리셋 </summary>
    private void ResetDissolve()
    {
        DisableKeywordForRenderer(DISSOLVE_KEYWORD);
        SetShadowCasting(true);
        TweenDissolve?.Kill(true);
        TweenDissolve = null;
        ChangeMaterialFloat(DISSOLVE_ID, 0);
    }

    /// <summary> keyword 화성화 </summary>
    private void EnableKeywordForRenderer(string keyword)
    {
        foreach (Renderer r in m_listCachedRenderers)
        {
            if (null == r)
                continue;

            foreach (var mat in r.materials)
            {
                mat.EnableKeyword(keyword);
            }
        }
    }

    /// <summary> keyword 비화성화 </summary>
    private void DisableKeywordForRenderer(string keyword)
    {
        foreach (Renderer r in m_listCachedRenderers)
        {
            if (null == r)
                continue;

            foreach (var mat in r.materials)
            {
                mat.DisableKeyword(keyword);
            }
        }
    }

    /// <summary> 메테리얼 프로퍼티 변경 </summary>
    private void ChangeMaterialFloat(string propertyId, float newValue)
    {
        ChangeMaterialFloat(Shader.PropertyToID(propertyId), newValue);
    }

    /// <summary> 메테리얼 프로퍼티 변경 </summary>
    public void ChangeMaterialFloat(int propertyId, float newValue)
    {
        foreach (Renderer r in m_listCachedRenderers)
        {
            if (null == r)
                continue;

            foreach (var mat in r.materials)
            {
                ChangeMaterialFloat(mat, propertyId, newValue);
            }
        }
    }

    /// <summary> 메테리얼 프로퍼티 변경 </summary>
    private void ChangeMaterialFloat(Material srcMat, int propertyId, float newValue)
    {
        if (!srcMat.HasProperty(propertyId))
            return;

        srcMat.SetFloat(propertyId, newValue);
    }

    /// <summary> 메테리얼 프로퍼티 칼라 변경 </summary>
    public void ChangeMaterialColor( int propertyId, Color color )
    {
        foreach( Renderer r in m_listCachedRenderers ) {
            if( null == r )
                continue;

            foreach( var mat in r.materials ) {
                ChangeMaterialColor( mat, propertyId, color );
            }
        }
    }

    /// <summary> 메테리얼 프로퍼티 칼라 변경 </summary>
    private void ChangeMaterialColor( Material srcMat, int propertyId, Color color )
    {
        if( !srcMat.HasProperty( propertyId ) )
            return;

        srcMat.SetColor( propertyId, color );
    }

    #endregion

    #region ===== :: 이벤트 :: =====
    /// <summary> 모델 로드 완료시 처리 </summary>
    public void DoAddEventLoadedModel(Action<GameObject> action)
    {
        DORemoveEventLoadedModel(action);
        mEventLoadedModel += action;
    }

    public void DORemoveEventLoadedModel(Action<GameObject> action)
    {
        mEventLoadedModel -= action;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
	{
		if (null != mLODGroup && CameraManager.hasInstance)
		{
			var curLevel = mLODGroup.GetCurrentLOD(CameraManager.Instance.Main);
			UnityEditor.Handles.Label(transform.position, $"LOD {curLevel}", UnityEditor.EditorStyles.boldLabel);
		}
	}
#endif
}
