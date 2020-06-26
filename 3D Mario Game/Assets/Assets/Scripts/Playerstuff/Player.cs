using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string CURRENTLEVEL;
    public Rigidbody rb;
    public float desired_move_speed = 12;
    private float Movespeed;
    Vector3 inputVector; //moving
    Vector3 rotation;
    public GameObject player;
    public Animator player_anim;
    public AnimatorOverrideController player_shell_anim;
    public  bool grounded;
    public float jumpForce;
    public float extra_gravity_if_needed = 0;

    public CapsuleCollider reg_coll;
    public CapsuleCollider crouch_col;

    public WalkDustManager walkdustmanager;

    public bool groundpound = false; //to lock the player controls until this is false while in groundpound
    public Animator cam_shake;

    private AudioSource groundpound_audio;
    public ParticleSystem GroundPoundDust;


    public GameObject wallraydetector;
    bool walljumpbool = false;
    public ParticleSystem WallJumpPS;
    public LayerMask ignoreWalls;
    int particleCount = 0; //trying to make particlesystem play under certain conditions of this number

    //different objects in the question blocks
    public GameObject[] Question_Block_Items;



    //fire mario's suit colours
    public Material[] fire_material;
    //regular mario's suit colours
    public Material[] reg_material;

    //gameobjects in mario's suit
    public GameObject[] mario_suit;

    //sounds effects
    public AudioSource[] Mario_Effects; //0. powerup , 1. Jump1, 2. Jump2, 3.Jump3
    int jump_count = 1;

    //fireball stuff
    public GameObject Fireball;
    public Vector3 velocity;
    public Transform fireball_spawn_loc;
    bool canshoot = true;
    public static bool FireMario = false;
    float shoot_time;

    float punch_time;
    public GameObject PunchDetector;

    public GameObject[] entire_mario_body;

    public bool PipeEntry = false;

    public bool MEGAMUSHROOM = false;
    public Animator mega; //mega Mario
    public Vector3 DESIRED_SCALE = new Vector3(0.7f,0.7f,0.7f);

    public GameObject Coin;
    public GameObject Sprite_1up;
    public GameObject[] ui_GreenStars;

    public GameObject koopaShellHoldPos;
    [HideInInspector]
    public bool holdingShell = false;
    GameObject koopashell;
    [HideInInspector]
    public float koopashellInvincible = 0;



    //level end stuff
    [HideInInspector]
    public bool REACHED_GOAL = false; //when mario first touches flagpole
    [HideInInspector]
    public bool flagpole_end = false; //did mario go all the way down the pole?
    private bool move_down_pole = false;
    GameObject flagpole = null; 
    Transform mario_level_end_position; //where mario moves out of the camera view when level ends
    bool move_out_of_camera = false;
    bool play_flag_sound = true;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        groundpound_audio = GetComponent<AudioSource>();
        grounded = false;

        mario_level_end_position = GameObject.FindGameObjectWithTag("LevelEndPosition").transform;

        FireMario = HubWorldPlayer.FireMario;

    }

    private void FixedUpdate()
    {
       //inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        //camera relative directions based on x and y inputs
        Vector3 XMOVE = Camera.main.transform.right * x;
        Vector3 YMOVE = Camera.main.transform.forward * y;

        //create a single movement vector and multiply speed by movespeed
        inputVector = XMOVE + YMOVE;
        inputVector *= Movespeed;

        //add the movements to the rigidbody. notice i separated inputVector by x, y, z because I dont want any input to mess up the player's y velocity.
        if (!groundpound && !walljumpbool && !PipeEntry && !REACHED_GOAL)
        {
            rb.velocity = new Vector3(inputVector.x, rb.velocity.y, inputVector.z);

        }
        
        //i base player rotation by previously declared inputvector, but i set y to 0 because i dont want to mess up player's y rotation
        if(!walljumpbool && !groundpound && !PipeEntry && !REACHED_GOAL)
            rotation = new Vector3(inputVector.x, 0, inputVector.z);


        //jump
        if (Input.GetKeyDown(KeyCode.Space) && grounded && !PipeEntry && !REACHED_GOAL)
        {

            player_anim.SetBool("Jump", true);
            jump();
            player_anim.SetBool("Moving", false);
            grounded = false;

        }

        if (grounded && !PipeEntry && !REACHED_GOAL)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 && !Input.GetKey(KeyCode.LeftShift)) //move but no crouch
            {
                player_anim.SetBool("CrouchIdle", false);
                player_anim.SetBool("CrouchMove", false);
                player_anim.SetBool("Moving", true);
                if(punch_time > 1)
                    Movespeed = desired_move_speed;
                reg_coll.enabled = true;
                crouch_col.enabled = false;
                if(MEGAMUSHROOM && (rb.velocity.z !=0))
                {
                    cam_shake.SetBool("Shake", true);
                }



            }
            if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0 && !Input.GetKey(KeyCode.LeftShift)) // no move, but no crouch
            {
                player_anim.SetBool("Moving", false);
                player_anim.SetBool("CrouchMove", false);
                player_anim.SetBool("CrouchIdle", false);
                reg_coll.enabled = true;
                crouch_col.enabled = false;
                if (MEGAMUSHROOM)
                {
                    cam_shake.SetBool("Shake", false);
                }


            }

            if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0 && Input.GetKey(KeyCode.LeftShift) && !holdingShell) //no move, and crouch
            {
                player_anim.SetBool("CrouchIdle", true);
                player_anim.SetBool("CrouchMove", false);
                player_anim.SetBool("Moving", false);
                reg_coll.enabled = false;
                crouch_col.enabled = true;
                if (MEGAMUSHROOM)
                {
                    cam_shake.SetBool("Shake", false);
                }



            }
            if ((Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && Input.GetKey(KeyCode.LeftShift) && !holdingShell) //move and crouch
            {
                player_anim.SetBool("CrouchIdle", false);
                player_anim.SetBool("CrouchMove", true);
                player_anim.SetBool("Moving", true);
                Movespeed = 2.5f;
                reg_coll.enabled = false;
                crouch_col.enabled = true;
                if (MEGAMUSHROOM)
                {
                    cam_shake.SetBool("Shake", true);
                }


            }
            
            if (Input.GetAxis("Horizontal") != 0 && !Input.GetKey(KeyCode.LeftShift)) //no crouch, move
            {
                player_anim.SetBool("CrouchIdle", false);
                player_anim.SetBool("CrouchMove", false);
                player_anim.SetBool("Moving", true);
                if(punch_time > 1)
                    Movespeed = desired_move_speed;
                reg_coll.enabled = true;
                crouch_col.enabled = false;
                if (MEGAMUSHROOM)
                {
                    cam_shake.SetBool("Shake", true);
                }

            }
        } //player crouch stuff and movement animation

        //groundpound
        if (!grounded && !PipeEntry && !holdingShell)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                StartCoroutine(GroundPound());
            }
        }


        //fixes an error i had with player's y rotation
        if (rotation != Vector3.zero)
        {
            player.transform.rotation = Quaternion.LookRotation(rotation);
        }

        if(!REACHED_GOAL)
            Walljump();


        //fireball stuff
        shoot_time = shoot_time + 0.05f;
        if(shoot_time > 2)
        {
            StartCoroutine(Shoot_Fireball());
        }

        //punch stuff
        punch_time += 0.05f;
        if(punch_time > 1.5f && !REACHED_GOAL)
        {
           punch();
        }
        

        //coin reset
        if(CoinCollect.COIN_COUNT > 99)
        {
            Lives.LIVES++;
            CoinCollect.COIN_COUNT = 0;
            Sprite_1up.GetComponent<ParticleSystem>().Play();
            Sprite_1up.GetComponent<AudioSource>().Play();
        }

        //megamushroom scale
        if (MEGAMUSHROOM)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(2.5f, 2.5f, 2.5f), 1f * Time.deltaTime);
        }
        if (!MEGAMUSHROOM)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, DESIRED_SCALE, 2.5f * Time.deltaTime);
        }


        if(REACHED_GOAL)
        {
            if(!flagpole_end && move_down_pole)
            {
                Invoke("flag_move_down", 1); //call method after 1 second
            }
            if(move_down_pole)
            {
                Vector3 position = new Vector3(flagpole.transform.position.x, transform.position.y, flagpole.transform.position.z);
                if(!move_out_of_camera)
                    transform.LookAt(position); //player should face pole
            }
        }
        if(move_out_of_camera)
        {
            //Vector3 distance_to_move = mario_level_end_position.position - transform.position;
            Vector3 distance_to_move = new Vector3(mario_level_end_position.position.x - transform.position.x, -25, mario_level_end_position.position.z - transform.position.z);
            rb.velocity = distance_to_move * Time.deltaTime * 25;
            Vector3 where_to_look = mario_level_end_position.position;
            where_to_look = new Vector3(where_to_look.x, transform.position.y, where_to_look.z);
            transform.LookAt(where_to_look);
        }

        if(FireMario)
        {
            CorrectSuitOnStart();
        }
        if(!groundpound)
            rb.AddForce(Vector3.down * extra_gravity_if_needed, ForceMode.Acceleration);

        //koopa shell stuff
        if (Input.GetMouseButtonUp(1))
        {
            ThrowShell();
        }
        koopashellInvincible += Time.deltaTime;


    }

   



    void jump()
    {
        rb.AddForce(player.transform.up * jumpForce);
        walkdustmanager.Walkdust.Stop();
        Mario_Effects[jump_count].Play();
        jump_count++;

        if (jump_count > 3)
            jump_count = 1;
    }
    void punch()
    {
        /*
        if(Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftShift) && grounded)
        {
            player_anim.SetTrigger("Punch");
            Movespeed = 6;
            punch_time = 0;

            //shoot a ray forward
            Ray check_punch = new Ray(PunchDetector.transform.position, PunchDetector.transform.forward); //raycast
            RaycastHit hit;



            if (Physics.Raycast(check_punch, out hit, 2f))//object at 2 units away
            {
                if(hit.transform.gameObject.tag == "GoombaEnemy")
                {
                    GameObject goomba = hit.transform.gameObject;
                    goomba.gameObject.GetComponent<GoombaChase>().Stop();
                    goomba.gameObject.GetComponent<GoombaChase>().enabled = false;
                    goomba.gameObject.GetComponent<Animator>().SetBool("Knockout", true);
                    goomba.gameObject.transform.GetChild(8).gameObject.SetActive(false);
                    goomba.gameObject.GetComponent<CapsuleCollider>().enabled = false;
                    StartCoroutine(Fireball.GetComponent<Fireball>().DestroyGoomba(goomba));
                }
                if(hit.transform.gameObject.tag == "Crate")
                {
                    GameObject Crate = hit.transform.gameObject;
                    StartCoroutine(Crate.GetComponent<Crate>().Destroy_Punch());
 
                }
            }
                            
        }
        */
    }
    IEnumerator GroundPound()
    {
        RaycastHit hit; //declare a raycast hit detector
        Ray downRay = new Ray(transform.position, -Vector3.up); //shoot a raycast downward

        Physics.Raycast(downRay, out hit); //tells unity if the downray hit something, and transfers result into hit.

         
        if(hit.distance > 1.5f) //if player is at desired height
        {
            grounded = false;
            groundpound = true;

            //stopping stuff from walljump
            wallraydetector.SetActive(false); //stops all of the methods and code in the wallump, as there is no raycast being emitted
            //StopCoroutine(WallJump());
            rb.drag = 0;

            player_anim.SetBool("GroundPound", true);
            player_anim.SetBool("Jump", false);
            rb.velocity = new Vector3(0, 0, 0); //freeze
            rb.useGravity = false; //no external forces
            yield return new WaitForSeconds(0.5f);
            rb.velocity = new Vector3(0, -30 - extra_gravity_if_needed, 0);
            rb.mass = 100; //so mario doesnt move randomly when groundpounding
            while (!grounded)
            {
                yield return new WaitForSeconds(0.001f); //a way to pause the function
            }
            cam_shake.SetBool("Shake", true); //cam shake animation
            GroundPoundDust.Play();
            groundpound_audio.Play();
            yield return new WaitForSeconds(0.1f);
            cam_shake.SetBool("Shake", false);

            groundpound = false; //reset all modifications
            rb.useGravity = true;
            rb.isKinematic = true;
            yield return new WaitForSeconds(0.4f);
            player_anim.SetBool("GroundPound", false);
            yield return new WaitForSeconds(0.1f);
            rb.isKinematic = false;
            rb.mass = 1;
            wallraydetector.SetActive(true);
            
        }
       //right,forward negative contact point normal
    }
    void Walljump()
    { 
        int rotatedirectionX = 0; //these values will either be -1 or 1 to later multiply the directions, so you can be opposite direction or correct direction, as 1 and -1 just create the opposite number when multplied
        int rotatedirectionZ = 0;

        //used to detect if player facing wall
        Ray wall = new Ray(wallraydetector.transform.position, wallraydetector.transform.forward); //raycasr
        RaycastHit hit;

         

        //used to see if player is off the ground, before walljumping
        RaycastHit hitdown; //declare a raycast hit detector
        Ray downRay = new Ray(transform.position, -Vector3.up); //shoot a raycast downward
        Physics.Raycast(downRay, out hitdown); //tells unity if the downray hit something, and transfers result into hit.

        bool offground = false;
        if (hitdown.distance > 0.6f)
        {
            offground = true;
        }

        particleCount++;
        if (particleCount > 4) //since this function is being called repeatedly, we want to play the particle system every 4 frames
        {
            WallJumpPS.Play();
            particleCount = 0;
        }


        if (Physics.Raycast(wall, out hit, 0.7f, ignoreWalls) && hit.normal.y < 0.05 && offground && rb.velocity.y <=0) //if raycast hits something closer than 0.7 from player, with steepness normal of y being 0.2 or less (vertical). the ignoreWalls will check if the object layer the raycast hits is valid. 
        {
            Debug.Log(hit.distance);
            rb.drag = 5;
            player_anim.SetBool("WallJumpLeft", true);

            //identify direction that player bounced off of            
            if (hit.normal.x > 0.05)
            {
                rotatedirectionX = 1; //right
                player_anim.SetBool("WallJumpLeft", true);
                player_anim.SetBool("Jump", false);


            }
            else if (hit.normal.x < -0.05)
            {
                rotatedirectionX = -1;//left
                player_anim.SetBool("WallJumpLeft", true);
                player_anim.SetBool("Jump", false);

            }
            else
                rotatedirectionX = 0;

            if (hit.normal.z > 0.1)
            {
                rotatedirectionZ = 1;//forward
                player_anim.SetBool("WallJumpLeft", true);
                player_anim.SetBool("Jump", false);
            }
            else if (hit.normal.z < -0.1)
            {
                player_anim.SetBool("WallJumpLeft", true);
                player_anim.SetBool("Jump", false);
                rotatedirectionZ = -1;//backward

            }
            else
            {
                player_anim.SetBool("WallJumpLeft", true);
                rotatedirectionZ = 0;

            }



            if (!grounded && Physics.Raycast(wall, out hit, 0.7f) && hit.normal.y < 0.2 && Input.GetKeyDown(KeyCode.Space))
            {
                walljumpbool = true;
                //rotation += new Vector3(270 * rotatedirectionX, 0, 270 * rotatedirectionZ); //modify direction with -1 and 1 
                rotation += new Vector3(180 * rotatedirectionX, 0, 180 * rotatedirectionZ);
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);//set to 0 and then change on next line, so we always stabilize the vlelocity increase
                rb.velocity = new Vector3(hit.normal.x * 8, rb.velocity.y + 20, hit.normal.z * 7); //bounce off to direction of normal

                player_anim.SetBool("WallJumpLeft", false);
                player_anim.SetBool("Jump", true);
                //sounds
                Mario_Effects[jump_count].Play();
                jump_count++;

                if (jump_count > 3)
                    jump_count = 1;

            }
           
        }
        else
        {
            player_anim.SetBool("WallJumpLeft", false);
            rb.drag = 0;
            WallJumpPS.Stop();


        }


    }
    IEnumerator Shoot_Fireball()
    {
        if (Input.GetMouseButtonDown(0) && canshoot && FireMario)
        {
            shoot_time = 0;
            player_anim.SetTrigger("Shoot");
            player_anim.SetBool("Jump", false);
            yield return new WaitForSeconds(0.1f);
            Mario_Effects[5].Play();
            GameObject Clone = Instantiate(Fireball, new Vector3(fireball_spawn_loc.position.x, fireball_spawn_loc.position.y, fireball_spawn_loc.position.z), Fireball.transform.rotation);
            Clone.GetComponent<Fireball>().enabled = true;
            
            Clone.GetComponent<Rigidbody>().velocity = transform.TransformDirection (velocity.x, velocity.y, velocity.z);//speed
            yield return new WaitForSeconds(10);
            StartCoroutine(Clone.GetComponent<Fireball>().Destroy());
        }

    }
    IEnumerator QuestionBlockHit(GameObject block, int direction)
    {
        block.transform.parent.gameObject.GetComponent<AudioSource>().Play(); //play the sound effect attached to empty parent object


        if(block.gameObject.GetComponent<QuestionBlockID>().ITEM_ID == 1)
        {
            Camera.main.transform.GetChild(1).GetComponent<AudioSource>().Play();//item gameobject under camera
            GameObject Clone = Instantiate(Question_Block_Items[1], block.transform.position, block.transform.rotation);
            Clone.transform.GetComponentInChildren<CapsuleCollider>().enabled = false;
            Clone.transform.GetComponentInChildren<Animator>().SetBool("Spawn", true);
        }
        for (int i = 0; i < 4; i++)
        {
            block.transform.position += new Vector3(0, 0.2f * direction, 0);
            block.transform.localScale += new Vector3(0.0003f, 0.0003f, 0.0003f);
            yield return new WaitForSeconds(0.006f);
        } //just moving the block up and down
        for (int i = 0; i < 4; i++)
        {
            block.transform.position += new Vector3(0, -0.2f * direction, 0);
            block.transform.localScale += new Vector3(-0.0003f, -0.0003f, -0.0003f);
            yield return new WaitForSeconds(0.006f);
        }
        

        block.GetComponent<BoxCollider>().enabled = false;
        block.transform.GetChild(0).gameObject.SetActive(false); // indexing the child objects so we can disable them, leaving only the empty block behind
        block.transform.GetChild(1).gameObject.SetActive(false);
        block.transform.GetChild(2).gameObject.SetActive(true); //enabling empty block

        if (block.gameObject.GetComponent<QuestionBlockID>().ITEM_ID == 0)
        {
            Vector3 newOffset = new Vector3(0, 0.5f, 0);

            GameObject clone = Instantiate(Question_Block_Items[0], block.transform.position + newOffset, Question_Block_Items[0].transform.rotation);
            clone.GetComponent<SphereCollider>().enabled = false;
            clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            clone.GetComponent<Rigidbody>().useGravity = true;
            clone.GetComponent<Rigidbody>().AddForce(Vector3.up * 450 * Time.deltaTime, ForceMode.Impulse);
            yield return new WaitForSeconds(0.2f);

            clone.GetComponent<Rigidbody>().useGravity = false;
            clone.gameObject.GetComponent<AudioSource>().Play();
            clone.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            clone.gameObject.GetComponent<ParticleSystem>().Play();
            clone.gameObject.GetComponent<Animator>().enabled = false;
            Destroy(clone, 1);
            CoinCollect.COIN_COUNT++;
        }

    }
    void BrickBlockHit(GameObject brick)
    {
        brick.gameObject.GetComponent<AudioSource>().Play();


        {
            for (int i = 0; i < 9; i++)
            {
                brick.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
            }
            brick.GetComponent<MeshRenderer>().enabled = false;
            brick.GetComponent<BoxCollider>().enabled = false;
            Destroy(brick, 2);
        }
    }
    



    IEnumerator OnCollisionEnter(Collision other)
    {
        walljumpbool = false;

        if (other.gameObject.tag == "ground"||  other.gameObject.tag == "CurveGround" || other.contacts[0].normal.y < 1.3f && other.contacts[0].normal.y > 0.7f) //last one is any other flat surface
        {
            player_anim.SetBool("Jump", false);
            player_anim.SetBool("WallJumpLeft", false);
            player_anim.SetBool("Jump", false);

            rb.drag = 0;
            if(other.contacts[0].normal.y < 1.3f && other.contacts[0].normal.y > 0.7f || other.gameObject.tag == "CurveGround")
            {
                grounded = true;
                player_anim.SetBool("Jump", false);  
                if(MEGAMUSHROOM)
                {
                    Camera.main.GetComponent<Animator>().SetBool("Shake", true);
                    yield return new WaitForSeconds(0.2f);
                    Camera.main.GetComponent<Animator>().SetBool("Shake", false);

                }
            }
        }
        if(other.gameObject.tag == "Question")
        {
            if(rb.velocity.y > -0.2f)
            {
                if(other.contacts[0].normal.y < -0.8)
                {
                   
                    StartCoroutine(QuestionBlockHit(other.gameObject, 1));
                }
            }
            if(groundpound && other.contacts[0].normal.y >= 0.85)
            {
                StartCoroutine(QuestionBlockHit(other.gameObject, -1));
            }
            if(MEGAMUSHROOM)
            {
                Destroy(other.gameObject);
            }
        }
        if(other.gameObject.tag == "BrickBlock")
        {
            if (rb.velocity.y > -0.1f)
            {
                if (other.contacts[0].normal.y < -0.85f)
                {

                    BrickBlockHit(other.gameObject);
                }
            }
            if(MEGAMUSHROOM)
            {
                BrickBlockHit(other.gameObject);
            }
            
        }
        if(other.gameObject.tag == "Crate")
        {
            if(MEGAMUSHROOM)
            {
                StartCoroutine(other.gameObject.GetComponent<Crate>().Destroy_GroundPound());
            }
        }
        if(other.gameObject.tag == "GoombaEnemy" && MEGAMUSHROOM)
        {
            other.gameObject.GetComponent<GoombaChase>().Stop();
            other.gameObject.GetComponent<GoombaChase>().enabled = false;
            other.gameObject.GetComponent<Animator>().SetBool("Knockout", true);
            other.gameObject.transform.GetChild(8).gameObject.SetActive(false);
            other.gameObject.GetComponent<CapsuleCollider>().enabled = false;
            other.gameObject.transform.GetChild(5).GetComponent<AudioSource>().Play(); //knockout sound
            StartCoroutine(DestroyGoomba(other.gameObject));
           
        }
        if(other.gameObject.tag == "MegaGoomba" && MEGAMUSHROOM)
        {
            Debug.Log("hewg");
            other.gameObject.GetComponent<MegaGoomba>().Stop();
            other.gameObject.GetComponent<MegaGoomba>().enabled = false;
            other.gameObject.GetComponent<Animator>().SetBool("Knockout", true);
            other.gameObject.transform.GetChild(8).gameObject.SetActive(false);
            other.gameObject.GetComponent<CapsuleCollider>().enabled = false;
            other.gameObject.transform.GetChild(5).GetComponent<AudioSource>().Play(); //knockout sound
            StartCoroutine(DestroyMegaGoomba(other.gameObject));
        }
        if (other.gameObject.tag == "Tree" && MEGAMUSHROOM)
        {
            Vector3 direction = other.contacts[0].point - transform.position; //angle of collision
            StartCoroutine(DestroyTree(other.gameObject, direction));
        }

        if (other.gameObject.tag == "KoopaShell")
        {
            if(!other.gameObject.GetComponent<KoopaShell>().moving && !Input.GetMouseButton(1))
            {
                koopashellInvincible = 0;
                float force = 1700;

                Vector3 dir = other.contacts[0].point - transform.position;
                dir.Normalize();

                Vector3 shellVel = dir * force * Time.deltaTime;
                shellVel.y = other.gameObject.GetComponent<Rigidbody>().velocity.y;

                other.gameObject.GetComponent<KoopaShell>().velocity = shellVel;
                other.gameObject.GetComponent<KoopaShell>().moving = true;
                other.transform.GetChild(0).GetComponent<Animator>().SetBool("Spin", true);
            }
            else if(!other.gameObject.GetComponent<KoopaShell>().moving && Input.GetMouseButton(1))
            {
                other.gameObject.GetComponent<KoopaShell>().holdPos = koopaShellHoldPos.transform;
                other.gameObject.GetComponent<KoopaShell>().heldByPlayer = true;
                holdingShell = true;
                koopaShellHoldPos.GetComponent<SphereCollider>().enabled = true;
                koopashell = other.gameObject;
                player_anim.runtimeAnimatorController = player_shell_anim;
            }

        }

    }

    IEnumerator OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "ground" || other.gameObject.tag == "CurveGround" || other.contacts[0].normal.y < 1.3f && other.contacts[0].normal.y > 0.7f) //last one is any other flat surface
        {
            player_anim.SetBool("WallJumpLeft", false);

            rb.drag = 0;
            if (other.contacts[0].normal.y < 1.3f && other.contacts[0].normal.y > 0.7f || other.gameObject.tag == "CurveGround")
            {
                if (!Input.GetKeyDown(KeyCode.Space))
                {
                    grounded = true;
                    player_anim.SetBool("Jump", false);
                }

                if (MEGAMUSHROOM)
                {
                    Camera.main.GetComponent<Animator>().SetBool("Shake", true);
                    yield return new WaitForSeconds(0.2f);
                    Camera.main.GetComponent<Animator>().SetBool("Shake", false);

                }
            }
        }
    }



    IEnumerator OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Fireflower")
        {
            if(!FireMario)
                StartCoroutine(FireSuit(other.gameObject));
            if(FireMario)
            {
                Destroy(other.gameObject);
            }
            FireMario = true;
        }
        if (other.gameObject.tag == "GoombaDeath" && rb.velocity.y < 0.5)
        {
            if(groundpound)
            {
                StartCoroutine(other.gameObject.GetComponentInParent<GoombaChase>().Dead());
                
            }
            if (!groundpound)
            {
                StartCoroutine(other.gameObject.GetComponentInParent<GoombaChase>().Dead());
                rb.AddForce(transform.up * 1500);
                player_anim.Play("Jump", -1,0); //unity forums, i searched for an answer on how to repeat an animation you are already on

                yield return new WaitForSeconds(0.5f);
                GameObject clone = Instantiate(Coin, other.transform.position, Coin.transform.rotation);
                clone.GetComponent<SphereCollider>().enabled = false;
                clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                clone.GetComponent<Rigidbody>().useGravity = true;
                clone.GetComponent<Rigidbody>().AddForce(Vector3.up *300 * Time.deltaTime, ForceMode.Impulse);
                yield return new WaitForSeconds(0.2f);

                Coin.GetComponent<Rigidbody>().useGravity = false;
                clone.gameObject.GetComponent<AudioSource>().Play();
                clone.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                clone.gameObject.GetComponent<ParticleSystem>().Play();
                clone.gameObject.GetComponent<Animator>().enabled = false;
                Destroy(clone, 1);
                CoinCollect.COIN_COUNT++;

            }
        }
        if(other.gameObject.tag == "Crate")
        {
            if(groundpound)
            {
                GameObject Crate = other.transform.parent.gameObject;//groundpound collider is a child object of actual crate
                StartCoroutine(Crate.GetComponent<Crate>().Destroy_GroundPound());
                
            }
        }
        if(other.gameObject.tag == "BrickBlock")
        {
            if(groundpound)
            {
                BrickBlockHit(other.transform.parent.gameObject);
                cam_shake.SetBool("Shake", true);
                rb.velocity = new Vector3(0, 0, 0);
                yield return new WaitForSeconds(0.05f);
                rb.velocity = new Vector3(0, -30, 0);
            }
        }
        if(other.gameObject.tag == "Coins")
        {
            other.gameObject.GetComponent<AudioSource>().Play();
            other.gameObject.GetComponent<SphereCollider>().enabled = false;
            other.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            other.gameObject.GetComponent<ParticleSystem>().Play();
            other.gameObject.GetComponent<Animator>().enabled = false;
            Destroy(other.gameObject, 1);
            CoinCollect.COIN_COUNT++;
        }
        if(other.gameObject.tag == "MegaMushroom")
        {
            Mario_Effects[7].Play();//grow sound
            Mario_Effects[6].Play();//item sound
            Mario_Effects[8].Play();//music sound

            //cannot be firemario and megamushroom at same time
            Renderer suit_1 = mario_suit[0].GetComponent<Renderer>();
            Renderer suit_2 = mario_suit[1].GetComponent<Renderer>();
            suit_1.sharedMaterial = reg_material[0];
            suit_2.sharedMaterial = reg_material[1];
            FireMario = false;

            MEGAMUSHROOM = true;
            float volume = Camera.main.GetComponent<AudioSource>().volume;
            Camera.main.GetComponent<AudioSource>().volume = 0;
            Destroy(other.gameObject);

            RuntimeAnimatorController regular_mario = player_anim.runtimeAnimatorController; //create temporary variable to hold the animator vaeiable
            player_anim.runtimeAnimatorController = mega.runtimeAnimatorController;

            yield return new WaitForSeconds(19);
            Mario_Effects[8].Stop();
            Camera.main.GetComponent<AudioSource>().volume = volume;
            MEGAMUSHROOM = false;
            player_anim.runtimeAnimatorController = regular_mario;
            cam_shake.SetBool("Shake", false);



            
        }
        if(other.gameObject.tag == "1UP Mushroom")
        {
            Sprite_1up.GetComponent<ParticleSystem>().Play();
            Sprite_1up.GetComponent<AudioSource>().Play();
            Destroy(other.gameObject);
            Lives.LIVES++;
        }
        if(other.gameObject.tag == "GreenStar")
        {
            GreenStarID greenstar = other.gameObject.GetComponent<GreenStarID>();
            int id = greenstar.ID;

            ui_GreenStars[id].SetActive(true);
            other.gameObject.GetComponent<Animator>().SetBool("Collected", true);
            other.GetComponent<AudioSource>().Play();
            other.gameObject.GetComponent<SphereCollider>().enabled = false;

        }

        if(other.gameObject.tag == "Koopa")
        {
            other.gameObject.GetComponent<Koopa>().die();
            rb.velocity = Vector3.zero;
            for(int i = 0; i < 60; i++)
            {
                rb.AddForce(Vector3.up * 15 * Time.deltaTime, ForceMode.Impulse);
            }
        }
        
            //game end
            if (other.gameObject.tag == "Flagpole")
        {
            REACHED_GOAL = true;
            rb.isKinematic = true;
            flagpole = other.gameObject;
            Vector3 position = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
            move_down_pole = true;
            player_anim.SetBool("Goal", true);
            other.gameObject.GetComponent<AudioSource>().Play();
            Camera.main.GetComponent<AudioSource>().Stop();

        }
        //this is for making mario stop moving down the flagpole
        if (other.gameObject.tag == "FlagpoleEnd")
        {
            flagpole_end = true;
            flagpole = other.transform.parent.gameObject;
            flagpole.GetComponent<CapsuleCollider>().enabled = false; //so it does not get in the way of mario making his finale
        }

    }

    void CorrectSuitOnStart()
    {
        Renderer suit_1 = mario_suit[0].GetComponent<Renderer>();
        Renderer suit_2 = mario_suit[1].GetComponent<Renderer>();
        suit_1.sharedMaterial = fire_material[0];
        suit_2.sharedMaterial = fire_material[1];
    }


    //normal to firesuit
    IEnumerator FireSuit(GameObject other)
    {
        Mario_Effects[6].Play(); //picking up poweup
        Mario_Effects[0].Play(); //audio powerup
        Destroy(other.gameObject);

        //getting the rendererers in the gameobjects
        Renderer suit_1 = mario_suit[0].GetComponent<Renderer>();
        Renderer suit_2 = mario_suit[1].GetComponent<Renderer>();

        //speed of time we want
        Time.timeScale = 0.08f;
        //create a float that is 0.1 more than the real time, and later execute loop until the time reaches this float
        float pauseEndTime = Time.realtimeSinceStartup + 0.2f; //unity documents show that this works independently of timescale, so I use this to freeze the game to get powerup effect

        while (Time.realtimeSinceStartup < pauseEndTime) //suit change effect
        {
            suit_1.sharedMaterial = fire_material[0];
            suit_2.sharedMaterial = fire_material[1];

            yield return new WaitForSeconds(0.01f);

            suit_1.sharedMaterial = reg_material[0];
            suit_2.sharedMaterial = reg_material[1];

            yield return new WaitForSeconds(0.01f);

            suit_1.sharedMaterial = fire_material[0];
            suit_2.sharedMaterial = fire_material[1];

            yield return new WaitForSeconds(0.01f);

            suit_1.sharedMaterial = reg_material[0];
            suit_2.sharedMaterial = reg_material[1];

            yield return new WaitForSeconds(0.01f);

            suit_1.sharedMaterial = fire_material[0];
            suit_2.sharedMaterial = fire_material[1];

        }
        Time.timeScale = 1;     
    }
    //firesuit to normal
    public IEnumerator Downgrade_FireSuit()
    {
        Mario_Effects[4].Play();
        FireMario = false;
        //getting the rendererers in the gameobjects
        Renderer suit_1 = mario_suit[0].GetComponent<Renderer>();
        Renderer suit_2 = mario_suit[1].GetComponent<Renderer>();

        //speed of time we want
        Time.timeScale = 0.04f;
        //create a float that is 0.1 more than the real time, and later execute loop until the time reaches this float
        float pauseEndTime = Time.realtimeSinceStartup + 0.2f; //unity documents show that this works independently of timescale, so I use this to freeze the game to get powerup effect

        while (Time.realtimeSinceStartup < pauseEndTime) //suit change effect
        {
            suit_1.sharedMaterial = reg_material[0];
            suit_2.sharedMaterial = reg_material[1];

            yield return new WaitForSeconds(0.005f);

            suit_1.sharedMaterial = fire_material[0];
            suit_2.sharedMaterial = fire_material[1];

            yield return new WaitForSeconds(0.005f);

            suit_1.sharedMaterial = reg_material[0];
            suit_2.sharedMaterial = reg_material[1];

            yield return new WaitForSeconds(0.005f);

            suit_1.sharedMaterial = fire_material[0];
            suit_2.sharedMaterial = fire_material[1];

            yield return new WaitForSeconds(0.005f);

            suit_1.sharedMaterial = reg_material[0];
            suit_2.sharedMaterial = reg_material[1];

        }
        Time.timeScale = 1;
        StartCoroutine(FlickerEffect());
    }   


    //if hit by something, there will be flicker effect
    public IEnumerator FlickerEffect()
    {
        Physics.IgnoreLayerCollision(9, 13, true);
        Physics.IgnoreLayerCollision(9, 21, true);
        for (int i = 0; i < 15; i++)
        {
           
            entire_mario_body[0].GetComponent<SkinnedMeshRenderer>().enabled = false;
            entire_mario_body[1].GetComponent<SkinnedMeshRenderer>().enabled = false;
            entire_mario_body[2].GetComponent<SkinnedMeshRenderer>().enabled = false;
            entire_mario_body[3].GetComponent<SkinnedMeshRenderer>().enabled = false;
            entire_mario_body[4].GetComponent<SkinnedMeshRenderer>().enabled = false;

            yield return new WaitForSeconds(0.05f);

            entire_mario_body[0].GetComponent<SkinnedMeshRenderer>().enabled = true;
            entire_mario_body[1].GetComponent<SkinnedMeshRenderer>().enabled = true;
            entire_mario_body[2].GetComponent<SkinnedMeshRenderer>().enabled = true;
            entire_mario_body[3].GetComponent<SkinnedMeshRenderer>().enabled = true;
            entire_mario_body[4].GetComponent<SkinnedMeshRenderer>().enabled = true;

            yield return new WaitForSeconds(0.05f);
        }
        Physics.IgnoreLayerCollision(9, 13, false);
        Physics.IgnoreLayerCollision(9, 21, false);
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
    }



    IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);

    }
    IEnumerator Pause(float time)
    {
        yield return new WaitForSeconds(time);
    }


    public IEnumerator DestroyGoomba(GameObject Goomba)
    {
        yield return new WaitForSeconds(0.4f);
        GameObject DeathPS = Goomba.GetComponent<GoombaChase>().DestroyPS;
        Vector3 position = new Vector3(Goomba.transform.position.x, Goomba.transform.position.y, Goomba.transform.position.z);
        Instantiate(DeathPS, position, DeathPS.transform.rotation);
        Goomba.GetComponent<GoombaChase>().Vanish.Play();
        Destroy(Goomba);
    }
    public IEnumerator DestroyMegaGoomba(GameObject Goomba)
    {
        yield return new WaitForSeconds(0.4f);
        GameObject DeathPS = Goomba.GetComponent<MegaGoomba>().DestroyPS;
        Vector3 position = new Vector3(Goomba.transform.position.x, Goomba.transform.position.y, Goomba.transform.position.z);
        Instantiate(DeathPS, position, DeathPS.transform.rotation);
        Goomba.GetComponent<MegaGoomba>().Vanish.Play();
        Destroy(Goomba);
    }
    IEnumerator DestroyTree(GameObject tree, Vector3 direction)
    {
        tree.GetComponent<CapsuleCollider>().enabled = false;

        for (int i = 0; i < 15; i++)
        {
            direction.y /= 1.2f;
            tree.transform.Translate(direction * Time.deltaTime * 9);
            tree.GetComponent<CapsuleCollider>().enabled = false;
            yield return new WaitForSeconds(0.000f);
        }
        tree.transform.GetChild(5).GetComponent<MeshRenderer>().enabled = false;
        tree.transform.GetChild(6).GetComponent<MeshRenderer>().enabled = false;
        for (int i = 0; i < 5; i++)
        {
            tree.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
        }
        Destroy(tree, 1);
    }


    //end level functions
    void flag_move_down()
    {
        if(!flagpole_end && move_down_pole)
        {
            transform.Translate(0, -8 * Time.deltaTime, 0);
            if (play_flag_sound)
            {
                Mario_Effects[10].Play();
                play_flag_sound = false;
            }
        }
        if (flagpole_end)
            StartCoroutine(MarioFinale());
    }
    IEnumerator MarioFinale()
    {
        yield return new WaitForSeconds(0.75f);
        rb.isKinematic = false;
        player_anim.SetBool("Goal", false);
        player_anim.SetBool("EndLevel", true);
        move_out_of_camera = true;
        yield return new WaitForSeconds(1.5f);
        Camera.main.transform.parent.GetComponent<CameraFollow>().CameraLevelEndMovement = true;

        GameObject manager = GameObject.FindGameObjectWithTag("SceneManager");
        sceneManage manage_script = manager.GetComponent<sceneManage>();

        if (CURRENTLEVEL.Equals("Level1"))
        {
            WorldMapLevelEnter.Level1Complete = true;
        }
        else if(CURRENTLEVEL.Equals("Level2"))
        {
            WorldMapLevelEnter.Level2Complete = true;
        }
        yield return new WaitForSeconds(6);
        StartCoroutine(manage_script.levelToWorldMap());
        
    }

    void ThrowShell()
    {
        
        if (holdingShell && koopashell != null)
        {
            holdingShell = false;
            koopashellInvincible = 0;
            koopashell.GetComponent<Rigidbody>().isKinematic = false;
            koopashell.GetComponent<KoopaShell>().heldByPlayer = false;
            koopashell.GetComponent<KoopaShell>().moving = true;

            float force = 1700;

            Vector3 dir = koopashell.transform.position - transform.position;
            dir.Normalize();

            Vector3 shellVel = dir * force * Time.deltaTime;
            shellVel.y = koopashell.GetComponent<Rigidbody>().velocity.y;

            koopashell.gameObject.GetComponent<KoopaShell>().velocity = shellVel;
            koopashell.gameObject.GetComponent<KoopaShell>().moving = true;
            koopashell.transform.GetChild(0).GetComponent<Animator>().SetBool("Spin", true);
            koopaShellHoldPos.GetComponent<SphereCollider>().enabled = false;

            koopashell.GetComponent<SphereCollider>().enabled = true;

            player_anim.runtimeAnimatorController = new AnimatorOverrideController(player_anim.runtimeAnimatorController);





        }
    }

}
