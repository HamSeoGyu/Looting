using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 20;
    public int currentHP;

    [Header("Reward")]
    public int rewardGold = 10;

    [Header("HP Bar")]
    public Transform hpBarFill;

    [Header("Death Animation")]
    public Animator animator;
    public float destroyDelay = 1.2f;

    private bool isDead = false;

    public bool IsDead
    {
        get { return isDead; }
    }

    private Vector3 hpBarOriginalScale;
    private Vector3 hpBarOriginalPosition;

    void Awake()
    {
        currentHP = maxHP;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (hpBarFill != null)
        {
            hpBarOriginalScale = hpBarFill.localScale;
            hpBarOriginalPosition = hpBarFill.localPosition;
        }

        UpdateHPBar();
    }

    public void SetMaxHP(int newMaxHP)
    {
        maxHP = Mathf.Max(1, newMaxHP);
        currentHP = maxHP;
        UpdateHPBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        damage = Mathf.Max(0, damage);
<<<<<<< HEAD
=======

        EnemyBuffReceiver buffReceiver = GetComponent<EnemyBuffReceiver>();
        if (buffReceiver != null)
        {
            damage = buffReceiver.ModifyIncomingDamage(damage);
        }

>>>>>>> 589f55e (boss 삭제)
        currentHP -= damage;

        if (currentHP < 0)
            currentHP = 0;

        UpdateHPBar();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public Vector2 GetClosestPoint(Vector2 fromPosition)
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        float closestDistance = Mathf.Infinity;
        Vector2 closestPoint = transform.position;

        foreach (Collider2D col in colliders)
        {
            if (col == null) continue;
            if (!col.enabled) continue;

            Vector2 point = col.ClosestPoint(fromPosition);
            float distance = Vector2.Distance(fromPosition, point);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    void UpdateHPBar()
    {
        if (hpBarFill == null) return;
        if (maxHP <= 0) return;

        float ratio = (float)currentHP / maxHP;
        ratio = Mathf.Clamp01(ratio);

        Vector3 newScale = hpBarOriginalScale;
        newScale.x = hpBarOriginalScale.x * ratio;
        hpBarFill.localScale = newScale;

        float diff = hpBarOriginalScale.x - newScale.x;
        Vector3 newPos = hpBarOriginalPosition;
        newPos.x = hpBarOriginalPosition.x + diff / 2f;
        hpBarFill.localPosition = newPos;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(rewardGold);
        }

        if (CompareTag("Enemy"))
        {
            gameObject.tag = "Untagged";
        }

        EnemyMove move = GetComponent<EnemyMove>();
        if (move != null)
        {
            move.enabled = false;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        PlayDeathAnimation();

        StartCoroutine(DestroyAfterDelay());
    }

    void PlayDeathAnimation()
    {
        SPUMEnemyWalk spumEnemyWalk = GetComponent<SPUMEnemyWalk>();

        if (spumEnemyWalk == null)
            spumEnemyWalk = GetComponentInChildren<SPUMEnemyWalk>(true);

        if (spumEnemyWalk != null)
        {
            bool played = spumEnemyWalk.PlayDeath();

            if (played)
                return;
        }

        if (animator == null) return;

        if (HasTriggerParameter(animator, "4_Death"))
        {
            animator.ResetTrigger("4_Death");
            animator.SetTrigger("4_Death");
        }

        if (HasBoolParameter(animator, "isDeath"))
        {
            animator.SetBool("isDeath", true);
        }
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    bool HasBoolParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                return true;
        }

        return false;
    }

    bool HasTriggerParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
                return true;
        }

        return false;
    }
}