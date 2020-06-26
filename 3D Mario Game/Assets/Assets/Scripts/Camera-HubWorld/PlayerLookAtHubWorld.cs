using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLookAtHubWorld : MonoBehaviour
{
    public Vector3 offset;
    private Transform player;
    Vector3 mypos;


// Start is called before the first frame update
void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        mypos = player.position + offset;

        float none = 0;

        transform.position = new Vector3(mypos.x, Mathf.SmoothDamp(transform.position.y,mypos.y, ref none, 6f * Time.deltaTime), mypos.z);
    }
}
