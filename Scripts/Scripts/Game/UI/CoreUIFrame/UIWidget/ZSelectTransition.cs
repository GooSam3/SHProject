using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[System.Serializable]
public class ZSelectTransition
{
    [FormerlySerializedAs("highlightGraphic")]
    [FormerlySerializedAs("m_HighlightGraphic")]
    [SerializeField]
    public Graphic m_TargetGraphic;

    public Image image
    {
        get { return m_TargetGraphic as Image; }
        set { m_TargetGraphic = value; }
    }

    [FormerlySerializedAs("transition")]
    [SerializeField]
    public Selectable.Transition m_Transition = Selectable.Transition.ColorTint;

    // Colors used for a color tint-based transition.
    [FormerlySerializedAs("colors")]
    [SerializeField]
    public ColorBlock m_Colors = ColorBlock.defaultColorBlock;

    // Sprites used for a Image swap-based transition.
    [FormerlySerializedAs("spriteState")]
    [SerializeField]
    public SpriteState m_SpriteState;

    public void StartColorTween(Color targetColor, bool instant)
    {
        if (m_TargetGraphic == null)
            return;

        m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, true, true);
    }

    public void DoSpriteSwap(Sprite newSprite)
    {
        if (image == null)
            return;

        image.overrideSprite = newSprite;
    }
}
