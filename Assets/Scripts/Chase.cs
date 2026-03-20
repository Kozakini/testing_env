using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AIController : MonoBehaviour
{
    Animator animation;
    public Transform target;

    public Transform cube;

    public float rotationSpeed = 3f;

    private Quaternion lookRotation;

    private Vector3 directionTarget;
    private Vector3 movement;

    private Renderer colored;
    public int speed = 1;
    public Rigidbody rb;

    private NavMeshAgent agent;
    void Start()
    {
        colored = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();
        animation = GetComponent<Animator>();


    }

    void Update()
    {

      agent.SetDestination(target.position);
      animation.SetFloat("Speed",1);
      if (cube != null){
          cube.position = transform.position;
      }


    }
}
