using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManage : MonoBehaviour
{
    public GameObject TransitionUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator levelToWorldMap()
    {
        TransitionUI.GetComponent<Animator>().SetBool("TransitionOut", true);
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("World");
    }

    public void level1()
    {
        SceneManager.LoadScene("Level1");
    }
    public void level2()
    {
        SceneManager.LoadScene("Level2-Cave");
    }
}
