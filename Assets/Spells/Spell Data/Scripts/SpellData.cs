using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Scriptable Spell")]
public class SpellData : ScriptableObject
{
    // Spell Info
    public float ManaCost;
    public float SpellCooldown;
    public Sprite Icon;

    public SpellModule[] UsedModules;

    // Value type that can be used to get a spell (so that it can be sent through networking)
    [System.Serializable]
    public struct SpellInfo : INetworkSerializeByMemcpy
    {
        // Constructor
        public SpellInfo(byte setIndex, byte spellIndex)
        {
            SetIndex = setIndex;
            SpellIndex = spellIndex;
        }

        // Fields
        public byte SetIndex;
        public byte SpellIndex;

        // Properties
        public readonly SpellData Spell
        {
            get
            {
                var set = GameSettings.Used.SpellSets[SetIndex];
                return set.spellsInSet[SpellIndex];
            }
        }
    }
}