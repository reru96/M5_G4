using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPath2 : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitTimeAtWaypoint = 1f;
    public float detectionRadius = 5f;
    public LayerMask playerLayer;
    
    public float viewAngle = 90f;
    public int rayCount = 50;
    public LineRenderer visionConeRenderer;

    private NavMeshAgent agent;
    private int currentWaypoint = 0;
    private Transform targetPlayer;
    private Coroutine waitCoroutine;

    private enum State { Patrolling, Chasing }
    private State currentState = State.Patrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InitializeVisionCone();
        GoToNextWaypoint();
    }

    void InitializeVisionCone()
    {
        if (visionConeRenderer == null) visionConeRenderer = gameObject.AddComponent<LineRenderer>();
        visionConeRenderer.useWorldSpace = false;
        visionConeRenderer.startWidth = 0.1f;
        visionConeRenderer.endWidth = 0.1f;
        visionConeRenderer.positionCount = rayCount + 2;
    }

    void Update()
    {
        UpdateVisionCone();
        CheckPlayerDetection();
        HandleStates();
    }

    void UpdateVisionCone()
    {
        visionConeRenderer.positionCount = rayCount + 2;
        Vector3[] points = new Vector3[visionConeRenderer.positionCount];
        points[0] = Vector3.zero;

        float currentAngle = -viewAngle / 2;
        float angleIncrement = viewAngle / (rayCount - 1);

        for (int i = 1; i <= rayCount; i++)
        {
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            points[i] = dir * detectionRadius;
            currentAngle += angleIncrement;
        }

        points[rayCount + 1] = Vector3.zero;
        visionConeRenderer.SetPositions(points);
    }

    void CheckPlayerDetection()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        bool playerDetected = false;

        foreach (Collider hit in hits)
        {
            if (IsTargetInVisionCone(hit.transform))
            {
                targetPlayer = hit.transform;
                playerDetected = true;
                break;
            }
        }

        if (playerDetected)
        {
            if (currentState == State.Patrolling && waitCoroutine != null)
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
    }

    bool IsTargetInVisionCone(Transform target)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;

  
        float angle = Vector3.Angle(transform.forward, dirToTarget);
        if (angle > viewAngle / 2) return false;


        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > detectionRadius) return false;

        return true;
    }

    void HandleStates()
    {
        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrolling();
                break;

            case State.Chasing:
                HandleChasing();
                break;
        }
    }

    void HandlePatrolling()
    {
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            waitCoroutine == null)
        {
            waitCoroutine = StartCoroutine(WaitAndGoToNext());
        }
    }

    void HandleChasing()
    {
        if (targetPlayer != null)
        {
            agent.SetDestination(targetPlayer.position);

            if (!IsTargetInVisionCone(targetPlayer))
            {
                currentState = State.Patrolling;
                GoToNextWaypoint();
            }
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

   
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        for (int i = 1; i < visionConeRenderer.positionCount - 1; i++)
        {
            Vector3 worldPos = transform.TransformPoint(visionConeRenderer.GetPosition(i));
            Gizmos.DrawLine(transform.position, worldPos);
        }
    }
}
