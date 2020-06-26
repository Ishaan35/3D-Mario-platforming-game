using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinCollect : MonoBehaviour
{
    public Text CoinUI;
    
    public static int COIN_COUNT = 0;
    // Start is called before the first frame update
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        CoinUI.text = ("X" + COIN_COUNT);
       
    }

    private void OnCollisionEnter(Collision collision)
    {
               
            
        
    }
}
