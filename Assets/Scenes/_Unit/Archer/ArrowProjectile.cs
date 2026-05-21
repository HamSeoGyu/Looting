using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 7f;
    public float turnSpeed = 540f;
    public float lifeTime = 4f;
    public float hitDistance = 0.18f;

    [Header("Damage")]
    public int damage = 2;

    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;
    public bool use8DirectionSprite = true;

    [Header("8 Direction Sprites")]
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite upRightSprite;
    public Sprite downRightSprite;
    public Sprite upLeftSprite;
    public Sprite downLeftSprite;

    private Transform target;
    private EnemyHealth targetHealth;
    private Vector2 moveDirection = Vector2.right;
    private float lifeTimer = 0f;
    private bool hasHit = false;

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError(gameObject.name + " : SpriteRenderer가 없습니다.");
        }
    }

    public void Initialize(Transform newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;

        Debug.Log(gameObject.name + " : Initialize 호출");

        if (target != null)
        {
            targetHealth = target.GetComponent<EnemyHealth>();
            if (targetHealth == null) targetHealth = target.GetComponentInParent<EnemyHealth>();
            if (targetHealth == null) targetHealth = target.GetComponentInChildren<EnemyHealth>();

            Vector2 dir = (target.position - transform.position);
            if (dir.sqrMagnitude > 0.001f)
            {
                moveDirection = dir.normalized;
            }
        }

        UpdateArrowVisual(moveDirection);
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (hasHit) return;

        if (target != null)
        {
            Vector2 toTarget = (target.position - transform.position);
            float distance = toTarget.magnitude;

            if (distance <= hitDistance)
            {
                HitTarget();
                return;
            }

            if (distance > 0.001f)
            {
                Vector2 desiredDir = toTarget.normalized;

                float currentAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                float targetAngle = Mathf.Atan2(desiredDir.y, desiredDir.x) * Mathf.Rad2Deg;

                float newAngle = Mathf.MoveTowardsAngle(
                    currentAngle,
                    targetAngle,
                    turnSpeed * Time.deltaTime
                );

                float rad = newAngle * Mathf.Deg2Rad;
                moveDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            }
        }

        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        UpdateArrowVisual(moveDirection);
    }

    void HitTarget()
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log(gameObject.name + " : 타겟 명중");

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    void UpdateArrowVisual(Vector2 dir)
    {
        if (spriteRenderer == null) return;

        if (use8DirectionSprite)
        {
            Set8DirectionSprite(dir);
            transform.rotation = Quaternion.identity;
        }
        else
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void Set8DirectionSprite(Vector2 dir)
    {
        float x = dir.x;
        float y = dir.y;

        Sprite selected = null;

        if (Mathf.Abs(x) < 0.35f && y > 0f) selected = upSprite;
        else if (Mathf.Abs(x) < 0.35f && y < 0f) selected = downSprite;
        else if (x > 0f && Mathf.Abs(y) < 0.35f) selected = rightSprite;
        else if (x < 0f && Mathf.Abs(y) < 0.35f) selected = leftSprite;
        else if (x > 0f && y > 0f) selected = upRightSprite;
        else if (x > 0f && y < 0f) selected = downRightSprite;
        else if (x < 0f && y > 0f) selected = upLeftSprite;
        else if (x < 0f && y < 0f) selected = downLeftSprite;

        if (selected != null)
        {
            spriteRenderer.sprite = selected;
        }
    }
}