using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarioWalkSounds : MonoBehaviour
{
    public Player player;
    public CameraFollow cam_follow;
    public AudioSource StompSound;
    public AudioSource BrickWalkSound;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Play_Stomp_Sound()
    {
        if(player.MEGAMUSHROOM)
        {
            StompSound.Play();
        }
        if(cam_follow.Underground && !player.MEGAMUSHROOM)
        {
            BrickWalkSound.Play();
        }
    }
}
