using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetriminoController : MonoBehaviour
{
    public Tetrimino tetrimino;

    [SerializeField]
    private TetriminoPropertyContainer propertyContainer;

    public bool ghost;

    public bool prev;


    private MinoController[] minoControllers;

    public MinoController[] MinoControllers
    {
        get{
             return minoControllers;
        }
    }

    private TetriminoPropertyContainer.TetriminoProperties properties;

    public TetriminoPropertyContainer.TetriminoProperties Properties
    {
        get{
            return properties;
        }
    }

    public int rotation;

   

    // Start is called before the first frame update
    void Start()
    {
        properties = propertyContainer.GetPropertiesOf(tetrimino);
        minoControllers = transform.GetComponentsInChildren<MinoController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(transform.childCount==0)
            GameObject.Destroy(gameObject);
    }
    public void GravityEffect()
    {
        transform.position  += new Vector3(0,-1,0);
    }

    public void Shape()
    {
        if(prev)
            transform.localScale = new Vector3(0.75f,0.75f,1);
        minoControllers = transform.GetComponentsInChildren<MinoController>();
        properties = propertyContainer.GetPropertiesOf(tetrimino);
        foreach(var c in minoControllers)
        {
            var col = properties.color;
            if(ghost)
                col = new Color(col.r,col.g,col.b,0.5f);
            c.SetColor(col);
        }
        Reshape();
    }

    public void Reshape()
    {
        minoControllers[0].SetOffsets(0,0);
        for (int i=1;i<minoControllers.Length;i++)
        {
            minoControllers[i].SetOffsets(properties.GetOffsets(rotation)[(i-1)*2],properties.GetOffsets(rotation)[(i-1)*2+1]);
        }
    }

    public void SetPosition(Vector2 vec)
    {
        transform.position = new Vector3(vec.x,vec.y,0);
    }

    public void RotateRight()
    {
        if(rotation==3)
            rotation=0;
        else
            rotation++;
        Reshape();
    }
    public void RotateLeft()
    {
        if(rotation==0)
            rotation=3;
        else
            rotation--;
        Reshape();
    }

    public void GreyOut()
    {
        foreach(var c in minoControllers)
        {
            c.SetColor(Color.gray);
        }
    }


}
