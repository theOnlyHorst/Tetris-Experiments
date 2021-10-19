using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagedUIText : MonoBehaviour
{
    [SerializeField]
    private UIManager uIManager;
    [SerializeField]
    private string ComponentName;
    // Start is called before the first frame update
    void Start()
    {
        uIManager.RegisterUIText(ComponentName,this.GetComponent<Text>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
