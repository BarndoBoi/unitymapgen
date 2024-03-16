using UnityEngine;
using UnityEngine.AI;

public class BotFSM : MonoBehaviour
{
    public float wanderRadius = 10f;
    public float closeDistanceRange = 5f;
    public float attackRange = 2f;

    private NavMeshAgent agent;
    private Transform player;
    private BotState currentState;

    private enum BotState
    {
        Wander,
        CloseDistanceToPlayer,
        AttackPlayer
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = BotState.Wander;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case BotState.Wander:
                HandleWanderState();
                if (distanceToPlayer <= closeDistanceRange)
                    currentState = BotState.CloseDistanceToPlayer;
                break;

            case BotState.CloseDistanceToPlayer:
                HandleCloseDistanceState(distanceToPlayer);
                if (distanceToPlayer <= attackRange)
                    currentState = BotState.AttackPlayer;
                else if (distanceToPlayer > closeDistanceRange)
                    currentState = BotState.Wander;
                break;

            case BotState.AttackPlayer:
                HandleAttackState();
                if (distanceToPlayer > attackRange)
                    currentState = BotState.CloseDistanceToPlayer;
                break;
        }
    }

    void HandleWanderState()
    {
        // Implement your wandering behavior here
        // e.g., move to a random point within wanderRadius
    }

    void HandleCloseDistanceState(float distanceToPlayer)
    {
        // Implement your close distance behavior here
        // e.g., move towards the player while maintaining closeDistanceRange
    }

    void HandleAttackState()
    {
        // Implement your attack behavior here
        // e.g., perform an attack action
    }
}