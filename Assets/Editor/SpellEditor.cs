using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SpellData))]
public class SpellEditor : Editor
{
    [SerializeField] private VisualTreeAsset UsedVisualTree; // can this be a serializedfield?

    public override VisualElement CreateInspectorGUI()
    {
        // Load the stuff from UI Builder to the inspector
        VisualElement inspectorRoot = new();
        UsedVisualTree.CloneTree(inspectorRoot);
        
        // Return the finished ui
        return inspectorRoot;
    }

    
}
