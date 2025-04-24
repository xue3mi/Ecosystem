using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    public GameObject sheepPrefab;
    public int initialSheepCount = 3; // Initial number of sheep to spawn
    public float spawnAreaRadius = 10f; // Area radius where sheep will spawn

    public AudioClip sheepSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SpawnInitialSheep();
    }

    // Spawn the initial number of sheep at random positions
    private void SpawnInitialSheep()
    {
        for (int i = 0; i < initialSheepCount; i++)
        {
            SpawnSheep();
        }
    }

    // Spawn a sheep at a random position within the spawn area
    public void SpawnSheep()
    {
        Vector2 randomPosition = new Vector2(
            transform.position.x + Random.Range(-spawnAreaRadius, spawnAreaRadius),
            transform.position.y + Random.Range(-spawnAreaRadius, spawnAreaRadius)
        );

        // Instantiate sheep prefab at the random position
        GameObject newSheep = Instantiate(sheepPrefab, randomPosition, Quaternion.identity);

        // Set a random hungry level between 80 and 100
        int randomHungryLevel = Random.Range(80, 101); // Random value between 80 and 100
        newSheep.GetComponent<Sheep>().InitializeSheep(randomHungryLevel);  // Set the random hungry level for the new sheep
    }

    // Spawn a sheep at a specific position, called after a sheep eats food
    public void SpawnSheepAtPosition(Vector2 spawnPosition)
    {
        // Instantiate sheep prefab at the specified position
        GameObject newSheep = Instantiate(sheepPrefab, spawnPosition, Quaternion.identity);

        // Set a random hungry level between 80 and 100
        int randomHungryLevel = Random.Range(80, 101);
        newSheep.GetComponent<Sheep>().InitializeSheep(randomHungryLevel);  // Set the random hungry level for the new sheep
        
        PlaySheepSound();
        Debug.Log("New sheep spawned at " + spawnPosition + " with hungry level: " + randomHungryLevel);
    }

    private void PlaySheepSound()
    {
        if (audioSource != null && sheepSound != null)
        {
            audioSource.PlayOneShot(sheepSound);
        }
    }
}
