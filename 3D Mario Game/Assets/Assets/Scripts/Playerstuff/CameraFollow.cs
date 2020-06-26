using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Player;
    public Transform Lookat;
    public Vector3 offset;
    public bool UseOffsetValues;
    public Player player_script;

    public float rotateSpeed;

    Quaternion rotation;

    public Vector3 Cam_distance; //pipe entry

    public bool Underground = false;


    Transform CameraEndPos;
    [HideInInspector]
    public bool CameraLevelEndMovement = false;
    public bool cloudPlatforms = false;
    public bool startLvel = false;
    public bool restOfCaveLevel = false;
    public bool endOfCaveLevel = false;
    bool EndMusicBool = true; //play music once, and other actions once
    private GameObject course_clear_text; 

    // Start is called before the first frame update
    void Start()
    {
        player_script = Player.gameObject.GetComponent<Player>();
        CameraEndPos = GameObject.FindGameObjectWithTag("CameraEndPosition").transform;
        course_clear_text = GameObject.FindGameObjectWithTag("CourseClearText");

    }

    // Update is called once per frame
    void LateUpdate()
    {
        
        {
            if (!player_script.PipeEntry && !Underground && !player_script.flagpole_end) //flagpole end is when the player is done moving down pipe
            {
                //x position of mouse and rotate player
                if(!player_script.REACHED_GOAL) //reached goal is when player touches the flagpole
                {
                    float horizontal = Input.GetAxis("Mouse X") * rotateSpeed;
                    Lookat.Rotate(0, horizontal, 0); //disable this when in a cave
                }


                transform.position = Lookat.position + offset;

                //move camera based on current rotation of target and original offset
                float desiredYangle = Lookat.eulerAngles.y; //only y axis since we only want camera rotating around Lookat object on y axis

                rotation = Quaternion.Euler(0, desiredYangle, 0);
                transform.position = Lookat.position - (rotation * offset); //the camera position should be same as lookat position, then subtract the offset vector3, and then again multiply it with the rotation, so the offset is affected by camera rotation too
                

                transform.LookAt(Lookat);
            }

            if(CameraLevelEndMovement)
            {
                LevelEndCameraPositionFunc();
                Invoke("EndLevelMusicFunc", 0.75f);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                offset.y = Mathf.Lerp(offset.y, -6, 0.2f);
                offset.z = Mathf.Lerp(offset.z, 15, 0.15f);

            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                offset.y = Mathf.Lerp(offset.y, -3, 0.2f);
                offset.z = Mathf.Lerp(offset.z, 10.5f, 0.15f);

            }

            if (cloudPlatforms)
            {
                Vector3 none = Vector3.zero;
                offset = Vector3.SmoothDamp(offset, new Vector3(offset.x, -3, offset.z), ref none, 5 * Time.deltaTime);
            }
            else if (startLvel)
            {
                Vector3 none = Vector3.zero;
                offset = Vector3.SmoothDamp(offset, new Vector3(offset.x, -4, offset.z), ref none, 5 * Time.deltaTime);
            }
            else if (endOfCaveLevel)
            {
                Vector3 none = Vector3.zero;
                offset = Vector3.SmoothDamp(offset, new Vector3(offset.x, -6.5f, offset.z), ref none, 5 * Time.deltaTime);
            }
            else if(restOfCaveLevel) //make this bool truw when exiting the cloud platform and start level colliders
            {
                Vector3 none = Vector3.zero;
                offset = Vector3.SmoothDamp(offset, new Vector3(offset.x, -5, offset.z), ref none, 5 * Time.deltaTime);
            }


        }
    }
    //transform.position = Player.transform.position + offset
    public void LerpToPosition()
    {
        float speed = 0.5f;
        transform.position = new Vector3(   Mathf.Lerp(transform.position.x, Player.transform.position.x - offset.x, speed)  ,    Mathf.Lerp(transform.position.y, Player.transform.position.y - offset.y * 2.2f, speed)   ,   Mathf.Lerp(transform.position.z, Player.transform.position.z - offset.z * 1.2f, speed));

        Debug.Log(transform.position - Player.transform.position - offset);
    }

    public void LevelEndCameraPositionFunc()
    {
        transform.position = Vector3.Lerp(transform.position, CameraEndPos.position, 5 * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, CameraEndPos.rotation, 4 * Time.deltaTime);
    }
    void EndLevelMusicFunc()
    {
        if(EndMusicBool)
        {
            Debug.Log("LEVEL ENDED");
            EndMusicBool = false;
            GetComponent<AudioSource>().Play(); ///course clear music
            course_clear_text.GetComponent<Animator>().SetBool("CourseClearText", true);
        }
    }


    
}
     