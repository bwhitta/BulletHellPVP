using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpellData))]
public class SpellEditor : Editor
{
    private readonly float spaceWidth = 12.5f;

    // Variables by group they are displayed in
    private readonly string[] SpellInfoGroup = new[]
    {
        "ManaCost",
        "SpellCooldown",
        "Icon",
    };
    #region Projectile
    private readonly string[] ProjectileGroup = new[]
    {
        "ProjectileSpawningArea",
        "TargetingType",
        "MovementType",
        "MovementSpeed"
    };
    private readonly string[] AbilityDealsDamageGroup = new[]
    {
        "Damage"
    };
    #endregion
    #region PlayerAttached
    private readonly string[] PlayerAttachedGroup = new[]
    {
        "AttachmentTime"
    };
    private readonly string[] PushesPlayerGroup = new[]
    {
        "PlayerPushSpeed",
        "SpriteFacingPush"
    };
    private readonly string[] AngleAfterStartGroup = new[]
    {
        "AngleChangeSpeed"
    };
    private readonly string[] AffectsPlayerMovementGroup = new[]
    {
        "PlayerMovementMod"
    };
    #endregion
    private readonly string[] UsesColliderGroup = new[]
    {
        "ColliderPath"
    };
    private readonly string[] UsesSpriteGroup = new[]
    {
        "UsedSprite"
    };
    private readonly string[] AnimatedGroup = new[]
    {
        "AnimationScaleMultiplier",
        "AnimatorController",
        "MultipartAnimationPrefabs"
    };
    private readonly string[] ScalesOverTimeGroup = new[]
    {
        "ScalingStartPercent",
        "MaxScaleMultiplier",
        "DestroyOnScalingCompleted"
    };
    private readonly string[] GeneratesParticlesGroup = new[]
    {
        "ParticleSystemPrefab",
        "ParticleSystemZ"
    };

    SerializedProperty moduleQuantity, usedModules;

    private void OnEnable()
    {
        usedModules = serializedObject.FindProperty("UsedModules");
    }
    
    private void DisplayModules(SerializedProperty module)
    {
        // Module Type
        switch ((SpellData.ModuleTypes)DisplayEnumFromModule(module, "ModuleType"))
        {
            case SpellData.ModuleTypes.Projectile:
                DisplayGroupFromModule(module, ProjectileGroup);

                if (DisplayToggleFromModule(module, "AbilityDealsDamage", 1))
                {
                    DisplayGroupFromModule(module, AbilityDealsDamageGroup, 2);
                }
                break;
            case SpellData.ModuleTypes.PlayerAttached:
                /* DisplayGroupFromModule(module, PlayerAttachedGroup);

                if (DisplayToggleFromModule(module, "PushesPlayer", 1))
                {
                    DisplayGroupFromModule(module, PushesPlayerGroup, 2);

                    if(DisplayToggleFromModule(module, "AngleAfterStart", 2))
                    {
                        DisplayGroupFromModule(module, AngleAfterStartGroup, 3);
                    }
                }
                if (DisplayToggleFromModule(module, "AffectsPlayerMovement", 1))
                {
                    DisplayGroupFromModule(module, AffectsPlayerMovementGroup, 2);
                } DISABLED FOR RESTRUCTURING */
                break;
        }

        EditorGUILayout.Space(spaceWidth / 2);

        // Instantiation Quantity
        DisplayFieldFromModule(module, "InstantiationQuantity");

        // Instantiation Scale
        DisplayFieldFromModule(module, "InstantiationScale");
        EditorGUILayout.Space(spaceWidth / 2);

        // Uses Collider
        if (DisplayToggleFromModule(module, "UsesCollider"))
        {
            DisplayGroupFromModule(module, UsesColliderGroup);
        }
        EditorGUILayout.Space(spaceWidth / 2);

        // Uses Sprite
        if (DisplayToggleFromModule(module, "UsesSprite"))
        {
            DisplayGroupFromModule(module, UsesSpriteGroup);
        }
        EditorGUILayout.Space(spaceWidth / 2);

        // Animated
        if (DisplayToggleFromModule(module, "Animated"))
        {
            DisplayGroupFromModule(module, AnimatedGroup);
        }
        EditorGUILayout.Space(spaceWidth / 2);

        if (DisplayToggleFromModule(module, "GeneratesParticles"))
        {
            DisplayGroupFromModule(module, GeneratesParticlesGroup);
        }
        EditorGUILayout.Space(spaceWidth / 2);

        // ScalesOverTime
        if (DisplayToggleFromModule(module, "ScalesOverTime"))
        {
            DisplayGroupFromModule(module, ScalesOverTimeGroup);
        }
    }

    public override void OnInspectorGUI()
    {
        moduleQuantity = serializedObject.FindProperty("ModuleQuantity");

        //Display spell info
        EditorGUILayout.LabelField("Spell Info");
        DisplayBaseGroup(SpellInfoGroup);
        EditorGUILayout.Space(spaceWidth * 2);

        // Display modules
        Modules();

        serializedObject.ApplyModifiedProperties();
    }

