using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveLookAt : MonoBehaviour
{
    public Vector3 offset;
    public Transform player;
    Vector3 mypos;

    public float smoothness_speed_x = 4.5f;
    public float smoothness_speed_y = 14f;
    public float smoothness_speed_z = 4.5f;

    bool entered_cave = false;
    bool follow = true;

    public GameObject surfaceLight;
    public GameObject lanterns;

    // Start is called before the first frame update
    void Start()
    {
        offset = player.transform.position - transform.position;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;

        mypos = (player.position) - offset;

        if (follow)
        {
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(mypos.x, transform.position.y, transform.position.z), ref velocity, smoothness_speed_x * Time.deltaTime);
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(transform.position.x, mypos.y, transform.position.z), ref velocity, smoothness_speed_y * Time.deltaTime);
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(transform.position.x, transform.position.y, mypos.z), ref velocity, smoothness_speed_z * Time.deltaTime);
        }



    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Equals("CameraRotateCollider0"))
        {
            smoothness_speed_y = 0f;
            transform.eulerAngles = new Vector3(0, 180, 0);
            smoothness_speed_x = 4.5f;
            yield return new WaitForSeconds(0.3f);
            smoothness_speed_z = 200f;
            smoothness_speed_y = 14f;

            if (!entered_cave) //sets the offset right
            {
                entered_cave = true;
            }
            

        }
        if (other.gameObject.name.Equals("CameraRotateCollider1"))
        {
            transform.eulerAngles = new Vector3(25, 90, 0);
            smoothness_speed_x = 200;
            smoothness_speed_z = 4.5f;
            transform.position = new Vector3(288.5f, transform.position.y, transform.position.z);
            offset = new Vector3(offset.x, offset.y, 0);

            lanterns.SetActive(true);

        }
        if (other.gameObject.name.Equals("CameraRotateCollider2"))
        {
            Camera.main.transform.parent.GetComponent<CameraFollow>().cloudPlatforms = true;
            Camera.main.transform.parent.GetComponent<CameraFollow>().restOfCaveLevel = false;
        }
        if (other.gameObject.name.Equals("CameraRotateCollider3"))
        {
            Camera.main.transform.parent.GetComponent<CameraFollow>().startLvel = true;
            Camera.main.transform.parent.GetComponent<CameraFollow>().restOfCaveLevel = false;
        }
        if (other.gameObject.name.Equals("CameraRotateCollider4"))
        {
            Camera.main.transform.parent.GetComponent<CameraFollow>().endOfCaveLevel = true;
            Camera.main.transform.parent.GetComponent<CameraFollow>().restOfCaveLevel = false;
            offset = new Vector3(offset.x, offset.y, -3);
            smoothness_speed_z = 0;
            smoothness_speed_y = 0;
            yield return new WaitForSeconds(0.5f);
            smoothness_speed_z = 30;
            smoothness_speed_y = 14;
        }

        if (other.gameObject.name.Equals("CameraRotateCollider5"))
        {
            GameObject.Find("Directional Light").transform.eulerAngles = new Vector3(95, -20, 0);
            transform.eulerAngles = new Vector3(0, 180, 0);
            smoothness_speed_x = 4.5f;
            yield return new WaitForSeconds(0.5f);
            smoothness_speed_z = 200f;
            offset = new Vector3(offset.x, -5, offset.z);
            if (!entered_cave) //sets the offset right
            {
                smoothness_speed_y = 14f;
                entered_cave = true;
            }
            lanterns.SetActive(false);


        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("CameraRotateCollider2"))
        {
            Camera.main.transform.parent.GetComponent<CameraFollow>().cloudPlatforms = false;
            Camera.main.transform.parent.GetComponent<CameraFollow>().restOfCaveLevel = true;
        }
        if (other.gameObject.name.Equals("CameraRotateCollider3"))
        {
            Camera.main.transform.parent.GetComponent<CameraFollow>().startLvel = false;
            Camera.main.transform.parent.GetComponent<CameraFollow>().restOfCaveLevel = true;
        }



    }
}
