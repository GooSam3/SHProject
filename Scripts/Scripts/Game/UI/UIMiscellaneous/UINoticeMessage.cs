using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UINoticeMessage : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Text NoticeTxt;
    #endregion

    public void Initialize(string _txt, Color _color)
    {
        transform.name = nameof(UINoticeMessage);
        transform.localPosition = new Vector3(0,100,0);
        transform.localScale = Vector3.one;

        NoticeTxt.text = _txt;
        NoticeTxt.color = _color;

        StartCoroutine(View());
    }

    private IEnumerator View()
    {
        float time = 0.0f;

        while(time < 0.1f)
        {
            //ljh : 쿨타임 무게 등등 작게뜨는 메세지는 일반상황에서만 출력
            if (UIManager.Instance.Find<UIFrameHUD>()?.CurUIType != E_UIStyle.Normal)
            {
                break;
            }


            time += 0.001f;
            transform.localPosition += new Vector3(0, 1.5f, 0);
            yield return new WaitForSeconds(0.001f);
        }

        Destroy(gameObject);
    }
}