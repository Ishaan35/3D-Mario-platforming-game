using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Destroy_Punch()
    {
        GetComponent<BoxCollider>().enabled = false;
        Destroy(transform.GetChild(5).gameObject); //groundpound cllider
        yield return new WaitForSeconds(0.2f);
        transform.GetChild(6).gameObject.SetActive(true); //broken planks. Index is 6 because we are destroying a child gameobject before this, so the size of "array" of child gameobjects reduces
        GetComponent<AudioSource>().Play();
        GetComponent<SkinnedMeshRenderer>().enabled = false;
        for (int i = 0; i < 5; i++)
        {
            GameObject CratePS = transform.GetChild(i).gameObject;
            transform.GetChild(i).GetComponent<ParticleSystem>().Play();
            Destroy(CratePS, 3);
        }



    }
    public IEnumerator Destroy_GroundPound()
    {
        transform.GetChild(7).gameObject.SetActive(true);
    
        GetComponent<BoxCollider>().enabled = false;
        yield return new WaitForSeconds(0f);
        Destroy(transform.GetChild(5).gameObject); //groundpound cllider
        GetComponent<AudioSource>().Play();
        GetComponent<SkinnedMeshRenderer>().enabled = false;
        for (int i = 0; i < 5; i++)
        {
            GameObject CratePS = transform.GetChild(i).gameObject;
            transform.GetChild(i).GetComponent<ParticleSystem>().Play();
            Destroy(CratePS, 3);
        }

    }

    public IEnumerator MegaMushroom()
    {
        transform.GetChild(7).gameObject.SetActive(true);
        Instantiate(transform.GetChild(7).gameObject, transform.GetChild(7).position, transform.GetChild(7).rotation); //broken planks

        GetComponent<BoxCollider>().enabled = false;
        yield return new WaitForSeconds(0f);
        Destroy(transform.GetChild(5).gameObject); //groundpound cllider
        GetComponent<AudioSource>().Play();
        GetComponent<SkinnedMeshRenderer>().enabled = false;
        for (int i = 0; i < 5; i++)
        {
            GameObject CratePS = transform.GetChild(i).gameObject;
            transform.GetChild(i).GetComponent<ParticleSystem>().Play();
            Destroy(CratePS, 3);
        }

    }
}
