using UnityEngine;
using System.Collections;

public class SphereBounce : MonoBehaviour
{
    public float force;
    public float torque;
    public float interval;

    private Rigidbody thisRigidbody;
    private Mesh thisMesh;

    private Color[] startColor;
    private float[] startTime;
    private float[] duration;

    private Color targetColor;
    private bool needToLerp;

    private WaitForSeconds delay;

    public void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();
        thisMesh = GetComponent<MeshFilter>().mesh;

        startColor = new Color[thisMesh.vertexCount];
        startTime = new float[thisMesh.vertexCount];
        duration = new float[thisMesh.vertexCount];
        delay = new WaitForSeconds(interval + Random.value * 4);

        targetColor = Color.white;
        needToLerp = false;
        Vector3[] vertices = thisMesh.vertices;
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            colors[i] = targetColor;
        }
        thisMesh.colors = colors;

        StartCoroutine(applyForceAndTorque());
    }

    public void Update()
    {
        lerpColors();
    }

    public void OnCollisionEnter(Collision collision)
    {
        Material material = collision.gameObject.GetComponent<Renderer>().sharedMaterial;
        bool randomColor = true;
        if (material.HasProperty("_GlowColor")) {
            targetColor = material.GetColor("_GlowColor");
            randomColor = (targetColor == Color.black);
        }

        if (randomColor) {
            targetColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
        }

        for (int i = 0; i < startTime.Length; ++i) {
            startColor[i] = thisMesh.colors[i];
            startTime[i] = Time.time;
            duration[i] = Random.Range(0.35f, 1f);
        }
        needToLerp = true;
    }

    private void lerpColors()
    {
        if (!needToLerp)
            return;

        bool continueLerp = false;
        Vector3[] vertices = thisMesh.vertices;
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            colors[i] = Color.Lerp(startColor[i], targetColor, (Time.time - startTime[i]) / duration[i]);
            if (colors[i] != targetColor && !continueLerp)
                continueLerp = true;
        }
        thisMesh.colors = colors;

        needToLerp = continueLerp;
    }

    private IEnumerator applyForceAndTorque()
    {
        while (true) {
            thisRigidbody.AddForce(force * Random.value * (Random.value > 0.5 ? -1 : 1), force * Random.value * (Random.value > 0.5 ? -1 : 1) / 4, 0, ForceMode.Force);
            thisRigidbody.AddTorque(torque * Random.value * (Random.value > 0.5 ? -1 : 1), torque * Random.value, torque * Random.value * (Random.value > 0.5 ? -1 : 1), ForceMode.VelocityChange);

            yield return delay;
        }
    }
}
