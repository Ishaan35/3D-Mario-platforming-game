using UnityEngine;
using System.Collections;

public class PlayAnimOnKeyUp : MonoBehaviour {

    public GameObject mainProjectile;
    public ParticleSystem mainParticleSystem;

	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.Space))
        {
            mainProjectile.SetActive(true);
        }

        if (mainParticleSystem.IsAlive() == false)
            mainProjectile.SetActive(false);
	
	}
}
