using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeEntryManager : MonoBehaviour
{
    public Player player;

    Transform pipe;
    Transform EnoughGoDown;


    private Rigidbody rb;
    Vector3 direction;
    bool downpipe = false;
    bool upPipe = false;

    bool scene_change = false; // surface to underground
    bool scene_return = false; //underground to surface
    bool scene_change_stillMoving; //surface to underground still moving with camera


    Pipe pipe_script;
    int pipeid = 0;




    // Start is called before the first frame update
    void Start()
    {
        
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (pipe != null)
        {


            direction = pipe.transform.position - transform.position;
            if (downpipe)
            {
                bool went_down_pipe = false;
                //long if statements make sure that player is standing close to the center of the pipe
                if((pipe.transform.position.x - transform.position.x >= 0.05 || pipe.transform.position.x - transform.position.x <= -0.05 || pipe.transform.position.z - transform.position.z >= 0.05 || pipe.transform.position.z - transform.position.z <= -0.05)    && (transform.position.y - EnoughGoDown.position.y >= 0.5f)  && !went_down_pipe)
                {
                    player.PipeEntry = true;
                    rb.velocity = new Vector3(direction.x * 300 * Time.deltaTime, 0f, direction.z * 300 * Time.deltaTime);
                }

                if (pipe.transform.position.x - transform.position.x <= 0.05 && pipe.transform.position.x - transform.position.x >= -0.05 && pipe.transform.position.z - transform.position.z <= 0.05   &&   pipe.transform.position.z - transform.position.z >= -0.05    &&    (transform.position.y - EnoughGoDown.position.y >= 0.5f) && !went_down_pipe)
                {
                    rb.isKinematic = true;
                    transform.Translate(0, -3 * Time.deltaTime, 0);
                }
                //after player goes down pipe enough, transport him to the destination
                if(transform.position.y - EnoughGoDown.position.y < 0.5f && went_down_pipe == false)
                {
                    went_down_pipe = true;
                    //this following stuff applies to level2
                    if(pipeid == 4 && player.CURRENTLEVEL == "Level2")
                    {
                        GameObject.Find("Directional Light").SetActive(true);
                        GameObject.Find("Directional Light").transform.eulerAngles = new Vector3(95, 6, 0);
                        GameObject.Find("Directional LightSurface1").GetComponent<Light>().intensity = 0;
                        GameObject.Find("Directional Light").GetComponent<Light>().intensity = 1.1f;
                        pipeid = 0;
                    }
                    else if(pipeid == 5 && player.CURRENTLEVEL == "Level2")
                    {
                        GameObject.Find("Directional Light").SetActive(false);
                        GameObject.Find("Directional LightSurface1").GetComponent<Light>().intensity = 1.8f;
                    }
                    else if(pipeid == 2 && player.CURRENTLEVEL == "Level2")
                    {
                        GameObject.Find("Directional Light").transform.eulerAngles = new Vector3(95, -20, 0);
                        GameObject.Find("Directional Light").GetComponent<Light>().intensity = 1.3f;
                    }
                    if (went_down_pipe)
                    {
                        player.transform.position = pipe_script.destination_Pipe.transform.GetChild(1).position; //get the empty gameobject representing the position for where the player should spawn in destination pipe
                        downpipe = false;
                        Camera.main.transform.parent.position = pipe_script.destination_Pipe.transform.GetChild(0).position;
                        Camera.main.transform.parent.rotation = pipe_script.destination_Pipe.transform.GetChild(0).rotation; //camera transform

                    }
                }
       

            }
            if(upPipe)
            {
                if(Vector3.Distance(pipe_script.destination_Pipe.transform.GetChild(2).position, transform.position) > 0.1 && upPipe) //enough go up
                {
                    transform.Translate(0, 3 * Time.deltaTime, 0);
                }
                if(Vector3.Distance(pipe_script.destination_Pipe.transform.GetChild(2).position, transform.position) <= 0.4)
                {
                    upPipe = false;
                    //player.PipeEntry = false;
                    rb.isKinematic = false;
                }
            }

       

            if(!downpipe && !upPipe && rb.isKinematic == false)
            {
                rb.isKinematic = false;
                player.PipeEntry = false;
            }
            
            
        }


    }

    IEnumerator OnTriggerEnter(Collider other)
    {
        if((other.gameObject.tag == "Pipe" )&& !player.MEGAMUSHROOM && !player.holdingShell)
        {
            if (other.gameObject.GetComponent<Pipe>().Pipe_ID == 2)
            {
                pipe = other.gameObject.transform.GetChild(0);
                EnoughGoDown = other.gameObject.transform.GetChild(1);
                pipe_script = other.gameObject.GetComponent<Pipe>();

                while (!Input.GetKeyDown(KeyCode.V) && Vector3.Distance(pipe.transform.position, transform.position) < 1.5f)
                {
                    yield return new WaitForSeconds(0.01f);
                }
                if(Input.GetKeyDown(KeyCode.V))
                {
                    downpipe = true;
                    pipe.transform.GetComponentInParent<AudioSource>().Play();
                    pipeid = 2;
                }
            }
            if(other.gameObject.GetComponent<Pipe>().Pipe_ID == 1)
            {
                pipe = other.transform.GetChild(0);
                EnoughGoDown = other.transform.GetChild(1);
                pipe_script = other.gameObject.GetComponent<Pipe>();
                while (!Input.GetKeyDown(KeyCode.V) && Vector3.Distance(pipe.transform.position, transform.position) < 1.1f)
                {
                    yield return new WaitForSeconds(0.01f);
                }
                if (Input.GetKeyDown(KeyCode.V))
                {
                    downpipe = true;
                    pipe.transform.GetComponentInParent<AudioSource>().Play();
                    scene_change = true;
                    scene_return = false;
                    
                }
                
            }
            if (other.gameObject.GetComponent<Pipe>().Pipe_ID == 3 || other.gameObject.GetComponent<Pipe>().Pipe_ID == 5)
            {
                pipe = other.transform.GetChild(0);
                EnoughGoDown = other.transform.GetChild(1);
                pipe_script = other.gameObject.GetComponent<Pipe>();
                while (!Input.GetKeyDown(KeyCode.V) && Vector3.Distance(pipe.transform.position, transform.position) < 1.1f)
                {
                    yield return new WaitForSeconds(0.01f);
                }
                if (Input.GetKeyDown(KeyCode.V))
                {
                    downpipe = true;
                    pipe.transform.GetComponentInParent<AudioSource>().Play();
                    scene_change = false;
                    scene_return = true;

                    if(other.gameObject.GetComponent<Pipe>().Pipe_ID == 5)
                    {
                        pipeid = 5;
                    }

                }

            }
            if (other.gameObject.GetComponent<Pipe>().Pipe_ID == 4)
            {
                pipe = other.transform.GetChild(0);
                pipeid = 4;
                EnoughGoDown = other.transform.GetChild(1);
                pipe_script = other.gameObject.GetComponent<Pipe>();
                while (!Input.GetKeyDown(KeyCode.V) && Vector3.Distance(pipe.transform.position, transform.position) < 1.1f)
                {
                    yield return new WaitForSeconds(0.01f);
                }
                if (Input.GetKeyDown(KeyCode.V))
                {
                    downpipe = true;
                    pipe.transform.GetComponentInParent<AudioSource>().Play();
                    scene_change_stillMoving = true;
                    scene_return = false;
                }

            }

        }

        if(other.gameObject.tag == "PipeDestination")
        {
            upPipe = true;
            yield return new WaitForSeconds(1);
            pipe.transform.GetComponentInParent<AudioSource>().Play();

            if(scene_change)//going underground
            {
                Camera.main.transform.parent.GetComponent<CameraFollow>().Underground = true;
                while(Camera.main.GetComponent<AudioSource>().volume > 0)
                {
                    Camera.main.GetComponent<AudioSource>().volume -= 0.05f;
                    yield return new WaitForSeconds(0.01f);
                }
                Camera.main.GetComponent<AudioSource>().Stop();
                Camera.main.transform.GetChild(2).GetComponent<AudioSource>().Play();
                scene_change = false;

            }
            if (scene_change_stillMoving)//going underground
            {
                while (Camera.main.GetComponent<AudioSource>().volume > 0)
                {
                    Camera.main.GetComponent<AudioSource>().volume -= 0.05f;
                    yield return new WaitForSeconds(0.01f);
                }
                Camera.main.GetComponent<AudioSource>().Stop();
                Camera.main.transform.GetChild(2).GetComponent<AudioSource>().Play();
                scene_change_stillMoving = false;

            }
            else if(scene_return)//going back to surface
            {
                Camera.main.transform.parent.GetComponent<CameraFollow>().Underground = false;
                while (Camera.main.transform.GetChild(2).GetComponent<AudioSource>().volume < 0.6)
                {
                    Camera.main.transform.GetChild(2).GetComponent<AudioSource>().volume -= 0.05f;
                    yield return new WaitForSeconds(0.01f);
                }
                Camera.main.transform.GetChild(2).GetComponent<AudioSource>().Stop();
                Camera.main.GetComponent<AudioSource>().volume = 0.6f;   //reset volume
                Camera.main.GetComponent<AudioSource>().Play();
                scene_return = false;
            }
        }
    }

    
    










}
