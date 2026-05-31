using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class SPUMEnemyWalk : MonoBehaviour
{
    [Header("SPUM")]
    public SPUM_Prefabs spumPrefab;

    [Header("Animator")]
    public Animator animator;

    [Header("Move Animation")]
    public int moveIndex = 0;
    public bool playMoveOnEnable = true;
    public bool loopMove = true;

    [Header("Death Animation")]
    public int deathIndex = 0;

    private PlayableGraph graph;
    private AnimationClipPlayable currentPlayable;
    private AnimationClip currentClip;
    private bool isPlayingDeath = false;

    private void Awake()
    {
        if (spumPrefab == null)
            spumPrefab = GetComponentInChildren<SPUM_Prefabs>(true);

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
    }

    private void OnEnable()
    {
        isPlayingDeath = false;

        if (playMoveOnEnable)
            PlayMove();
    }

    private void Update()
    {
        if (!graph.IsValid()) return;
        if (!currentPlayable.IsValid()) return;
        if (currentClip == null) return;
        if (currentClip.length <= 0f) return;

        if (currentPlayable.GetTime() >= currentClip.length)
        {
            if (isPlayingDeath)
            {
                currentPlayable.SetTime(currentClip.length);
                graph.Evaluate(0f);
            }
            else if (loopMove)
            {
                currentPlayable.SetTime(0f);
                graph.Evaluate(0f);
            }
        }
    }

    public bool PlayMove()
    {
        if (spumPrefab == null)
        {
            Debug.LogWarning(gameObject.name + " 에 SPUM_Prefabs가 없습니다.");
            return false;
        }

        if (animator == null)
        {
            Debug.LogWarning(gameObject.name + " 에 Animator가 없습니다.");
            return false;
        }

        if (spumPrefab.MOVE_List == null || spumPrefab.MOVE_List.Count == 0)
        {
            Debug.LogWarning(gameObject.name + " 의 MOVE_List가 비어 있습니다.");
            return false;
        }

        if (moveIndex < 0 || moveIndex >= spumPrefab.MOVE_List.Count)
        {
            Debug.LogWarning(gameObject.name + " 의 moveIndex가 범위를 벗어났습니다.");
            return false;
        }

        AnimationClip clip = spumPrefab.MOVE_List[moveIndex];

        if (clip == null)
        {
            Debug.LogWarning(gameObject.name + " 의 MOVE_List[" + moveIndex + "]가 비어 있습니다.");
            return false;
        }

        isPlayingDeath = false;
        PlayClip(clip);
        return true;
    }

    public bool PlayDeath()
    {
        if (spumPrefab == null)
        {
            Debug.LogWarning(gameObject.name + " 에 SPUM_Prefabs가 없습니다.");
            return false;
        }

        if (animator == null)
        {
            Debug.LogWarning(gameObject.name + " 에 Animator가 없습니다.");
            return false;
        }

        if (spumPrefab.DEATH_List == null || spumPrefab.DEATH_List.Count == 0)
        {
            Debug.LogWarning(gameObject.name + " 의 DEATH_List가 비어 있습니다.");
            return false;
        }

        if (deathIndex < 0 || deathIndex >= spumPrefab.DEATH_List.Count)
        {
            Debug.LogWarning(gameObject.name + " 의 deathIndex가 범위를 벗어났습니다.");
            return false;
        }

        AnimationClip clip = spumPrefab.DEATH_List[deathIndex];

        if (clip == null)
        {
            Debug.LogWarning(gameObject.name + " 의 DEATH_List[" + deathIndex + "]가 비어 있습니다.");
            return false;
        }

        isPlayingDeath = true;
        PlayClip(clip);
        return true;
    }

    private void PlayClip(AnimationClip clip)
    {
        StopGraph();

        currentClip = clip;
        currentPlayable = AnimationPlayableUtilities.PlayClip(animator, currentClip, out graph);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
    }

    private void OnDisable()
    {
        StopGraph();
    }

    private void OnDestroy()
    {
        StopGraph();
    }

    private void StopGraph()
    {
        if (graph.IsValid())
            graph.Destroy();
    }
}