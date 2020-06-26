using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Details : MonoBehaviour
{
    public float rotate = 1.2f;

    // Update is called once per frame
    void Update()
    {
        rotate++;
        RenderSettings.skybox.SetFloat("_Rotation", Time.deltaTime * rotate);
    }
}
