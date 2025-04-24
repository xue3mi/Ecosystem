using UnityEngine;

public class LionSpawner : MonoBehaviour
{
    public GameObject lionPrefab;
    public int initialLionCount = 1;
    public float spawnAreaRadius = 10f;

    public AudioClip lionSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SpawnInitialLions();
    }

    private void SpawnInitialLions()
    {
        for (int i = 0; i < initialLionCount; i++)
        {
            SpawnLionAtPosition(new Vector2(Random.Range(-spawnAreaRadius, spawnAreaRadius), Random.Range(-spawnAreaRadius, spawnAreaRadius)));
        }
    }

    public void SpawnLionAtPosition(Vector2 spawnPosition)
    {
        Instantiate(lionPrefab, spawnPosition, Quaternion.identity);
        PlayLionSound();
        Debug.Log("New lion spawned at " + spawnPosition);
    }

    private void PlayLionSound()
    {
        if (audioSource != null && lionSound != null)
        {
            audioSource.PlayOneShot(lionSound);
        }
    }
}
