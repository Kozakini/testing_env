using UnityEngine;

public class camera : MonoBehaviour
{
    RaycastHit hit;
    LayerMask layerMask;
    GameObject hitted;
     public Transform target;
     private Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = transform.position - target.position;
        layerMask = LayerMask.GetMask("notwalka");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = offset + target.position;

        Physics.Raycast(transform.position,target.position-transform.position, out hit, 10000f);
        Debug.DrawRay(transform.position, target.position - transform.position, Color.red);
        Debug.Log(hit.collider);
        if (hit.collider != null)
        {
            hit.collider.gameObject.SetActive(false);
            hitted = hit.collider.gameObject;
        }
        if(hitted != null && hit.collider == null){
            hitted.SetActive(true);
        }

    }
}
