using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class EditorUtilities
{
    public class HiderBoolInfo
    {
        public string BoolFieldName;
        public string BoolPropertyName;
        public string HiddenElementName;
    }
    public class HiderBool
    {
        public PropertyField BoolField;
        public SerializedProperty BoolProperty;
        public VisualElement HiddenElement;
        
        public void SetAllFields(VisualElement container, SerializedProperty relativeProperty, HiderBoolInfo hiderBoolInfo)
        {
            SetAllFields(container, relativeProperty, hiderBoolInfo.BoolFieldName, hiderBoolInfo.BoolPropertyName, hiderBoolInfo.HiddenElementName);
        }
        public void SetAllFields(VisualElement container, SerializedProperty relativeProperty, string boolFieldName, string boolPropertyName, string hiddenElementName)
        {
            BoolField = container.Q<PropertyField>(boolFieldName);
            HiddenElement = container.Q<VisualElement>(hiddenElementName);
            BoolProperty = relativeProperty.FindPropertyRelative(boolPropertyName);

            // Error Handling
            if (BoolField == null || HiddenElement == null || BoolProperty == null) Debug.LogError("One or more HiderBool fields is null");
            if (BoolProperty.type != "bool") Debug.LogError($"property {BoolProperty} should be a bool but is instead type '{BoolProperty.type}'");
        }
        public void UpdateVisibility()
        {
            ChangeElementVisibility(HiddenElement, BoolProperty.boolValue);
        }
    } 
    
    public class SwitcherEnumInfo
    {
        public string EnumFieldName;
        public string EnumPropertyName;
        public string[] HiddenElementNames;
    }
    public class SwitcherEnum
    {
        public PropertyField EnumField;
        public SerializedProperty EnumProperty;
        public VisualElement[] HiddenElements;
        
        public void SetAllFields(VisualElement container, SerializedProperty relativeProperty, SwitcherEnumInfo switcherEnumInfo)
        {
            SetAllFields(container, relativeProperty, switcherEnumInfo.EnumFieldName, switcherEnumInfo.EnumPropertyName, switcherEnumInfo.HiddenElementNames);
        }
        public void SetAllFields(VisualElement container, SerializedProperty relativeProperty, string enumFieldName, string enumPropertyName, string[] hiddenElementNames)
        {
            EnumField = container.Q<PropertyField>(enumFieldName);
            EnumProperty = relativeProperty.FindPropertyRelative(enumPropertyName);
            
            HiddenElements = new VisualElement[hiddenElementNames.Length];
            for (int i = 0; i < hiddenElementNames.Length; i++)
            {
                HiddenElements[i] = container.Q<VisualElement>(hiddenElementNames[i]);

                //Error handling
                if (HiddenElements[i] == null) Debug.LogError($"VisualElement with name '{hiddenElementNames}' wasn't found");
            }

            // Error Handling
            if (EnumField == null || HiddenElements == null || EnumProperty == null) Debug.LogError("One or more EnumSwitcher fields is null");
            if (EnumProperty.type != "Enum") Debug.LogError($"property {EnumProperty} should be an Enum but is instead type '{EnumProperty.type}'");
        }
        public void UpdateVisibility()
        {
            int enumIndex = EnumProperty.enumValueIndex;
            
            for (int i = 0; i < HiddenElements.Length; i++)
            {
                VisualElement visualElement = HiddenElements[i];
                bool visible = enumIndex == i;

                ChangeElementVisibility(visualElement, visible);
            }
        }
    }

    /// <summary>
    /// Changes the visibility of a VisualElement
    /// </summary>
    public static void ChangeElementVisibility(VisualElement visualElement, bool showElement)
    {
        //Debug.Log($"Updating visibility of {visualElement.name}");
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
