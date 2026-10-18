using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    public float dampTime = 0.15f;
    Animator animator;
    NavMeshAgent agent;
    EnemyAI ai;

    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int ChasingHash = Animator.StringToHash("Chasing");

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();
        ai = GetComponentInParent<EnemyAI>();
        animator.applyRootMotion = false;
    }

    void Update()
    {
        if (agent == null || ai == null) return;
        animator.SetFloat(SpeedHash, agent.velocity.magnitude, dampTime, Time.deltaTime);
        animator.SetBool(ChasingHash, ai.State == EnemyState.Chase);
    }
}
