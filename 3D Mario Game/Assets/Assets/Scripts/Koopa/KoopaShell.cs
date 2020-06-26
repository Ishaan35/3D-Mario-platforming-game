using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoopaShell : MonoBehaviour
{
    private Rigidbody rb;

    [HideInInspector]
    public Vector3 velocity = Vector3.zero;
    public bool moving;

    [HideInInspector]
    public bool heldByPlayer = false;
    [HideInInspector]
    public Transform holdPos;
    public AudioSource Hit_Sound;


    public ParticleSystem dustps;
    public ParticleSystem spark;
    private float particleDelay;

    public GameObject coin;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (moving && !heldByPlayer)
        {
            rb.isKinematic = false;
            GetComponent<SphereCollider>().enabled = true;

            Vector3 oldVel = velocity;
            oldVel = oldVel.normalized;
            oldVel *= 1700 * Time.deltaTime;

            velocity = oldVel;

            velocity.y = rb.velocity.y;
            rb.AddForce(Vector3.down * 25 * Time.deltaTime, ForceMode.Acceleration);
            rb.velocity = velocity;

            if (particleDelay > 0.05f && moving)
            {
                dustps.Play();
                particleDelay = 0;
            }


        }

        if (heldByPlayer)
        {
            transform.position = holdPos.position;
            transform.rotation = holdPos.rotation;
            rb.isKinematic = true;
            GetComponent<SphereCollider>().enabled = false;
        }


        particleDelay += Time.deltaTime;
        


    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "ground" && collision.gameObject.tag != "Player" && collision.gameObject.tag != "Fireball" && collision.gameObject.tag != "GoombaEnemy" && collision.contacts[0].normal.y < 0.3f)
        {
            Vector3 oldVel = velocity;
            oldVel = oldVel.normalized;
            oldVel *= 1700 * Time.deltaTime;

            Vector3 newvel = Vector3.Reflect(oldVel, collision.contacts[0].normal);
            velocity = new Vector3(newvel.x, rb.velocity.y, newvel.z);
            GetComponent<AudioSource>().Play();
            GameObject clone = Instantiate(spark, collision.contacts[0].point,  spark.transform.rotation).gameObject;
            spark.Play();

            if(collision.gameObject.tag == "BrickBlock")
            {
                collision.gameObject.GetComponent<AudioSource>().Play();

                for (int i = 0; i < 9; i++)
                {
                    collision.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
                }
                collision.gameObject.GetComponent<MeshRenderer>().enabled = false;
                collision.gameObject.GetComponent<BoxCollider>().enabled = false;
                Destroy(collision.gameObject, 2);
            }
            if(collision.gameObject.tag == "Question")
            {
                Debug.Log(collision.contacts[0].normal);
                if(collision.contacts[0].normal.y < 0.2)
                {
                    StartCoroutine(QuestionBlockHit(collision.gameObject));
                }
            }
        }
        if (collision.gameObject.tag == "Player" && moving)
        {
            GameObject player = collision.gameObject;
            Rigidbody p_rb = player.GetComponent<Rigidbody>();
            Player player_script = player.GetComponent<Player>();

            if (!player.GetComponent<Player>().groundpound && player.GetComponent<Player>().koopashellInvincible > 0.5)
            {
                Hit_Sound.Play();
                if (Player.FireMario)//static bool
                {
                    StartCoroutine(player_script.Downgrade_FireSuit());
                }
            }
        }
        if(collision.gameObject.tag == "GoombaEnemy" && moving)
        {
            collision.gameObject.GetComponent<GoombaChase>().Stop();
            collision.gameObject.GetComponent<GoombaChase>().enabled = false;
            collision.gameObject.GetComponent<Animator>().SetBool("Knockout", true);
            collision.gameObject.transform.GetChild(8).gameObject.SetActive(false);
            collision.gameObject.GetComponent<CapsuleCollider>().enabled = false;
            collision.gameObject.transform.GetChild(5).GetComponent<AudioSource>().Play(); //knockout sound
            StartCoroutine(DestroyGoomba(collision.gameObject));
        }
    }

    private void OnTriggerStay(Collider other)
    {
       
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 18)
        {
            transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            transform.GetChild(0).GetChild(2).GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        }
    }

    public IEnumerator hop()
    {
        for(int i = 0; i < 60; i++)
        {
            rb.AddForce(Vector3.up * 1000 * Time.deltaTime, ForceMode.Impulse);
            yield return new WaitForSeconds(0.01f);
        }
    }
    public IEnumerator DestroyGoomba(GameObject Goomba)
    {
        yield return new WaitForSeconds(0.4f);
        GameObject DeathPS = Goomba.GetComponent<GoombaChase>().DestroyPS;
        Vector3 position = new Vector3(Goomba.transform.GetChild(3).position.x, Goomba.transform.GetChild(3).position.y + 2, Goomba.transform.GetChild(3).position.z + 2);
        GameObject clone = Instantiate(DeathPS, position, DeathPS.transform.rotation);
        Goomba.GetComponent<GoombaChase>().Vanish.Play();
        Destroy(Goomba);
    }


    IEnumerator QuestionBlockHit(GameObject block)
    {
        if(Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) < 20)
        {
            block.transform.parent.gameObject.GetComponent<AudioSource>().Play(); //play the sound effect attached to empty parent object
        }
        for (int i = 0; i < 3; i++)
        {
            block.transform.position += new Vector3(0, 0.3f, 0);
            block.transform.localScale += new Vector3(0.0003f, 0.0003f, 0.0003f);
            yield return new WaitForSeconds(0.001f);
        } //just moving the block up and down
        for (int i = 0; i < 3; i++)
        {
            block.transform.position += new Vector3(0, -0.3f, 0);
            block.transform.localScale += new Vector3(-0.0003f, -0.0003f, -0.0003f);
            yield return new WaitForSeconds(0.001f);
        }

        if (block.gameObject.GetComponent<QuestionBlockID>().ITEM_ID == 0)
        {
            Vector3 newOffset = new Vector3(0, 0.5f, 0);

            GameObject clone = Instantiate(coin, block.transform.position + newOffset, coin.transform.rotation);
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

        


        block.GetComponent<BoxCollider>().enabled = false;
        block.transform.GetChild(0).gameObject.SetActive(false); // indexing the child objects so we can disable them, leaving only the empty block behind
        block.transform.GetChild(1).gameObject.SetActive(false);
        block.transform.GetChild(2).gameObject.SetActive(true); //enabling empty block

    }


}
