using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubWorldPlayer : MonoBehaviour
{
    public static bool FireMario = false;
    [Header("SUIT")]
    public GameObject[] mario_suit;
    public Material[] RegMaterial;
    public Material[] FireMaterial;

    private Rigidbody rb;

    private Animator player_anim;

    [Header("MovementStuff")]
    public float moveSpeed = 100;
    bool Grounded = true;
    bool jump_bool = false;
    public float JumpForce;
    public float gravityForce;

    public bool entering_level = false;
    public bool entering_level2 = false;
    GameObject LevelColliderWithScript; //the collider for level
    GameObject whichLevelToJump; //the gameobject specific to the level the player is jumping in


    // Start is called before the first frame update
    void Start()
    {
        FireMario = Player.FireMario;
        rb = gameObject.GetComponent<Rigidbody>();
        player_anim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Movements();
        Suit();
        CustomGravity();

        if(entering_level) //basically move towards. 
        {
            JumpInLevel(whichLevelToJump); //the object to move towards which is the gameobject representing the level
        }
        else if (entering_level2)
        {
            JumpInLevel2(whichLevelToJump); //the object to move towards which is the gameobject representing the level
        }
    }

    void Suit()
    {
        if (FireMario)
        {
            mario_suit[0].GetComponent<Renderer>().sharedMaterial = FireMaterial[0];
            mario_suit[1].GetComponent<Renderer>().sharedMaterial = FireMaterial[1];
        }
        if(!FireMario)
        {
            mario_suit[0].GetComponent<Renderer>().sharedMaterial = RegMaterial[0];
            mario_suit[1].GetComponent<Renderer>().sharedMaterial = RegMaterial[1];
        }
    }
    void jump()
    {
        if(Grounded)
        {
            jump_bool = true;
            player_anim.SetBool("Jump", true);
            player_anim.SetBool("Moving", false);
            rb.AddForce(Vector3.up * JumpForce * Time.deltaTime, ForceMode.Impulse);
            Grounded = false;
            Debug.Log("Jump");
        }
    }
    void Movements()
    {
        if(!entering_level)
        {
            //input
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 rotation = new Vector3(-x, 0, -z);


            //controller
            rb.velocity = new Vector3(-x * Time.deltaTime * moveSpeed, rb.velocity.y, -z * Time.deltaTime * moveSpeed);
            //fixes an error i had with player's y rotation
            if (rotation != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(rotation);
            }
            //jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jump();
            }
            //animations
            if (x != 0 || z != 0)
            {
                player_anim.SetBool("Moving", true);
            }
            else
            {
                player_anim.SetBool("Moving", false);
            }
        }

        

        
        
        

    }
    void CustomGravity()
    {
        rb.AddForce(Vector3.down * gravityForce * Time.deltaTime, ForceMode.Acceleration);
    }
    IEnumerator QuestionBlockHit(GameObject block)
    {
        for (int i = 0; i < 3; i++)
        {
            block.transform.position += new Vector3(0, 0.2f, 0);
            block.transform.localScale += new Vector3(0.0003f, 0.0003f, 0.0003f);
            yield return new WaitForSeconds(0.01f);
        } //just moving the block up and down
        for (int i = 0; i < 3; i++)
        {
            block.transform.position += new Vector3(0, -0.2f, 0);
            block.transform.localScale += new Vector3(-0.0003f, -0.0003f, -0.0003f);
            yield return new WaitForSeconds(0.01f);
        }
        block.GetComponent<BoxCollider>().enabled = false;
        block.transform.GetChild(0).gameObject.SetActive(false); // indexing the child objects so we can disable them, leaving only the empty block behind
        block.transform.GetChild(1).gameObject.SetActive(false);
        block.transform.GetChild(2).gameObject.SetActive(true); //enabling empty block
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

    void JumpInLevel(GameObject whichLevel)
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        Vector3 newpos = new Vector3(whichLevel.transform.position.x, transform.position.y, whichLevel.transform.position.z);
        transform.position = Vector3.Lerp(transform.position, newpos, 1.5f * Time.deltaTime);

        Vector3 positionLook = new Vector3(whichLevel.transform.position.x, transform.position.y, whichLevel.transform.position.z);
        transform.LookAt(positionLook);


        Vector3 none = Vector3.zero;

        Camera.main.transform.parent.position = Vector3.SmoothDamp(Camera.main.transform.parent.position, LevelColliderWithScript.GetComponent<WorldMapLevelEnter>().level1Camera.position, ref none, 4 * Time.deltaTime); //interpolate the cam position
        player_anim.SetBool("LevelIn", true);
    }
    void JumpInLevel2(GameObject whichLevel)
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        Vector3 newpos = new Vector3(whichLevel.transform.position.x, transform.position.y, whichLevel.transform.position.z);
        transform.position = Vector3.Lerp(transform.position, newpos, 1.5f * Time.deltaTime);

        Vector3 positionLook = new Vector3(whichLevel.transform.position.x, transform.position.y, whichLevel.transform.position.z);
        transform.LookAt(positionLook);


        Vector3 none = Vector3.zero;

        Camera.main.transform.parent.position = Vector3.SmoothDamp(Camera.main.transform.parent.position, LevelColliderWithScript.GetComponent<WorldMapLevelEnter>().Level2Camera.transform.position, ref none, 4 * Time.deltaTime); //interpolate the cam position
        player_anim.SetBool("LevelIn", true);
    }



    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "ground")
        {
            if(!jump_bool)
            {
                Grounded = true;

                if (rb.velocity.x != 0 || rb.velocity.z != 0)
                {
                    player_anim.SetBool("Moving", true);
                    player_anim.SetBool("Jump", false);
                }
                if (rb.velocity.x == 0 && rb.velocity.z == 0)
                {
                    player_anim.SetBool("Moving", false);
                    player_anim.SetBool("Jump", false);
                }
            } //since onCOllisionStay is called every frame, we need a bool to make sure everything happens once
                
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "ground")
        {
            //when player ends jump
            Grounded = true;
            jump_bool = false;
        }

        if (collision.gameObject.tag == "Question")
        {
            if (rb.velocity.y > -0.2f)
            {
                if (collision.contacts[0].normal.y < -0.8)
                {

                    StartCoroutine(QuestionBlockHit(collision.gameObject));
                }
            }

        }
        if (collision.gameObject.tag == "BrickBlock")
        {
            if (rb.velocity.y > -0.1f)
            {
                if (collision.contacts[0].normal.y < -0.85f)
                {

                    BrickBlockHit(collision.gameObject);
                }
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "ground")
        {
            Grounded = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(Input.GetKeyDown(KeyCode.Space) && other.gameObject.tag == "LEVEL1")
        {
            if(!entering_level)
            {
                entering_level = true;
                LevelColliderWithScript = other.gameObject;
                GameObject Particles = LevelColliderWithScript.GetComponent<WorldMapLevelEnter>().Level1Particles;
                StartCoroutine(other.gameObject.GetComponent<WorldMapLevelEnter>().EnterLevel(Particles));
                rb.AddForce(Vector3.up * JumpForce/5 * Time.deltaTime, ForceMode.Impulse);
                gravityForce = 1000;
                whichLevelToJump = LevelColliderWithScript.GetComponent<WorldMapLevelEnter>().level1; //the
                JumpInLevel(whichLevelToJump);
                Camera.main.transform.parent.GetComponent<HubWorldCamera>().following_Player = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space) && other.gameObject.tag == "LEVEL2")
        {
            if (!entering_level)
            {
                entering_level2 = true;
                LevelColliderWithScript = other.gameObject;
                GameObject Particles = LevelColliderWithScript.GetComponent<WorldMapLevelEnter>().Level2Particles;
                StartCoroutine(other.gameObject.GetComponent<WorldMapLevelEnter>().EnterLevel2(Particles));
                rb.AddForce(Vector3.up * JumpForce / 5 * Time.deltaTime, ForceMode.Impulse);
                gravityForce = 1000;
                whichLevelToJump = LevelColliderWithScript.GetComponent<WorldMapLevelEnter>().level2; //the
                JumpInLevel2(whichLevelToJump);
                Camera.main.transform.parent.GetComponent<HubWorldCamera>().following_Player = false;
            }
        }
    }

}

