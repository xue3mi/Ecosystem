using UnityEngine;

public class Lion : MonoBehaviour
{
    private Rigidbody2D rb;

    public float speed = 4f;
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

    private LionState currentState = LionState.Walking;
    public int hungryLevel;
    private float hungerTimer = 0f;
    public float hungerDecreaseInterval = 5f;

    public float maxLifespan = 25f; // Maximum lifespan in seconds
    private float ageTimer = 0f; // Timer to track the lion's age

    public enum LionState
    {
        Idle,
        Walking,
        Eating,
        FindWolf,  // New state for finding wolves
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
        AgeLion();

        // If the lion is hungry and not in the dead state, start looking for a wolf
        if (hungryLevel < 30 && currentState != LionState.Dead && currentState != LionState.Eating)
        {
            StartState(LionState.FindWolf);
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

    public void StartState(LionState newState)
    {
        EndState(currentState);

        switch (newState)
        {
            case LionState.Idle:
                ChangeSprite(stand);
                break;
            case LionState.Walking:
                ChangeSprite(walk);
                break;
            case LionState.Eating:
                ChangeSprite(eat);
                Debug.Log("Lion is eating");
                break;
            case LionState.Dead:
                ChangeSprite(dead);
                Debug.Log("Lion has died.");
                Invoke(nameof(DestroyLion), 5f);
                break;
            case LionState.FindWolf: // New state for finding wolves
                FindWolf();
                break;
        }
        currentState = newState;
    }

    public void UpdateState()
    {
        switch (currentState)
        {
            case LionState.Idle:
                Idle();
                break;
            case LionState.Walking:
                Move();
                break;
            case LionState.Eating:
                Eating();
                break;
            case LionState.FindWolf: // Handling the state of finding wolves
                FindWolf();
                break;
            case LionState.Dead:
                break;
        }
    }

    private void EndState(LionState oldState)
    {
        switch (oldState)
        {
            case LionState.Idle:
                break;
            case LionState.Walking:
                break;
            case LionState.Eating:
                break;
            case LionState.Dead:
                break;
        }
    }

    private void Idle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            StartState(LionState.Walking);
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
            StartState(LionState.Idle);
            idleTimer = Random.Range(1f, 5f);
        }
    }

    private void Eating(GameObject wolf)
    {
        // Only allow eating if hungry
        if (hungryLevel < 30)
        {
            if (wolf == null) return;
            Destroy(wolf);
            hungryLevel = Mathf.Min(100, hungryLevel + 50);
            StartState(LionState.Eating);
            Invoke(nameof(CompleteEating), eatingTime);
        }
    }

    private void CompleteEating()
    {
        StartState(LionState.Idle);
        idleTimer = Random.Range(1f, 5f);

        // Spawn a new lion at the current lion's position after eating
        GameObject lionSpawner = GameObject.FindObjectOfType<LionSpawner>().gameObject;
        lionSpawner.GetComponent<LionSpawner>().SpawnLionAtPosition(transform.position);
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
                StartState(LionState.Dead);
            }
        }
    }

    private void FindWolf()
    {
        GameObject[] wolves = GameObject.FindGameObjectsWithTag("Wolf");

        if (wolves.Length > 0)
        {
            GameObject nearestWolf = wolves[0];
            float minDistance = Vector2.Distance(transform.position, nearestWolf.transform.position);

            foreach (GameObject wolf in wolves)
            {
                float distance = Vector2.Distance(transform.position, wolf.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestWolf = wolf;
                }
            }

            if (nearestWolf != null)
            {
                transform.position = Vector2.MoveTowards(transform.position, nearestWolf.transform.position, speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, nearestWolf.transform.position) < 0.5f)
                {
                    Eating(nearestWolf);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wolf"))
        {
            Debug.Log("Lion collided with wolf");
            Eating(other.gameObject);  // Trigger eating when lion collides with wolf
        }
    }

    private void DestroyLion()
    {
        Destroy(gameObject);
    }

    private void ChangeSprite(Sprite newSprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    // Track the lion's age and handle death when it exceeds the maximum lifespan
    private void AgeLion()
    {
        ageTimer += Time.deltaTime;

        if (ageTimer >= maxLifespan)
        {
            StartState(LionState.Dead);  // If the lion exceeds its max lifespan, it dies
        }
    }
}
