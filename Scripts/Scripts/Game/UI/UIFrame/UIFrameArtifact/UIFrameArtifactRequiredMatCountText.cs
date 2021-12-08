using GameDB;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameArtifactRequiredMatCountText : MonoBehaviour
{
    public Text txt;

    public void Set(byte grade, E_PetType type, uint curCnt, uint requiredCnt)
    {
        string tierTxt = DBLocale.GetText(DBUIResouce.GetTierText(grade));
        string cntState = string.Format("<color=#FFFFFF>{0}/{1}</color>", curCnt, requiredCnt);

        txt.text = string.Format(DBLocale.GetText("Artifact_Upgrade_Material_Text")
           , DBUIResouce.GetItemGradeFormat(tierTxt, grade)
           , type == E_PetType.Pet ? "펫" : "탈것"
           , requiredCnt
           , curCnt);
        // , cntState);
    }
}
