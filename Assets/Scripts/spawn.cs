using UnityEditor;
using UnityEngine;


public class spawn : MonoBehaviour
{
    public GameObject prefab;
    private float time = 0;
    public Transform target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time >= 5)
        {
            GameObject clone;
            clone = Instantiate(prefab);
            clone.GetComponent<AIController>().target = target;
            time = 0;
        }
    }
}
