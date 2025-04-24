using UnityEngine;

public class WolfSpawner : MonoBehaviour
{
    public GameObject wolfPrefab;
    public int initialWolfCount = 2;  // Initial number of wolves to spawn
    public float spawnAreaRadius = 10f;

    public AudioClip wolfSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SpawnInitialWolves();
    }

    private void SpawnInitialWolves()
    {
        for (int i = 0; i < initialWolfCount; i++)
        {
            SpawnWolf();
        }
    }

    public void SpawnWolf()
    {
        Vector2 randomPosition = new Vector2(
            transform.position.x + Random.Range(-spawnAreaRadius, spawnAreaRadius),
            transform.position.y + Random.Range(-spawnAreaRadius, spawnAreaRadius)
        );

        // Instantiate wolf prefab at the random position
        GameObject newWolf = Instantiate(wolfPrefab, randomPosition, Quaternion.identity);

        // Optionally, you can set a random hungry level for the newly spawned wolf
        newWolf.GetComponent<Wolf>().hungryLevel = Random.Range(90, 101);
    }

    // Spawn a wolf at a specific position, typically called after a wolf eats a sheep
    public void SpawnWolfAtPosition(Vector2 spawnPosition)
    {
        // Instantiate wolf prefab at the specified position
        GameObject newWolf = Instantiate(wolfPrefab, spawnPosition, Quaternion.identity);

        // Optionally, you can set a random hungry level for the newly spawned wolf
        newWolf.GetComponent<Wolf>().hungryLevel = Random.Range(90, 101);
        
        PlayWolfSound();
        Debug.Log("New wolf spawned at " + spawnPosition);
    }

    private void PlayWolfSound()
    {
        if (audioSource != null && wolfSound != null)
        {
            audioSource.PlayOneShot(wolfSound);
        }
    }
}
