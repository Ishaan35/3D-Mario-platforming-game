using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookatscript : MonoBehaviour
{
    public Vector3 offset;
    public Transform player;
    Vector3 mypos;

    public float smoothness_speed;
    // Start is called before the first frame update
    void Start()
    {
        offset = player.transform.position - transform.position;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;
        
        mypos = (player.position) - offset;

        transform.position = Vector3.SmoothDamp(transform.position, mypos, ref velocity, 4.5f * Time.deltaTime);

        
        
    }
}
