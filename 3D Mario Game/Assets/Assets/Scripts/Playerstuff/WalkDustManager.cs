using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkDustManager : MonoBehaviour
{
    public ParticleSystem Walkdust;
    public Player player;
    int count = 0; //i will use this to make sure particle.play() is called every second frame, as i dont need particle per frame
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player.grounded == true && !player.PipeEntry)
        {
            if ((Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && !Input.GetMouseButton(0) && count < 1) //if count is 0, then this will work
            {
                Walkdust.Play();
            }
            count++; //count is now 1
            if (count > 2) //count has to be 2 for it to reset, so there is a 1 frame gap in between each time the walkdust.play() is called
            {
                count = 0;
            }
            if ((Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) || Input.GetMouseButton(0))
            {
                 Walkdust.Stop();
            }
        }
        
    }


    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "ground" || other.gameObject.tag == "Enemy" || other.gameObject.tag == "CurveGround" || other.contacts[0].normal.y < 1.3f && other.contacts[0].normal.y > 0.7f) //last one is any other flat surface
        {
            
        }
    }
}
