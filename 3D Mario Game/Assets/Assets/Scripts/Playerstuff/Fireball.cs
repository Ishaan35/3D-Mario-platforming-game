using UnityEngine;
using System.Collections;

public class Fireball : MonoBehaviour
{

    public Rigidbody rb;

    public Vector3 velocity;

    




    // Use this for initialization
    void Start()
    {

        rb = GetComponent<Rigidbody>();
        velocity = rb.velocity;


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 gravity = 175 * Vector3.down; //cant simulate fireball bounces with normal realworld gravity, so i ad a downwards force that i can change from script, simulating gravity for fireball only
        rb.AddForce(gravity, ForceMode.Acceleration);

        if (rb.velocity.y < velocity.y) //to avoid arcs formed when mario initially shoots fireball
        {
            rb.velocity = velocity;
        }

        


    }


    void OnCollisionEnter(Collision col)
    {
        //hit goomba
        if(col.gameObject.tag == "GoombaEnemy")
        {

            col.gameObject.GetComponent<GoombaChase>().Stop();
            col.gameObject.GetComponent<GoombaChase>().enabled = false;
            col.gameObject.GetComponent<Animator>().SetBool("Knockout", true);
            col.gameObject.transform.GetChild(8).gameObject.SetActive(false);
            col.gameObject.GetComponent<CapsuleCollider>().enabled = false;
            col.gameObject.transform.GetChild(5).GetComponent<AudioSource>().Play(); //knockout sound
            StartCoroutine(DestroyGoomba(col.gameObject));
            StartCoroutine(Destroy());

        }
        else if(col.gameObject.tag == "MegaGoomba")
        {
            col.gameObject.GetComponent<MegaGoomba>().health--;
            StartCoroutine(Destroy());

           

            if(col.gameObject.GetComponent<MegaGoomba>().health < 1)
            {
                col.gameObject.GetComponent<MegaGoomba>().Stop();
                col.gameObject.GetComponent<MegaGoomba>().enabled = false;
                col.gameObject.GetComponent<Animator>().SetBool("Knockout", true);
                col.gameObject.transform.GetChild(8).gameObject.SetActive(false);
                col.gameObject.GetComponent<CapsuleCollider>().enabled = false;
                col.gameObject.transform.GetChild(5).GetComponent<AudioSource>().Play(); //knockout sound
                StartCoroutine(DestroyMegaGoomba(col.gameObject));
            }
        }

        if(col.gameObject.tag == "Crate")
        {
            StartCoroutine(col.gameObject.GetComponent<Crate>().Destroy_GroundPound());
        }

        if(col.gameObject.tag == "KoopaShell")
        {
            Debug.Log("FireballShell");
            col.gameObject.GetComponent<KoopaShell>().velocity = Vector3.zero;
            col.gameObject.GetComponent<KoopaShell>().moving = false;
            col.gameObject.GetComponent<KoopaShell>().velocity = Vector3.zero;
            col.transform.GetChild(0).GetComponent<Animator>().SetBool("Spin", false);
            StartCoroutine(col.gameObject.GetComponent<KoopaShell>().hop());

            StartCoroutine(Destroy());

        }

        if (col.contacts[0].normal.y > 0.4 && col.contacts[0].normal.y < 1.6)
        {
            rb.velocity = new Vector3(velocity.x, -velocity.y, velocity.z);
        }

        if(col.contacts[0].normal.x > 0.3 || col.contacts[0].normal.z > 0.3f || col.contacts[0].normal.x < -0.3f || col.contacts[0].normal.z < -0.3f)
        {
            Vector3 oldVel = velocity;
            oldVel = oldVel.normalized;
            oldVel *= 1900 * Time.deltaTime;

            Vector3 newvel = Vector3.Reflect(oldVel, col.contacts[0].normal);

            velocity = new Vector3(newvel.x, oldVel.y, newvel.z);
            rb.velocity = rb.velocity;
        }
            








    }
    public IEnumerator Destroy()
    {
        transform.GetComponent<MeshRenderer>().enabled = false;
        transform.GetComponent<SphereCollider>().enabled = false;
        transform.GetComponent<ParticleSystem>().Stop();
        GameObject Dissolve = transform.GetChild(0).gameObject;
        GameObject Clone = Instantiate(Dissolve, transform.position, Dissolve.transform.rotation);
        Clone.GetComponent<ParticleSystem>().Play();
        Destroy(Clone, 5);
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    public IEnumerator DestroyGoomba(GameObject Goomba)
    {
        yield return new WaitForSeconds(0.4f);
        GameObject DeathPS = Goomba.GetComponent<GoombaChase>().DestroyPS;
        Vector3 position = new Vector3(Goomba.transform.GetChild(3).position.x, Goomba.transform.GetChild(3).position.y + 2, Goomba.transform.GetChild(3).position.z+2);
        GameObject clone = Instantiate(DeathPS, position, DeathPS.transform.rotation);
        Goomba.GetComponent<GoombaChase>().Vanish.Play();
        Destroy(Goomba);
    }

    public IEnumerator DestroyMegaGoomba(GameObject Goomba)
    {
        yield return new WaitForSeconds(0.4f);
        GameObject DeathPS = Goomba.GetComponent<MegaGoomba>().DestroyPS;
        Vector3 position = new Vector3(Goomba.transform.GetChild(3).position.x, Goomba.transform.GetChild(3).position.y + 2, Goomba.transform.GetChild(3).position.z + 2);
        GameObject Clone = Instantiate(DeathPS, position, DeathPS.transform.rotation);
        Clone.transform.localScale += new Vector3(10, 10, 10);
        Instantiate(Goomba.GetComponent<MegaGoomba>().MegaMushroom, Goomba.transform.position, Goomba.GetComponent<MegaGoomba>().MegaMushroom.transform.rotation);
        Goomba.GetComponent<MegaGoomba>().Vanish.Play();
        Destroy(Goomba);
    }

}