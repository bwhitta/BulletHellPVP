using UnityEngine;

[CreateAssetMenu(menuName = "Visual Settings")]
public class VisualSettings : ScriptableObject
{
    [Header("Color")]
    public BarColors HealthBarColors;
    public BarColors ManaBarColors;

    [System.Serializable]
    public class BarColors
    {
        public Color BaseColor;
        public Color ValueColor;
        public Color LossesColor;
    }

    
}
