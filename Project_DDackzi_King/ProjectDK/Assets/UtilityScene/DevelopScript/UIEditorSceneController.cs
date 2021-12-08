using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEditorSceneController : MonoBehaviour
{
  
    void Start()
    {
        DKUIManager.Instance.DoUIFrameShow(nameof(DKUIFrameValuePrint));
    }

    void Update()
    {
        
    }
}
