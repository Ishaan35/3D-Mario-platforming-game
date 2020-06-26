using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudPlatform : MonoBehaviour
{
    public int direction;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * 5 * direction);
    }
}
