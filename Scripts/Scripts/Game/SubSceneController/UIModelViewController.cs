using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIModelViewController : MonoBehaviour
{
    [Serializable]
    private class PCRFxRootGroup
	{
        public E_PetChangeViewType type;

        public List<Vector3> tfListview;
        public List<Vector3> tfCollection;
	}

    [SerializeField] private List<ParticleSystem> listBGFx;
    [SerializeField] private GameObject fxRoot;

    [SerializeField] private ParticleSystem fxLevelup;

    [SerializeField] private List<PCRFxRootGroup> rootTrGroup;

    // 0 == default
    public void SetFxRootType(E_PetChangeViewType viewType, UIFramePetChangeBase.E_PetChangeContentType type = UIFramePetChangeBase.E_PetChangeContentType.Content_1)
	{
        if (rootTrGroup.Count <= (int)viewType)
            return;

        var trGroup = rootTrGroup[(int)viewType];

        var tr = new List<Vector3>();

        if (type == UIFramePetChangeBase.E_PetChangeContentType.Content_1)
		{//리스트뷰
            tr = trGroup.tfListview;
		}
        else
		{//컬렉션
            tr = trGroup.tfCollection;
        }

        fxRoot.transform.SetLocalTRS(tr[0], Quaternion.Euler(tr[1]), tr[2]);
    }
    public void SetGradeFx(byte grade)
    {
        for(int i =0;i<listBGFx.Count;i++)
        {
            if (i + 1 == grade)
            {
                if (listBGFx[i].gameObject.activeSelf == false)
                {
                    listBGFx[i].gameObject.SetActive(true);
                    listBGFx[i].Play(true);
                }
            }
            else
            {
                if(listBGFx[i].gameObject.activeSelf ==true)
                {
                    listBGFx[i].Stop(true);
                    listBGFx[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void PlayLevelupFx()
    {
        if (fxLevelup.gameObject.activeSelf == false)
            fxLevelup.gameObject.SetActive(true);

        fxLevelup.Play(true);
    }

    public void ResetFx()
    {
        SetGradeFx(0);

        if (fxLevelup.gameObject.activeSelf == true)
            fxLevelup.gameObject.SetActive(false);
        fxLevelup.Stop(true);
    }
}
