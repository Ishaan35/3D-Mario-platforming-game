using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapLevelEnter : MonoBehaviour
{
    public GameObject transitionUI;

    [Header("LEVEL 1 STUFF")]
    public BoxCollider LevelWall;
    private GameObject Player;
    public GameObject level1;
    public Transform level1Camera;
    public GameObject Level1Particles;
    public GameObject Level1Flag;

    public static bool Level1Complete = false;
    public static bool Level2Complete = false;

    private sceneManage scene_manage;



    [Header("LEVEL 2 STUFF")]
    public GameObject level2;
    public GameObject Level2Particles;
    public Transform Level2Camera;
    public BoxCollider Level2Wall;
    public GameObject Level2Flag;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        scene_manage = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<sceneManage>();

    }

    // Update is called once per frame
    void Update()
    {
        if(Level1Complete)
        {
            Level1Flag.SetActive(true);
        }
        if (Level2Complete)
        {
            Level2Flag.SetActive(true);
        }
    }

    public IEnumerator EnterLevel(GameObject Particles)
    {
        LevelWall.enabled = false;
        yield return new WaitForSeconds(1);
        int num = Particles.transform.childCount;
        for(int i = 0; i < num; i++)
        {
            Particles.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
        }
        GetComponent<AudioSource>().Play();

        Camera.main.GetComponent<Animator>().SetTrigger("Shake");
        yield return new WaitForSeconds(0.1f);
        Camera.main.GetComponent<Animator>().ResetTrigger("Shake");

        yield return new WaitForSeconds(2f);
        transitionUI.GetComponent<Animator>().SetBool("TransitionOut", true);

        for (int i = 100; i >= 0; i--)
        {
            Camera.main.transform.parent.GetComponent<AudioSource>().volume -= 0.025f;
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(1.5f);
        scene_manage.level1();

    }

    public IEnumerator EnterLevel2(GameObject Particles)
    {
        Level2Wall.enabled = false;
        yield return new WaitForSeconds(1);
        int num = Particles.transform.childCount;
        for (int i = 0; i < num; i++)
        {
            Particles.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
        }
        GetComponent<AudioSource>().Play();

        Camera.main.GetComponent<Animator>().SetTrigger("Shake");
        yield return new WaitForSeconds(0.1f);
        Camera.main.GetComponent<Animator>().ResetTrigger("Shake");

        yield return new WaitForSeconds(2f);
        transitionUI.GetComponent<Animator>().SetBool("TransitionOut", true);

        for (int i = 100; i >= 0; i--)
        {
            Camera.main.transform.parent.GetComponent<AudioSource>().volume -= 0.025f;
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(1.5f);
        scene_manage.level2();

    }
}