    private readonly Dictionary<SerializedProperty, SerializedProperty> foldoutOpenProperties = new();
    private void Modules()
    {
        // Set up modules
        ModuleQuantitySelection();
        ShowModules();
        
        void ModuleQuantitySelection()
        {
            EditorGUILayout.BeginHorizontal();
            BeginBoldArea();
            
            EditorGUILayout.LabelField("Number of modules: ", GUILayout.Width(125));
            moduleQuantity.intValue = Mathf.Clamp(EditorGUILayout.IntField(moduleQuantity.intValue), 0, 100);

            EndBoldArea();
            EditorGUILayout.EndHorizontal();

            usedModules.arraySize = moduleQuantity.intValue;
        }
        
        void ShowModules()
        {
            for (int i = 0; i < usedModules.arraySize; i++)
            {
                SerializedProperty module = usedModules.GetArrayElementAtIndex(i); 
                
                EditorGUILayout.Space(spaceWidth);
                if (ModuleFoldout(module, i) == false)
                    continue;

                EditorGUI.indentLevel = 1;
                DisplayModules(module);
                EditorGUILayout.Space(spaceWidth);
                EditorGUI.indentLevel = 0;
            }
        }
        bool ModuleFoldout(SerializedProperty module, int index)
        {
            if (foldoutOpenProperties.ContainsKey(module) == false)
            {
                foldoutOpenProperties.Add(module, module.FindPropertyRelative("FoldoutOpen"));
            }
            foldoutOpenProperties[module].boolValue = EditorGUILayout.Foldout(foldoutOpenProperties[module].boolValue, $"Module {index}");

            return foldoutOpenProperties[module].boolValue;
        }
    }

    // Display from a module:
    private bool DisplayToggleFromModule(SerializedProperty module, string propertyName, int indent = 0)
    {
        CheckAllModules(module, propertyName);

        EditorGUI.indentLevel += indent;
        BeginBoldArea();
        EditorGUILayout.PropertyField(allModules[module][propertyName]);
        EndBoldArea();
        EditorGUI.indentLevel -= indent;

        return allModules[module][propertyName].boolValue;
    }
    private int DisplayEnumFromModule(SerializedProperty module, string propertyName)
    {
        CheckAllModules(module, propertyName);

        BeginBoldArea();
        EditorGUILayout.PropertyField(allModules[module][propertyName]);
        EndBoldArea();

        return allModules[module][propertyName].enumValueIndex;
    }
    private void DisplayFieldFromModule(SerializedProperty module, string propertyName, int extraArrayIndent = 1)
    {
        CheckAllModules(module, propertyName);

        int indent = 0;
        if (allModules[module][propertyName].isArray)
            indent = extraArrayIndent;

        EditorGUI.indentLevel += indent;
        EditorGUILayout.PropertyField(allModules[module][propertyName]);
        EditorGUI.indentLevel -= indent;
    }
    private void DisplayGroupFromModule(SerializedProperty module, string[] moduleGroup, int indents = 1)
    {
        EditorGUI.indentLevel+=indents;
        foreach (string propertyName in moduleGroup)
        {
            DisplayFieldFromModule(module, propertyName);
        }
        EditorGUI.indentLevel-=indents;
    }

    // Store modules and their properties:
    private readonly Dictionary<SerializedProperty, Dictionary<string, SerializedProperty>> allModules = new();
    private void CheckAllModules(SerializedProperty module, string propertyName)
    {
        if (allModules.ContainsKey(module) == false)
        {
            allModules.Add(module, new Dictionary<string, SerializedProperty>());
        }
        if (allModules[module].ContainsKey(propertyName) == false)
        {
            allModules[module].Add(propertyName, module.FindPropertyRelative(propertyName));
        }
    }

    // Display from base:
    private void DisplayBaseGroup(string[] fields)
    {
        EditorGUI.indentLevel = 1;
        foreach (string field in fields)
        {
            DisplayBaseField(field);
        }
        EditorGUI.indentLevel = 0;
    }
    private void DisplayBaseField(string property)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty(property));
    }

    // Display options
    private void BeginBoldArea()
    {
        EditorGUI.BeginChangeCheck();
        EditorStyles.label.fontStyle = FontStyle.Bold;
    }
    private void EndBoldArea()
    {
        EditorStyles.label.fontStyle = FontStyle.Normal;
        EditorGUI.EndChangeCheck();
    }
    
    /* UNUSED METHODS - DO NOT ERASE
    private void CustomDropdown(string dropdownVariableName, Dictionary<string, string[]> variablesToDisplay)
    {
        SerializedProperty property = serializedObject.FindProperty(dropdownVariableName);
        property.enumValueIndex = EditorGUILayout.Popup(property.enumValueIndex, property.enumNames);

        DisplayBaseGroup(variablesToDisplay[property.enumNames[property.enumValueIndex]]);
    }

    private void CustomToggle (string toggleVariableName, string[] showOnChecked, string[] showOnUnchecked = null)
    {
        // Start formatting
        EditorGUI.BeginChangeCheck();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUI.indentLevel = -1;
        
        // Create the toggle
        SerializedProperty property = serializedObject.FindProperty(toggleVariableName);
        property.boolValue = EditorGUILayout.ToggleLeft(toggleVariableName , property.boolValue);
        
        // End formatting
        EditorGUI.indentLevel = 0;
        EditorStyles.label.fontStyle = FontStyle.Normal;
        EditorGUI.EndChangeCheck();
        

        if(property.boolValue == true)
        {
            DisplayBaseGroup(showOnChecked);
        }
        else if (showOnUnchecked != null)
        {
            DisplayBaseGroup(showOnUnchecked);
        }

        EditorGUILayout.Space(spaceWidth);
    }*/
        }