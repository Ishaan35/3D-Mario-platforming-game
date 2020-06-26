using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Koopa : MonoBehaviour
{
    private Transform player;
    private Animator anim;
    private Rigidbody rb;

    public GameObject normalFace;
    public GameObject madFace;
    public GameObject shell;
    public float speed;

    private bool close = false;
    private bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim = transform.GetChild(0).GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        chase();
    }

    void chase()
    {
        Ray rayUP = new Ray(transform.position, transform.up); //raycast
        RaycastHit hit;

        if(Mathf.Abs(player.position.x - transform.position.x) < 0.6f && Mathf.Abs(player.position.z - transform.position.z) < 0.3f)
        {
            close = true;
        }
        else
        {
            close = false;
        }

        
        if (Vector3.Distance(player.transform.position, transform.position) < 12 && Mathf.Abs(player.position.y - transform.position.y) < 5)
        {
            if (!close)
            {
                Vector3 direction = player.transform.position - transform.position; //this will find a difference in one of the x,y, or z values between player and enemy
                direction.y = 0;
                //this will rotate the enemy according to the direction vector3, and the lookRotation will pinpoint where to make enemy face

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.5f);

                anim.SetBool("Run", true);
                madFace.SetActive(true);
                normalFace.SetActive(false);

                rb.velocity = transform.TransformDirection(0, rb.velocity.y, speed * Time.deltaTime); //goes in direction thingy is facing in as its positive z value
            }
            else
            {
                anim.SetBool("Run", false);
                madFace.SetActive(false);
                normalFace.SetActive(true);
            }
            

        }
        else
        {
            anim.SetBool("Run", false);
            madFace.SetActive(false);
            normalFace.SetActive(true);
        }

    }

    public void die()
    {
        GameObject psDeath = GameObject.FindGameObjectWithTag("DestroyParticleSystem");
        Instantiate(psDeath, transform.position, transform.rotation);
        GameObject clone = Instantiate(shell, transform.position, shell.transform.rotation);
        clone.SetActive(true);
        Destroy(gameObject);

    }
}
