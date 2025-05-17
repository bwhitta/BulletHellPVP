using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SpellModule))]
public class SpellModuleEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualTreeAsset usedVisualTree = (VisualTreeAsset)EditorGUIUtility.Load("Assets/Editor/Resources/SpellModuleEditorVisualTree.uxml");

        VisualElement container = new();

        // add in the stuff from ui builder
        usedVisualTree.CloneTree(container);

        // return the finished ui
        return container;
    }
}
