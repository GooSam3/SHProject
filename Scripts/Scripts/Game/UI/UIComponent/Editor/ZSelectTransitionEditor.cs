using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEditor;
using UnityEditor.UI;

[CustomPropertyDrawer(typeof(ZSelectTransition), true)]
public class ZSelectTransitionEditor : PropertyDrawer
{
    SerializedProperty m_TargetGraphicProperty;
    SerializedProperty m_TransitionProperty;
    SerializedProperty m_ColorBlockProperty;
    SerializedProperty m_SpriteStateProperty;
    SerializedProperty m_AnimTriggerProperty;

    AnimBool m_ShowColorTint = new AnimBool();
    AnimBool m_ShowSpriteTrasition = new AnimBool();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return -2f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        m_TargetGraphicProperty = property.FindPropertyRelative("m_TargetGraphic");
        m_TransitionProperty = property.FindPropertyRelative("m_Transition");
        m_ColorBlockProperty = property.FindPropertyRelative("m_Colors");
        m_SpriteStateProperty = property.FindPropertyRelative("m_SpriteState");

        var trans = GetTransition(m_TransitionProperty);
        m_ShowColorTint.value = (trans == Selectable.Transition.ColorTint);
        m_ShowSpriteTrasition.value = (trans == Selectable.Transition.SpriteSwap);

        var graphic = m_TargetGraphicProperty.objectReferenceValue as Graphic;

        m_ShowColorTint.target = (!m_TransitionProperty.hasMultipleDifferentValues && trans == Button.Transition.ColorTint);
        m_ShowSpriteTrasition.target = (!m_TransitionProperty.hasMultipleDifferentValues && trans == Button.Transition.SpriteSwap);

        EditorGUILayout.PropertyField(m_TransitionProperty);
        ++EditorGUI.indentLevel;
        {
            if (trans == Selectable.Transition.ColorTint || trans == Selectable.Transition.SpriteSwap)
            {
                EditorGUILayout.PropertyField(m_TargetGraphicProperty);
            }

            switch (trans)
            {
                case Selectable.Transition.ColorTint:
                    if (graphic == null)
                        EditorGUILayout.HelpBox("You must have a Graphic target in order to use a color transition.", MessageType.Warning);
                    break;

                case Selectable.Transition.SpriteSwap:
                    if (graphic as Image == null)
                        EditorGUILayout.HelpBox("You must have a Image target in order to use a sprite swap transition.", MessageType.Warning);
                    break;
                case Selectable.Transition.Animation:
                    EditorGUILayout.HelpBox("지원하지 않습니다.", MessageType.Error);
                    break;
            }
            
            if (EditorGUILayout.BeginFadeGroup(m_ShowColorTint.faded))
            {
                EditorGUILayout.PropertyField(m_ColorBlockProperty);
            }
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(m_ShowSpriteTrasition.faded))
            {
                EditorGUILayout.PropertyField(m_SpriteStateProperty);
            }
            EditorGUILayout.EndFadeGroup();

        }
        --EditorGUI.indentLevel;

        EditorGUILayout.Space();
    }

    static Selectable.Transition GetTransition(SerializedProperty transition)
    {
        return (Selectable.Transition)transition.enumValueIndex;
    }
}
