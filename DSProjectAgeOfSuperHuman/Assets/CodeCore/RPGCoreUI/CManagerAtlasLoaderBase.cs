using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
public abstract class CManagerAtlasLoaderBase : CManagerTemplateBase<CManagerAtlasLoaderBase>
{
	private Dictionary<int, SpriteAtlas> m_mapAtlasInstance = new Dictionary<int, SpriteAtlas>();
	//--------------------------------------------------------
	protected void ProtAtlasLoaderAdd(int hAtlasID, SpriteAtlas pAtlas)
	{
		m_mapAtlasInstance[hAtlasID] = pAtlas;
	}

	protected Sprite FindAtlasSprite(int hSpriteType, string strSpriteName)
	{
		Sprite pFindSprite = null;
		if (m_mapAtlasInstance.ContainsKey(hSpriteType))
		{
			SpriteAtlas pSpriteAtlas = m_mapAtlasInstance[hSpriteType];
			pFindSprite = pSpriteAtlas.GetSprite(strSpriteName);
		}

		return pFindSprite;
	}


}
