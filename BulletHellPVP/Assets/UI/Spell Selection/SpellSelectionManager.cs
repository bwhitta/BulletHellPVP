using UnityEngine;

public class SpellSelectionManager : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private GameObject displayPrefab;
    [SerializeField] private Vector2 distanceBetweenIcons;
    [SerializeField] private int columnsOfIcons;

    public void CreateSpellObjects(SpellSetInfo selectedSet)
    {
        // Destroy all of the old child objects
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < selectedSet.spellsInSet.Length; i++)
        {
            // Instaniates the prefab
            GameObject childObject = Instantiate(displayPrefab, transform);

            float x = i % 5;
            float y = Mathf.Floor(i / 5);

            Vector3 displacement = new(x * distanceBetweenIcons.x, y * -distanceBetweenIcons.y, 0);
            childObject.transform.position = transform.position + displacement;

            //Set the object's sprite
            childObject.GetComponent<SpriteRenderer>().sprite = selectedSet.spellsInSet[i].Icon;
        }
    }
}
