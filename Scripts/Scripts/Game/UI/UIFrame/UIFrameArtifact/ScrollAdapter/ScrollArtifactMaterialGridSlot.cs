using System;
using UnityEngine;

public class ScrollArtifactMaterialGridSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    #endregion
    #endregion

    #region System Variables
    private Action _onClicked;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void AddListener_OnClicked(Action callback)
    {
        _onClicked += callback;
    }

    public void RemoveListener_OnClicked(Action callback)
    {
        _onClicked -= callback;
    }

    #endregion

    #region Private Methods
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClicked()
    {
        _onClicked?.Invoke();
    }
    #endregion
}
