using UnityEngine;
using UnityEngine.UI;

public class UIMessageListItem : MonoBehaviour
{
    #region Variable
    public Text TxtObj = null;
    #endregion

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}