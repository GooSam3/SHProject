using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SHManagerAtlasLoader : CManagerAtlasLoaderBase
{   public static new SHManagerAtlasLoader Instance { get { return CManagerAtlasLoaderBase.Instance as SHManagerAtlasLoader; } }

	public enum EAtlasType
	{
		None,
		FaceHero,
		Skill,
		Button,
		Combat,
		Dialog,
		Icon,
		Lobby,
		Mics,
		Common,
	}

	[System.Serializable]
	public class SAtalsInfo
	{
		public EAtlasType		AtlasType = EAtlasType.None;
		public SpriteAtlas	Atlas = null;
	}

	[SerializeField]
	private List<SAtalsInfo> LoadAtlas = new List<SAtalsInfo>();

	//---------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		for (int i = 0; i < LoadAtlas.Count; i++)
		{
			ProtAtlasLoaderAdd((int)LoadAtlas[i].AtlasType, LoadAtlas[i].Atlas);
		}
	}

	//----------------------------------------------------------------
	public Sprite DoMgrAtlasFindSprite(EAtlasType eAtlasType, string strSpriteName)
	{
		return FindAtlasSprite((int)eAtlasType, strSpriteName);
	}
}
