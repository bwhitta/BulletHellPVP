using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SpellData))]
public class SpellEditor : Editor
{
    private bool openFoldout;

    private readonly int spaceWidth = 25;

    private readonly string[] SpellUsesMovementChildren = new[] { "SpawningArea", "TargetingType", "MovementType", "MovementSpeed" };
    private readonly string[] SpellDealsDamageChildren = new[] { "Damage" };
    private readonly string[] SpellUsesColliderChildren = new[] { "ColliderPath" };
    private readonly string[] SpellUsesSpriteChildren = new[] { "SpellSprite", "SpriteScale" };
    private readonly string[] AnimatedSpellChildren = new[] { "SpellAnimatorController", "MultipartAnimationPrefabs" };
    private readonly string[] SpellScalesChildren = new[] { "ScalingStartPercent", "MaxScaleMultiplier", "DestroyOnScalingCompleted" };
    public override void OnInspectorGUI()
    {
        FoldoutHeader();

        EditorGUILayout.Space(spaceWidth);

        DisplayField("Icon");
        DisplayField("Description");


        EditorGUILayout.Space(spaceWidth);
        CustomToggle("SpellUsesMovement", SpellUsesMovementChildren);

        EditorGUILayout.Space(spaceWidth);
        CustomToggle("SpellDealsDamage", SpellDealsDamageChildren);

        EditorGUILayout.Space(spaceWidth);
        CustomToggle("SpellUsesCollider", SpellUsesColliderChildren);

        EditorGUILayout.Space(spaceWidth);
        CustomToggle("SpellUsesSprite", SpellUsesSpriteChildren);

        EditorGUILayout.Space(spaceWidth);
        CustomToggle("AnimatedSpell", AnimatedSpellChildren);

        EditorGUILayout.Space(spaceWidth);
        CustomToggle("SpellScales", SpellScalesChildren);


        serializedObject.ApplyModifiedProperties();
    }

    private void FoldoutHeader()
    {
        openFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(openFoldout, "Basic Spell Info");
        if (openFoldout)
        {
            EditorGUI.indentLevel = 1;
            DisplayField("SpellName");
            DisplayField("ManaCost");
            DisplayField("SpellCooldown");
            DisplayField("InstantiationQuantity");
            EditorGUI.indentLevel = 0;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void CustomToggle (string togglePropertyString, string[] fields)
    {
        EditorGUI.BeginChangeCheck();
        EditorStyles.label.fontStyle = FontStyle.Bold;

        EditorGUI.indentLevel = -1;
        SerializedProperty property = serializedObject.FindProperty(togglePropertyString);
        property.boolValue = EditorGUILayout.ToggleLeft(togglePropertyString , property.boolValue); //(togglePropertyString, property.boolValue
        EditorGUI.indentLevel = 0;

        EditorStyles.label.fontStyle = FontStyle.Normal;
        EditorGUI.EndChangeCheck();
        
        if(property.boolValue == true)
        {
            EditorGUI.indentLevel = 1;
            foreach (string field in fields)
            {
                DisplayField(field);
            }
            EditorGUI.indentLevel = 0;
        }
    }
    private SerializedProperty CreateCategoryToggle(string propertyPath)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyPath);
        EditorGUILayout.PropertyField(property);
        return property;
    }
    private void DisplayField(string Property)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty(Property));
    }
}