using UnityEngine;

public class UIFrameArtifactOverlayPopupBase : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    #endregion
    #endregion

    #region Protected Variables
    protected bool isOpen;

    protected UIFrameArtifact FrameArtifact;

    #endregion

    #region Properties
    public bool IsOpen { get { return isOpen; } }
    #endregion

    #region Unity Methods
    #endregion

    #region Public Methods
    #endregion

    #region Virtual
    virtual public void Initialize(UIFrameArtifact frameArtifact)
    {
        FrameArtifact = frameArtifact;
    }

    virtual public void Open()
    {
        if (IsOpen)
            return;

        gameObject.SetActive(true);
        isOpen = true;
    }

    virtual public void Close()
    {
        if (IsOpen == false)
            return;

        gameObject.SetActive(false);
        isOpen = false;
    }
    #endregion

    #region Private Methods
    #region Common
    #endregion
    #endregion

    #region Inspector Events 
    #region OnClick

    #endregion
    #endregion

}
