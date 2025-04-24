using UnityEngine;

public class Sheep : MonoBehaviour
{
    private Rigidbody2D rb;

    public float speed = 2f;
    public float changeDirectionTime = 5f;
    private Vector2 targetPosition;
    private float timer;
    private float idleTimer;
    private float eatingTime = 3f;
    private float eatingTimer = 0f;

    public int hungryLevel = 100; //starter hungry level
    private float hungerTimer = 0f;
    public float hungerDecreaseInterval = 2f; //every 2 sec - 1

    public Sprite stand;
    public Sprite walk;
    public Sprite eat;
    public Sprite dead;

    private SheepState currentState = SheepState.Walking;

    // Add a timer for sheep's life
    private float lifeTimer = 0f;
    public float maxLifeTime = 30f; // Max lifetime before sheep dies

    public enum SheepState
    {
        Idle,
        Walking,
        Eating,
        FindFood,
        Dead
    }

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        SetNewTargetPosition(); // randomly spawn one position
        idleTimer = Random.Range(5f, 10f);
    }

    public void InitializeSheep(int initialHungryLevel)
    {
        hungryLevel = initialHungryLevel;  // Set the hungry level when the sheep is spawned
    }

    void Update()
    {
        UpdateState();
        lifeTimer += Time.deltaTime;  // Increment the life timer

        // Check if the sheep's life timer exceeds max life time
        if (lifeTimer >= maxLifeTime)
        {
            StartState(SheepState.Dead);  // Change state to Dead if exceeded
        }
    }

    void SetNewTargetPosition()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        //randomly select one targetPosition within camera range
        float randomX = Random.Range(cam.transform.position.x - camWidth, cam.transform.position.x + camWidth);
        float randomY = Random.Range(cam.transform.position.y - camHeight, cam.transform.position.y + camHeight);
        targetPosition = new Vector2(randomX, randomY);
    }

    public void StartState(SheepState newState)
    {
        EndState(currentState);

        // start preparation eg.Animation & sound
        switch (newState)
        {
            case SheepState.Idle:
                ChangeSprite(stand);
                break;
            case SheepState.Walking:
                ChangeSprite(walk);
                break;
            case SheepState.Eating:
                ChangeSprite(eat);
                Debug.Log("Sheep is eating");
                break;
            case SheepState.Dead:
                ChangeSprite(dead);
                Debug.Log("Sheep has died.");
                Invoke(nameof(DestroySheep), 5f);
                break;
        }
        currentState = newState;
    }

    public void UpdateState()
    {
        switch (currentState)
        {
            case SheepState.Idle:
                Idle();
                break;
            case SheepState.Walking:
                Move();
                break;
            case SheepState.FindFood:
                FindFood();
                break;
            case SheepState.Eating:
                Eating();
                break;
            case SheepState.Dead:
                break;
        }
    }

    private void EndState(SheepState oldState)
    {
        switch (oldState)
        {
            case SheepState.Idle:
                break;
            case SheepState.Walking:
                break;
            case SheepState.Eating:
                break;
            case SheepState.Dead:
                break;
        }
    }

    private void Idle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            StartState(SheepState.Walking);
            idleTimer = Random.Range(3f, 8f);
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

        hungerTimer += Time.deltaTime;
        if (hungerTimer >= hungerDecreaseInterval && hungryLevel > 0)
        {
            hungryLevel -= 10;
            hungerTimer = 0f;

            if (hungryLevel <= 60 && currentState != SheepState.FindFood)
            {
                StartState(SheepState.FindFood);
            }

            if (hungryLevel <= 0)
            {
                StartState(SheepState.Dead);
            }
        }

        ClampPosition();
    }

    private void Eating()
    {
        eatingTimer += Time.deltaTime;

        if (eatingTimer >= eatingTime)
        {
            eatingTimer = 0f;
            StartState(SheepState.Idle);
            idleTimer = Random.Range(1f, 5f);
        }
    }

    private void Eating(GameObject food)
    {
        if (food == null) return;

        hungryLevel += 50;

        if (hungryLevel >= 100)
        {
            hungryLevel = 100;
        }

        Destroy(food);
        Debug.Log("Food destroyed.");

        //eating animation
        StartState(SheepState.Eating);
        Invoke(nameof(CompleteEating), eatingTime);
    }

    private void CompleteEating()
    {
        // get sheep's current position to spawn a new sheep after eating
        Vector2 sheepPosition = transform.position;

        // let SheepSpawner to spawn
        FindObjectOfType<SheepSpawner>().SpawnSheepAtPosition(sheepPosition);

        StartState(SheepState.Idle);
        idleTimer = Random.Range(1f, 3f);
    }

    private void FindFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Fruit");

        if (foods.Length > 0)
        {
            GameObject nearestFood = foods[0];
            float minDistance = Vector2.Distance(transform.position, nearestFood.transform.position);

            foreach (GameObject food in foods)
            {
                float distance = Vector2.Distance(transform.position, food.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestFood = food;
                }
            }

            if (nearestFood != null)
            {
                transform.position = Vector2.MoveTowards(transform.position, nearestFood.transform.position, speed * Time.deltaTime);

                hungerTimer += Time.deltaTime;
                if (hungerTimer >= hungerDecreaseInterval && hungryLevel > 0)
                {
                    hungryLevel -= 10;
                    hungerTimer = 0f;

                    if (hungryLevel <= 0)
                    {
                        StartState(SheepState.Dead);
                        return;
                    }
                }

                // start eating if close to food
                if (Vector2.Distance(transform.position, nearestFood.transform.position) < 0.5f)
                {
                    // eat() only if state not Eating
                    if (currentState != SheepState.Eating)
                    {
                        Eating(nearestFood);
                    }
                }
            }
        }
        else
        {
            StartState(SheepState.Walking);
        }
    }

    private void DestroySheep()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fruit"))
        {
            Debug.Log("Sheep collided with fruit");
            Eating(other.gameObject);
        }
    }

    private void ClampPosition()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        Collider2D collider = GetComponent<Collider2D>();
        float halfWidth = collider.bounds.size.x / 2f;
        float halfHeight = collider.bounds.size.y / 2f;

        float x = Mathf.Clamp(transform.position.x, cam.transform.position.x - camWidth + halfWidth, cam.transform.position.x + camWidth - halfWidth);
        float y = Mathf.Clamp(transform.position.y, cam.transform.position.y - camHeight + halfHeight, cam.transform.position.y + camHeight - halfHeight);

        transform.position = new Vector2(x, y);
    }

    private void ChangeSprite(Sprite newSprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = newSprite;
    }
}
