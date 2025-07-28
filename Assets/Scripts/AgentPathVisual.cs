using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class AgentPathVisual : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private NavMeshAgent agent;
    private NavMeshPath path;

    public Color colorComplete = Color.green;
    public Color colorPartial = Color.yellow;
    public Color colorInvalid = Color.red;

    private Vector3 pendingDestination;
    private bool hasPendingDestination = false;


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (agent.CalculatePath(hit.point, path))
                {
                    DrawPath(path);
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        pendingDestination = hit.point;
                        hasPendingDestination = true;
                    }
                    else
                    {
                        agent.SetDestination(hit.point);
                        hasPendingDestination = false;
                    }
                }
            }
            if (hasPendingDestination && Input.GetKeyUp(KeyCode.LeftShift))
            {
                agent.SetDestination(pendingDestination);
                hasPendingDestination = false;
            }
        }
    }

    void DrawPath(NavMeshPath navPath)
    {
        if (navPath.status == NavMeshPathStatus.PathInvalid || navPath.corners.Length < 2)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = navPath.corners.Length;
        lineRenderer.SetPositions(navPath.corners);


        switch (navPath.status)
        {
            case NavMeshPathStatus.PathComplete:
                lineRenderer.material.color = colorComplete;
                break;
            case NavMeshPathStatus.PathPartial:
                lineRenderer.material.color = colorPartial;
                break;
            case NavMeshPathStatus.PathInvalid:
                lineRenderer.material.color = colorInvalid;
                break;
        }
    }
}
