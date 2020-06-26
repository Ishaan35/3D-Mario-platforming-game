using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigamePrizeSpawn : MonoBehaviour
{
    public GameObject prize;
    Transform spawnloc;

    public GameObject condition;

    public int prize_type; //1. 1up mushroom
    bool prize_spawned = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!prize_spawned )
        {
            if(condition.transform.childCount == 0 && prize_type == 1)
            {
                Instantiate(prize, transform.position, prize.transform.rotation);
                prize_spawned = true;
                GetComponent<AudioSource>().Play();
            }
        }
    }
}
