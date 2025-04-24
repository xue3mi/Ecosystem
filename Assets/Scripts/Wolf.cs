using UnityEngine;

public class Wolf : MonoBehaviour
{
    private Rigidbody2D rb;

    public float speed = 3f;
    public float changeDirectionTime = 5f;
    private Vector2 targetPosition;
    private float timer;
    private float idleTimer;
    private float eatingTime = 3f;
    private float eatingTimer = 0f;

    public Sprite stand;
    public Sprite walk;
    public Sprite eat;
    public Sprite dead;

    private WolfState currentState = WolfState.Walking;
    public int hungryLevel;
    private float hungerTimer = 0f;
    public float hungerDecreaseInterval = 4f;

    public float lifespan = 5f;  // Lifespan in seconds
    private float lifespanTimer = 0f;  // Timer to track the lifespan

    public enum WolfState
    {
        Idle,
        Walking,
        Eating,
        FindSheep,  // New state for finding sheep
        Dead
    }

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        hungryLevel = Random.Range(80, 101);
        SetNewTargetPosition();
        idleTimer = Random.Range(5f, 10f);
    }

    void Update()
    {
        UpdateState();
        DecreaseHungerOverTime();

        // If the wolf is hungry and not in a dead state, start looking for a sheep
        if (hungryLevel < 45 && currentState != WolfState.Dead && currentState != WolfState.Eating)
        {
            StartState(WolfState.FindSheep);
        }

        // Update lifespan timer
        lifespanTimer += Time.deltaTime;

        // If the wolf's lifespan is over, set it to dead
        if (lifespanTimer >= lifespan)
        {
            StartState(WolfState.Dead);
        }
    }

    void SetNewTargetPosition()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        float randomX = Random.Range(cam.transform.position.x - camWidth, cam.transform.position.x + camWidth);
        float randomY = Random.Range(cam.transform.position.y - camHeight, cam.transform.position.y + camHeight);
        targetPosition = new Vector2(randomX, randomY);
    }

    public void StartState(WolfState newState)
    {
        EndState(currentState);

        switch (newState)
        {
            case WolfState.Idle:
                ChangeSprite(stand);
                break;
            case WolfState.Walking:
                ChangeSprite(walk);
                break;
            case WolfState.Eating:
                ChangeSprite(eat);
                Debug.Log("Wolf is eating");
                break;
            case WolfState.Dead:
                ChangeSprite(dead);
                Debug.Log("Wolf has died.");
                Invoke(nameof(DestroyWolf), 5f);
                break;
            case WolfState.FindSheep: // New state for finding sheep
                FindSheep();
                break;
        }
        currentState = newState;
    }

    public void UpdateState()
    {
        switch (currentState)
        {
            case WolfState.Idle:
                Idle();
                break;
            case WolfState.Walking:
                Move();
                break;
            case WolfState.Eating:
                Eating();
                break;
            case WolfState.FindSheep: // Handling the state of finding sheep
                FindSheep();
                break;
            case WolfState.Dead:
                break;
        }
    }

    private void EndState(WolfState oldState)
    {
        switch (oldState)
        {
            case WolfState.Idle:
                break;
            case WolfState.Walking:
                break;
            case WolfState.Eating:
                break;
            case WolfState.Dead:
                break;
        }
    }

    private void Idle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            StartState(WolfState.Walking);
            idleTimer = Random.Range(5f, 10f);
        }
    }

    private void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        timer += Time.deltaTime;
        if (timer > changeDirectionTime)
        {
            SetNewTargetPosition();
            timer = 0f;
        }
    }

    private void Eating()
    {
        eatingTimer += Time.deltaTime;

        if (eatingTimer >= eatingTime)
        {
            eatingTimer = 0f;
            StartState(WolfState.Idle);
            idleTimer = Random.Range(1f, 5f);
        }
    }

    private void Eating(GameObject sheep)
    {
        if (hungryLevel < 45)  // Only eat when hungry
        {
            if (sheep == null) return;
            Destroy(sheep);
            hungryLevel = Mathf.Min(100, hungryLevel + 50);
            StartState(WolfState.Eating);
            Invoke(nameof(CompleteEating), eatingTime);
        }
    }

    private void CompleteEating()
    {
        StartState(WolfState.Idle);
        idleTimer = Random.Range(1f, 3f);
        GameObject wolfSpawner = GameObject.FindObjectOfType<WolfSpawner>().gameObject;
        wolfSpawner.GetComponent<WolfSpawner>().SpawnWolfAtPosition(transform.position);
    }

    private void DecreaseHungerOverTime()
    {
        hungerTimer += Time.deltaTime;

        if (hungerTimer >= hungerDecreaseInterval && hungryLevel > 0)
        {
            hungryLevel -= 10;
            hungerTimer = 0f;

            if (hungryLevel <= 0)
            {
                StartState(WolfState.Dead);
            }
        }
    }

    private void FindSheep()
    {
        GameObject[] sheeps = GameObject.FindGameObjectsWithTag("Sheep");

        if (sheeps.Length > 0)
        {
            GameObject nearestSheep = sheeps[0];
            float minDistance = Vector2.Distance(transform.position, nearestSheep.transform.position);

            foreach (GameObject sheep in sheeps)
            {
                float distance = Vector2.Distance(transform.position, sheep.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSheep = sheep;
                }
            }

            if (nearestSheep != null)
            {
                transform.position = Vector2.MoveTowards(transform.position, nearestSheep.transform.position, speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, nearestSheep.transform.position) < 0.5f)
                {
                    Eating(nearestSheep);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sheep"))
        {
            Debug.Log("Wolf collided with sheep");
            Eating(other.gameObject);
        }
    }

    private void DestroyWolf()
    {
        Destroy(gameObject);
    }

    private void ChangeSprite(Sprite newSprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = newSprite;
    }
}
