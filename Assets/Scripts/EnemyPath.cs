using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitTimeAtWaypoint = 1f;
    public float detectionRadius = 5f;
    public LayerMask playerLayer;

    private NavMeshAgent agent;
    private int currentWaypoint = 0;
    private Transform targetPlayer;
    private Coroutine waitCoroutine;

    private enum State { Patrolling, Chasing }
    private State currentState = State.Patrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextWaypoint();
    }

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        if (hits.Length > 0)
        {
            targetPlayer = hits[0].transform;

            if (waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }

            currentState = State.Chasing;
        }
        else if (currentState == State.Chasing)
        {
            currentState = State.Patrolling;
            GoToNextWaypoint();
        }

        if (currentState == State.Patrolling)
        {
           
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && waitCoroutine == null)
            {
                waitCoroutine = StartCoroutine(WaitAndGoToNext());
            }
        }
        else if (currentState == State.Chasing && targetPlayer != null)
        {
            agent.SetDestination(targetPlayer.position);
        }
    }

    IEnumerator WaitAndGoToNext()
    {
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        GoToNextWaypoint();
        waitCoroutine = null;
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypoint].position);
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
