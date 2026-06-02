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

        if (hpBarFill != null)
        {
            hpBarOriginalScale = hpBarFill.localScale;
            hpBarOriginalPosition = hpBarFill.localPosition;
        }

        UpdateHPBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        damage = Mathf.Max(0, damage);

        EnemyCurseReceiver curseReceiver = GetComponent<EnemyCurseReceiver>();

        if (curseReceiver != null)
        {
            damage = curseReceiver.ModifyIncomingDamage(damage);
        }

        EnemyGoblinFrenzyReceiver frenzyReceiver = GetComponent<EnemyGoblinFrenzyReceiver>();

        if (frenzyReceiver != null)
        {
            damage = frenzyReceiver.ModifyIncomingDamage(damage);
        }

        currentHP -= damage;

        if (currentHP < 0)
        {
            currentHP = 0;
        }

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
            if (col != null)
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
        Component spumEnemyWalk = GetComponent("SPUMEnemyWalk");

        if (spumEnemyWalk == null)
        {
            Component[] components = GetComponentsInChildren<Component>(true);

            foreach (Component component in components)
            {
                if (component != null && component.GetType().Name == "SPUMEnemyWalk")
                {
                    spumEnemyWalk = component;
                    break;
                }
            }
        }

        if (spumEnemyWalk != null)
        {
            System.Reflection.MethodInfo method = spumEnemyWalk.GetType().GetMethod("PlayDeath");

            if (method != null)
            {
                object result = method.Invoke(spumEnemyWalk, null);

                if (result is bool && (bool)result)
                {
                    return;
                }
            }
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
        if (anim == null) return false;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }

        return false;
    }

    bool HasTriggerParameter(Animator anim, string paramName)
    {
        if (anim == null) return false;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
            {
                return true;
            }
        }

        return false;
    }
}