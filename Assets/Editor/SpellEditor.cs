using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SpellData))]
public class SpellEditor : Editor
{
    public VisualTreeAsset UsedVisualTree; // can this be a serializedfield?

    //private Toggle testToggle;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement myInspector = new();

        // add in the stuff from ui builder
        UsedVisualTree.CloneTree(myInspector);

        /*// Find the toggle
        testToggle = root.Q<Toggle>("TestBool");

        // Set up an event for when the toggle is changed
        testToggle.RegisterCallback<ClickEvent>(TestToggleClicked);*/

        // return the finished ui
        return myInspector;
    }

    // Button Events
    /*private void TestToggleClicked(ClickEvent clickEvent)
    {
        Debug.Log($"Test toggle clicked! Toggle Value: {testToggle.value}");
    }*/
}
