using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SpellModule))]
public class SpellModuleEditor : PropertyDrawer
{
    private readonly string[] hiderBoolNames = { };
    private readonly string[] hiddenElementsNames = { };
    private readonly string[] hiderBoolPropertyNames = { };

    private PropertyField[] hiderBools; // maybe these will just be locals, idk
    private VisualElement[] hiddenElements;
    private SerializedProperty[] hiderBoolProperties;
    
    [SerializeField] private VisualTreeAsset UsedVisualTree;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Add in the stuff from ui builder
        VisualElement container = new();
        UsedVisualTree.CloneTree(container);

        // Setup events for bools that hide elements when off
        HiderBoolEvents(property, container);
        
        // Return the finished ui
        return container;
    }

    private void HiderBoolEvents(SerializedProperty property, VisualElement container)
    {
        hiderBoolProperties = new SerializedProperty[hiderBoolPropertyNames.Length];
        hiderBools = new PropertyField[hiderBoolNames.Length];
        hiddenElements = new VisualElement[hiddenElementsNames.Length];

        for (int i = 0; i < hiderBoolNames.Length; i++)
        {
            hiderBoolProperties[i] = property.serializedObject.FindProperty(hiderBoolPropertyNames[i]);
            hiderBools[i] = container.Q<PropertyField>(hiderBoolNames[i]);
            hiddenElements[i] = container.Q<VisualElement>(hiddenElementsNames[i]);

            // Set up events for when a toggle is changed
            hiderBools[i].RegisterCallback<ChangeEvent<bool>>(BoolChanged);
        }
    }

    private void BoolChanged(ChangeEvent<bool> changeEvent)
    {
        // Update all hideable elements' visibility
        for (int i = 0; i < hiderBoolNames.Length; i++)
        {
            ChangeElementVisibility(hiddenElements[i], hiderBoolProperties[i].boolValue);
        }
    }

    private void ChangeElementVisibility(VisualElement visualElement, bool showElement)
    {
        if (showElement)
        {
            visualElement.style.display = DisplayStyle.Flex;
        }
        else
        {
            visualElement.style.display = DisplayStyle.None;
        }
    }
}