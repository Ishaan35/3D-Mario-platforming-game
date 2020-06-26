using UnityEngine;
using System.Collections;

public class GlowColorAnimation : MonoBehaviour
{
    public Color colorA;
    public Color colorB;
    public float speed;
    public Material glowMaterial;

    void Update()
    {
        Color color = Color.Lerp(colorA, colorB, Mathf.PingPong(Time.time * speed, 100));
        glowMaterial.SetColor("_GlowColorMult", color);
    }
}
