using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private bool isWASD;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h != 0 || v != 0)
        {
            isWASD = true;
        }
        else
        {
            isWASD = false;
        }

        if (isWASD)
        {
            Vector3 dir = new Vector3(h, 0, v);
            agent.velocity = dir * agent.speed;
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    agent.SetDestination(hit.point);
                }

            }
        }
    }
}
