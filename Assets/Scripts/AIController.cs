using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIController : PlayerController2D
{
    public Transform ball;
    public float speed = 5f;
    public float stop = 0.5f;

    [SerializeField] private float center = 6f;
    public Transform def;
    private string team;
    private Rigidbody2D rb;

    public float itemRng = 100f;
    public float itemCD = 10f;

    public bool aggressive = false;
    private const float Timeout = 2.0f;
    private float chaseTimer = 0f;
    private float itemCDT = 0f;
    private AIController[] mates = new AIController[0];

    public Transform Target { get; private set; }
    private Transform itemTgt;

    private void Init()
    {
        if (transform.parent != null && string.IsNullOrEmpty(team))
        {
            team = transform.parent.name;
            mates = transform.parent.GetComponentsInChildren<AIController>();

            if (mates.Length > 0 && Random.value < 0.66f)
            {
                aggressive = true;
            }
        }

        if (ball == null)
        {
            GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
            if (ballObj != null) ball = ballObj.transform;
        }

        if (def == null && !string.IsNullOrEmpty(team))
        {
            GameObject defObj = new GameObject($"DefTarget_{team}");
            float defX = team == "Team1" ? center - 2f : center + 2f;
            defObj.transform.position = new Vector3(defX, transform.position.y, 0);
            def = defObj.transform;
            defObj.hideFlags = HideFlags.HideInHierarchy;
        }

        chaseTimer = 0f;
        itemTgt = null;
        Target = ball;
    }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        controlledByPlayer = 0;
        enabled = true;
        Init();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (string.IsNullOrEmpty(team)) Init();
    }

    private int CountTgt(Transform target)
    {
        if (target == null) return 0;
        int count = 0;
        foreach (var ai in mates)
        {
            if (ai != this && ai.Target == target)
            {
                count++;
            }
        }
        return count;
    }

    void Update()
    {
        if (ball == null || rb == null || def == null) return;

        if (itemCDT > 0) itemCDT -= Time.deltaTime;
        ChaseUpdate();

        Target = FindTgt();

        Move();
    }

    private void ChaseUpdate()
    {
        if (Target == itemTgt && itemTgt != null)
        {
            chaseTimer += Time.deltaTime;
        }
        else
        {
            chaseTimer = 0f;
            itemTgt = null;
        }
    }

    private Transform FindTgt()
    {
        Transform collectibleTarget = NearestItem();
        if (collectibleTarget != null)
        {
            return collectibleTarget;
        }

        return BallOrDef();
    }

    private Transform NearestItem()
    {
        if (itemCDT > 0) return null;

        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");

        float sqrMinDistance = itemRng * itemRng;
        Transform nearestCollectible = null;

        foreach (var collectible in collectibles)
        {
            Vector3 offset = transform.position - collectible.transform.position;
            float sqrDist = offset.sqrMagnitude;

            if (sqrDist < sqrMinDistance)
            {
                sqrMinDistance = sqrDist;
                nearestCollectible = collectible.transform;
            }
        }

        if (nearestCollectible != null)
        {
            int collectibleCount = CountTgt(nearestCollectible);

            if (chaseTimer < Timeout && collectibleCount < 2)
            {
                itemTgt = nearestCollectible;
                return nearestCollectible;
            }
            else if (chaseTimer >= Timeout)
            {
                Debug.Log($"[{name}] Abandoning item chase after {Timeout}s timeout.");
                chaseTimer = 0f;
            }
        }
        return null;
    }

    private Transform BallOrDef()
    {
        bool isBallOnOurSide = team == "Team1" ? ball.position.x < center : ball.position.x > center;

        Transform preferredTarget;
        Transform alternateTarget;

        if (aggressive)
        {
            preferredTarget = ball;
            alternateTarget = def;
        }
        else
        {
            preferredTarget = isBallOnOurSide ? ball : def;
            alternateTarget = isBallOnOurSide ? def : ball;
        }

        int preferredTargetCount = CountTgt(preferredTarget);

        if (preferredTargetCount < 2)
        {
            return preferredTarget;
        }

        int alternateTargetCount = CountTgt(alternateTarget);
        if (alternateTargetCount < 2)
        {
            return alternateTarget;
        }

        Debug.LogWarning($"[{name}] Both Ball and Defense targets are saturated (>=2 members). Sticking to Ball.");
        return ball;
    }

    private void Move()
    {
        if (Target == null)
        {
            moveInput = Vector2.zero;
            isMoving = false;
            return;
        }

        Vector3 targetDirection = Target.position - transform.position;
        float distance = targetDirection.magnitude;

        float currentStopDistance = (Target == ball || Target == def) ? stop : 0.1f;

        if (distance > currentStopDistance)
        {
            Vector2 desiredDirection = targetDirection.normalized;
            moveInput = Vector2.Lerp(moveInput, desiredDirection, Time.deltaTime * speed);
            isMoving = true;
        }
        else
        {
            moveInput = Vector2.zero;
            isMoving = false;
        }

        if (isMoving)
        {
            targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
        }
    }

    void FixedUpdate()
    {
        base.ApplySteering();
        base.ApplyMovement();
        base.ApplyDrift();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Collectible"))
        {
            itemCDT = itemCD;
            chaseTimer = 0f;
            itemTgt = null;
        }
    }
}