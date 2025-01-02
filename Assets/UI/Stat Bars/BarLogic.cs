using UnityEngine;
using UnityEngine.UI;

public class BarLogic : MonoBehaviour
{
    // Fields
    [SerializeField] private Text valueTextObject;
    [SerializeField] private GameObject vanishingLossesObject;
    private float vanishingLossesValue;
    private float vanishingLossesVelocity = 0;
    [SerializeField] private GameObject statValueObject;
    private Image _statValueImage;
    private Image StatValueImage
    {
        get
        {
            _statValueImage = _statValueImage != null ? _statValueImage : statValueObject.GetComponent<Image>();
            return _statValueImage;
        }
        set => _statValueImage = value;
    }

    [SerializeField] private RectTransform divider;
    [SerializeField] private float dividerRange;

    // Properties
    private float _statValue;
    public float StatValue
    {
        get
        {
            return _statValue;
        }
        set
        {
            _statValue = Mathf.Clamp(value, 0f, StatMax);
            UpdateStat();
        }
    }
    private float _statMax;
    public float StatMax
    {
        get
        {
            return _statMax;
        }
        set
        {
            _statMax = Mathf.Max(value, 0f);
            UpdateStat();
        }
    }

    // Methods
    private void Update()
    {
        StatValue = _statValue; // DELETE

        UpdateVanishingLosses();
    }
    private void UpdateStat()
    {
        valueTextObject.text = Mathf.Floor(StatValue).ToString();
        float statPercentage = StatValue / StatMax;
        StatValueImage.fillAmount = statPercentage;

        // Update the divider
        Vector3 adjustedPosition = divider.anchoredPosition;
        adjustedPosition.x = Calculations.RelativeTo(-dividerRange / 2, dividerRange / 2, statPercentage);
        divider.anchoredPosition = adjustedPosition;
    }
    private void UpdateVanishingLosses()
    {
        if (vanishingLossesValue > StatValue)
        {
            // Move statLost down and speed up velocity
            vanishingLossesValue -= vanishingLossesVelocity * Time.deltaTime;
            vanishingLossesVelocity += GameSettings.Used.StatLostVelocity;
        }
        if (vanishingLossesValue < StatValue)
        {
            // Floor at actual stat and stop the movement
            vanishingLossesValue = StatValue; 
            vanishingLossesVelocity = 0;
        }
        
        valueTextObject.text = Mathf.Floor(StatValue).ToString();
        float lossesPercentage = vanishingLossesValue / StatMax;
        vanishingLossesObject.GetComponent<Image>().fillAmount = lossesPercentage;
    }
}