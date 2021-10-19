using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalInputs : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    private bool startKeySw;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("StartGame")) startKeySw = true;
    }

    void FixedUpdate()
    {
        if(startKeySw)
        {
            gameManager.OnGameStart.Invoke();
            startKeySw = false;
        }
    }
}
