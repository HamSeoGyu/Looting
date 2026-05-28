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
        currentHP -= damage;

        if (currentHP < 0)
            currentHP = 0;

        UpdateHPBar();

        if (currentHP <= 0)
        {
            Die();
        }
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

        // 오른쪽에서 왼쪽으로 줄어들게 유지
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

        // 살아 있는 적 카운트에서 즉시 빠지도록 태그 제거
        if (CompareTag("Enemy"))
        {
            gameObject.tag = "Untagged";
        }

        // 이동 멈춤
        EnemyMove move = GetComponent<EnemyMove>();
        if (move != null)
        {
            move.enabled = false;
        }

        // 충돌 끄기
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // 물리 멈춤
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