using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static EditorUtilities;

[CustomPropertyDrawer(typeof(SpellModule), true)]
public class SpellModuleEditor : PropertyDrawer
{
    private readonly HiderBoolInfo[] hiderBoolInfos = 
    {
        new() { BoolFieldName = "PlayerAttached", BoolPropertyName = "PlayerAttached", HiddenElementName = "PlayerAttachedHides" },
        new() { BoolFieldName = "LimitedLifespan", BoolPropertyName = "LimitedLifespan", HiddenElementName = "LimitedLifespanHides" },
        new() { BoolFieldName = "DestroyAfterDistanceMoved", BoolPropertyName = "DestroyAfterDistanceMoved", HiddenElementName = "DestroyAfterDistanceMovedHides" },
        new() { BoolFieldName = "UsesCollider", BoolPropertyName = "UsesCollider", HiddenElementName = "UsesColliderHides" },
        new() { BoolFieldName = "DealsDamage", BoolPropertyName = "DealsDamage", HiddenElementName = "DealsDamageHides" },
        new() { BoolFieldName = "SpellUsesSprite", BoolPropertyName = "SpellUsesSprite", HiddenElementName = "SpellUsesSpriteHides" },
        new() { BoolFieldName = "UsesAnimation", BoolPropertyName = "UsesAnimation", HiddenElementName = "UsesAnimationHides" },
        new() { BoolFieldName = "GeneratesParticles", BoolPropertyName = "GeneratesParticles", HiddenElementName = "GeneratesParticlesHides" }
    };

    private readonly SwitcherEnumInfo[] switcherEnumInfos =
    {
        //new() { EnumFieldName = "ModuleType", EnumPropertyName = "ModuleType", HiddenElementNames = new string[]{"TypeProjectile", "TypePlayerAttached"} }
    };

    [SerializeField] private VisualTreeAsset UsedVisualTree;
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Add in the stuff from ui builder
        VisualElement container = new();
        UsedVisualTree.CloneTree(container);

        // Setup events for bools that hide elements when false
        HiderBoolEvents(property, container);
        SwitcherEnumEvents(property, container);

        // Return the finished ui
        return container;
    }
    
    private void HiderBoolEvents(SerializedProperty property, VisualElement container)
    {
        foreach (HiderBoolInfo hiderBoolInfo in hiderBoolInfos)
        {
            HiderBool hiderBool = new();

            // Find things
            hiderBool.SetAllFields(container, property, hiderBoolInfo);

            // Create event
            hiderBool.BoolField.RegisterCallback<ChangeEvent<bool>, HiderBool>(OnBoolChanged, hiderBool);
        }
    }
    private void OnBoolChanged(ChangeEvent<bool> changeEvent, HiderBool hiderBool)
    {
        hiderBool.UpdateVisibility();
    }

    private void SwitcherEnumEvents(SerializedProperty property, VisualElement container)
    {
        foreach (SwitcherEnumInfo switcherEnumInfo in switcherEnumInfos)
        {
            SwitcherEnum switcherEnum = new();

            // Find things
            switcherEnum.SetAllFields(container, property, switcherEnumInfo);

            // Create event
            switcherEnum.EnumField.RegisterCallback<ChangeEvent<string>, SwitcherEnum>(OnEnumChanged, switcherEnum);
        }
    }
    private void OnEnumChanged(ChangeEvent<string> changeEvent, SwitcherEnum switcherEnum)
    {
        switcherEnum.UpdateVisibility();
    }
}