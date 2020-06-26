using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaGoomba : MonoBehaviour
{
    private GameObject player;
    private Animator goomba_anim;
    private Rigidbody rb;
    public AudioSource Vanish;


    private SkinnedMeshRenderer renderer;
    public Material idle_face;
    public Material mad_face;
    public Material dead_face;
    private GameObject body;

    public GameObject MegaMushroom;


    private AudioSource Surprise;
    bool play_Surprise = true;//play sound once

    public float speed;

    private ParticleSystem ChasePS;
    int particle_count;
    bool grounded;

    private AudioSource Hit_Sound;

    bool close;
    bool dead = false;

    public GameObject DestroyPS;

    public int health = 5;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        goomba_anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Surprise = GetComponent<AudioSource>();

        body = transform.GetChild(3).gameObject; //third gameobject
        renderer = body.GetComponent<SkinnedMeshRenderer>();

        ChasePS = transform.GetChild(6).GetComponent<ParticleSystem>();

        Hit_Sound = transform.GetChild(7).GetComponent<AudioSource>();

        DestroyPS = GameObject.FindGameObjectWithTag("DestroyParticleSystem");


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //this boolean checks if the goomba is within a certain range of the player
        close = (transform.position.x - player.transform.position.x <= 0.5 && transform.position.x - player.transform.position.x >= -0.5) && (transform.position.z - player.transform.position.z <= 0.5 && transform.position.z - player.transform.position.z >= -0.5);

        if (Vector3.Distance(player.transform.position, transform.position) < 20 && !dead)
        {
            StartCoroutine(Chase());
        }

    }

    public IEnumerator Chase()
    {
        Vector3 direction = player.transform.position - transform.position; //this will find a difference in one of the x,y, or z values between player and enemy
        direction.y = 0; //ensures goomba doesnt rotate upwards

        //this will rotate the enemy according to the direction vector3, and the lookRotation will pinpoint where to make enemy face
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);

        //animations
        goomba_anim.SetBool("Turning", true);
        yield return new WaitForSeconds(0.8f);
        if (play_Surprise)
        {
            Surprise.Play();
            play_Surprise = false;
        }

        goomba_anim.SetBool("Surprise", true);
        goomba_anim.SetBool("Turning", false);

        //MAD FACE AND STUFF
        yield return new WaitForSeconds(0.8f);

        //chase
        if (Vector3.Distance(player.transform.position, transform.position) < 20 && Vector3.Distance(player.transform.position, transform.position) > 2 && !close)
        {
            renderer.sharedMaterial = mad_face;
            
            if (particle_count < 1)
            {
                ChasePS.Play();
            }
            if (particle_count > 15)
            {
                particle_count = 0;
            }

        }
        yield return new WaitForSeconds(0.25f);


        if (close)//if close enough, the speed of goomba is reduced
        {
            speed = 100;
        }
        else
            speed = 300;

        rb.velocity = transform.TransformDirection(0, rb.velocity.y, speed * Time.deltaTime); //goes in direction thingy is facing in as its positive z value
        goomba_anim.SetBool("Chase", true);
        goomba_anim.SetBool("Surprise", false);

        //if 20 units away..
        if (Vector3.Distance(player.transform.position, transform.position) > 20 || Vector3.Distance(player.transform.position, transform.position) < 1)
        {
            goomba_anim.SetBool("Idle", true);
            goomba_anim.SetBool("Chase", false);
            goomba_anim.SetBool("Surprise", false);
            goomba_anim.SetBool("Turning", false);
            renderer.sharedMaterial = idle_face;
            rb.velocity = new Vector3(0, 0, 0 * Time.deltaTime);
            renderer.sharedMaterial = idle_face;
            play_Surprise = true;
            Surprise.Stop();
        }


    }

    public IEnumerator Dead()
    {
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        transform.GetChild(3).GetComponent<AudioSource>().Play();
        dead = true;
        ChasePS.Stop();
        transform.GetChild(8).gameObject.SetActive(false);//death collider
        StopCoroutine(Chase());
        rb.velocity = new Vector3(0, 0, 0);
        rb.isKinematic = true;
        renderer.sharedMaterial = dead_face;
        goomba_anim.SetBool("Dead", true);
        ChasePS.Stop();
        yield return new WaitForSeconds(1f);
        Instantiate(DestroyPS, transform.position, transform.rotation);
        Vanish.Play();
        Destroy(gameObject);
    }

    public void Stop()
    {
        dead = true;
        StopAllCoroutines();
        renderer.sharedMaterial = dead_face;
        ChasePS.Stop();
        rb.velocity = new Vector3(0, 0, 0);
        transform.rotation = transform.rotation;
        rb.isKinematic = true;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject player = collision.gameObject;
            Rigidbody p_rb = player.GetComponent<Rigidbody>();
            Player player_script = player.GetComponent<Player>();

            if (p_rb.velocity.y < 0.1f && (player.GetComponent<Player>().groundpound == false))
            {
                Hit_Sound.Play();
                StopCoroutine(Chase());
                if (Player.FireMario)//static bool
                {
                    StartCoroutine(player_script.Downgrade_FireSuit());
                }
            }
        }
    }

    public void Play_Stomp_Sound()
    {
        transform.GetChild(4).GetComponent<AudioSource>().Play();
    }


}
