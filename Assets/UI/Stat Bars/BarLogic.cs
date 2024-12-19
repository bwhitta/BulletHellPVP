using UnityEngine;
using UnityEngine.UI;

// 249, 264 (margins around image)

public class BarLogic : MonoBehaviour
{
    [SerializeField] private GameObject baseObject, statValueObject, vanishingLossesObject;
    [SerializeField] private Text valueTextObject;
    [HideInInspector] public VisualSettings.BarColors BarColors;

    private float vanishingLossesValue;
    private float vanishingLossesVelocity = 0;

    private float _statValue;
    public float StatValue
    {
        get
        {
            return _statValue;
        }
        set
        {
            UpdateStat();
            _statValue = value;
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
            UpdateStat();
            _statMax = value;
        }
    }

    private void Start()
    {
        baseObject.GetComponent<Image>().color = BarColors.BaseColor;
        statValueObject.GetComponent<Image>().color = BarColors.ValueColor;
        vanishingLossesObject.GetComponent<Image>().color = BarColors.LossesColor;
    }

    private void Update()
    {
        UpdateVanishingLosses();
    }
    private void UpdateStat()
    {
        // Update the text
        valueTextObject.text = Mathf.Floor(StatValue).ToString();

        float statPercentage = StatValue / StatMax;
        statValueObject.GetComponent<Image>().fillAmount = statPercentage;
        
        /*// Divider bar
        float minimumDivPos = (-displayImage.preferredWidth / 2) + edgeLeft;
        float divPos = minimumDivPos + ((displayImage.preferredWidth - (edgeLeft + edgeRight)) * statPercentage);
        GameObject div = displayImage.transform.GetChild(0).gameObject;
        div.GetComponent<RectTransform>().anchoredPosition = new Vector2(divPos, 0);*/
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
        // Update the text
        valueTextObject.text = Mathf.Floor(StatValue).ToString();

        float lossesPercentage = vanishingLossesValue / StatMax;
        vanishingLossesObject.GetComponent<Image>().fillAmount = lossesPercentage;
    }
}