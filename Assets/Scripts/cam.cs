using UnityEngine;
using System.Collections;
public class cam : MonoBehaviour
{
    RaycastHit hit;
    LayerMask layerMask;
    GameObject hitted;
    [SerializeField] public Transform target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layerMask = LayerMask.GetMask("notwalka");
    }

    // Update is called once per frame
    void Update()
    {
        Physics.Raycast(transform.position,target.position-transform.position, out hit, 10f,layerMask);
        Debug.DrawRay(transform.position, target.position - transform.position, Color.red);
        Debug.Log(hit.collider);
        if (hit.collider != null)
        {
            hit.collider.GetComponent<Renderer>().enabled = false;
            hitted = hit.collider.gameObject;
        }
        if(hitted != null && hit.collider == null){
            hitted.GetComponent<Renderer>().enabled = true;;
        }
    }
}
