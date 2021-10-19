using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinoController : MonoBehaviour
{
    private SpriteRenderer rend;
    private Color color;

    private int offX;

    public int OffX{
        get{
            return offX;
        }
    }

    private int offY;

    public int OffY{
        get{
            return offY;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        rend.color = color;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color c)
    {
        color = c;
        if(rend!=null)
            rend.color =c;
    }

    public void SetOffsets(int x, int y)
    {
        offX = x;
        offY = y;

        transform.localPosition=new Vector3(offX,offY,0);
    }

    public void Clear()
    {
        GameObject.Destroy(this.gameObject);
    }

    public void SetAbsolutePos(Vector2 pos)
    {
        transform.position = pos;
    }

}
