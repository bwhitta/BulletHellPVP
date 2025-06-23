using UnityEngine;

public class SpawnInputDetection : MonoBehaviour
{
    private PredictiveSpawnerExample predictiveSpawner;
    private void Start()
    {
        predictiveSpawner = GetComponent<PredictiveSpawnerExample>();
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Mouse.current.backButton.wasPressedThisFrame)
        {
            predictiveSpawner.SpawnObject(new Vector3(Random.Range(-5, 5), Random.Range(-5, 5)));
        }
    }
}
