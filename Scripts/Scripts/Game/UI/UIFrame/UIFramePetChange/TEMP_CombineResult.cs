using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 합성 결과용 임시 스크립트
/// 반드시 삭제
/// 강림/펫/탈것 프레임 내 TEMP_CombineResult도 삭제~~~~
/// </summary>
public class TEMP_CombineResult : MonoBehaviour
{
    [SerializeField] Transform parentTR;

    [SerializeField] Text txt;
    [SerializeField] UIPetChangeListItem pcr;

    private List<GameObject> listItems = new List<GameObject>();

    public void Show(List<C_PetChangeData> change, List<string> keep)
    {
        Clear();

        gameObject.SetActive(true);

        foreach(var iter in change)
        {
            if (listItems.Count > 36)
                break;

            var obj = Instantiate(pcr.gameObject, parentTR);
            obj.GetComponent<UIPetChangeListItem>().SetSlotSimple(iter);
            obj.SetActive(true);
            listItems.Add(obj);
        }

        foreach(var iter in keep)
        {
            if (listItems.Count > 36)
                break; 
            
            var obj = Instantiate(txt.gameObject, parentTR);
            obj.GetComponent<Text>().text = iter;

            obj.SetActive(true);
            listItems.Add(obj);
        }
    }

    private void Clear()
    {
        foreach (var iter in listItems)
            DestroyImmediate(iter);

        listItems.Clear();
    }
}
