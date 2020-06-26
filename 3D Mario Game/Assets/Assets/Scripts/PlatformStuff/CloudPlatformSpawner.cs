using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudPlatformSpawner : MonoBehaviour
{
    public GameObject CloudPlatform;
    public int direction = 0;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("spawn", 1, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void spawn()
    {
        GameObject Clone = Instantiate(CloudPlatform, transform.position, CloudPlatform.transform.rotation);
        Clone.GetComponent<CloudPlatform>().direction = direction;

    }
}
