using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lives : MonoBehaviour
{
    private Text lives_ui;

    public static int LIVES = 5;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        lives_ui = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        lives_ui.text = "X" + LIVES;
    }
}
