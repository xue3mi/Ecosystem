using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public float minSpawnTime = 3f;
    public float maxSpawnTime = 8f;
    public int maxFoodCount = 10;
    public int initialFoodCount = 3;

    void Start()
    {
        for (int i = 0; i < initialFoodCount; i++)
        {
            SpawnFood();
        }

        StartSpawning();
    }

    void StartSpawning()
    {
        float randomTime = Random.Range(minSpawnTime, maxSpawnTime);
        Invoke(nameof(SpawnFood), randomTime);
    }

    void SpawnFood()
    {
        int currentFoodCount = GameObject.FindGameObjectsWithTag("Fruit").Length;

        if (currentFoodCount < maxFoodCount)
        {
            Camera cam = Camera.main;
            float camHeight = cam.orthographicSize;
            float camWidth = cam.orthographicSize * cam.aspect;

            //only spawn inside camera
            float randomX = Random.Range(cam.transform.position.x - camWidth, cam.transform.position.x + camWidth);
            float randomY = Random.Range(cam.transform.position.y - camHeight, cam.transform.position.y + camHeight);
            Vector2 spawnPosition = new Vector2(randomX, randomY);

            Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
        }

        //continue spawn
        StartSpawning();
    }
}
