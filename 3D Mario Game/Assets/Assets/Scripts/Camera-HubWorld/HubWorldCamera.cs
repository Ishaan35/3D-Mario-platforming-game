using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubWorldCamera : MonoBehaviour
{
    public Transform PlayerLookAt;
    public Vector3 offset;

    public bool following_Player = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(following_Player)
            transform.position = PlayerLookAt.position + offset;
    }

}
