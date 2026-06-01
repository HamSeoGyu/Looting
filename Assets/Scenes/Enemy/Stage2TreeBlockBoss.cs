using UnityEngine;

public class Stage2TreeBlockBoss : MonoBehaviour
{
    [Header("Block Setting")]
    public string enemyTag = "Enemy";
    public float freezeRefreshDuration = 0.2f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!CompareTag(enemyTag))
            return;

        if (!other.CompareTag(enemyTag))
            return;

        if (other.gameObject == gameObject)
            return;

        if (other.transform.root == transform.root)
            return;

        EnemyMove enemyMove = other.GetComponentInParent<EnemyMove>();

        if (enemyMove == null)
            return;

        enemyMove.ApplyFreeze(freezeRefreshDuration);
    }
}