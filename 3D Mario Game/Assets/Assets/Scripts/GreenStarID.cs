using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenStarID : MonoBehaviour
{
    public int ID;
    public ParticleSystem my_material;

    public Color my_colour;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ParticleSystem.MainModule colour = my_material.main;

        colour.startColor = Color.HSVToRGB(100, 100, 100);

    }
}
