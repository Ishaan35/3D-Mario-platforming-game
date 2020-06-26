using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        

    }
    public IEnumerator Shake()
    {
        Vector3 original_position = transform.localPosition;
        float time_elapsed = 0;
        while (time_elapsed < 0.1f) //loops to move camera
        {
            float x = Random.Range(-0.07f, 0.07f);
            float y = Random.Range(-0.07f, 0.07f);
            transform.localPosition = new Vector3(x, y, original_position.z);
            time_elapsed += Time.deltaTime;
            yield return new WaitForSeconds(0.05f);

        }
        transform.localPosition = original_position;
    }
}
