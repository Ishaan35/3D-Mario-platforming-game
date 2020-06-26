using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public int Pipe_ID; //0 is an exit pipe; no entry, //1 is a pipe going from 1 setting to another i.e surface to underground //2 is a transporting pipe. ie moving within the same setting// 3 is underground to surface //4 is surface to underground with camera still following
    public GameObject destination_Pipe;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
