using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CAtlasReference : CMonoBase
{
    [SerializeField]
    private List<SpriteAtlas> Atlas = new List<SpriteAtlas>();
    private Dictionary<string, Sprite> m_mapSpriteInstance = new Dictionary<string, Sprite>();
       
    //------------------------------------------------------------
    protected override void OnUnityAwake() 
    {
        base.OnUnityAwake();
        PrivAtlasReferencing();
    }

    //---------------------------------------------------------------
    public Sprite GetSprite(string _spriteName)
    {
        Sprite result = null;
        m_mapSpriteInstance.TryGetValue(_spriteName, out result);
        return result;        
    }

    //-------------------------------------------------------------
    private void PrivAtlasReferencing()
    {
        m_mapSpriteInstance.Clear();

        for (int i = 0; i < Atlas.Count; i++)
        {
            if (Atlas[i] == null) continue;

            int Count = Atlas[i].spriteCount;
            Sprite [] ArraySprite = new Sprite[Count];
            Atlas[i].GetSprites(ArraySprite);

            for (int j = 0; j < ArraySprite.Length; j++)
            {
                if (ArraySprite[j] != null)
                {
                    ArraySprite[j].name = ArraySprite[j].name.Replace("(Clone)", "").Trim();
                    m_mapSpriteInstance[ArraySprite[j].name] = ArraySprite[j];
                }
            }
        }
    }

}
