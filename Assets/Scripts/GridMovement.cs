using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour
{   
    [SerializeField]
    private GridController controller;

    bool hardDrop;
    bool rotLeft;

    bool rotRight;
    bool softDropDown, softDropUp;

    bool leftDown, leftUp;

    bool rightDown, rightUp;

    bool leftPressed, rightPressed;

    bool DASOn;
    bool firstMove;

    bool hold;

    private float DAS, ARR;


    private float msCount;


    // Start is called before the first frame update
    void Start()
    {
        DAS = ControlProperties.Instance.DAS;
        ARR = ControlProperties.Instance.ARR;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("harddrop"))
        {
            hardDrop = true;
        }
        if(Input.GetButtonDown("RotLeft"))
        {
            rotLeft =true;
        }
        if(Input.GetButtonDown("RotRight"))
        {
            rotRight = true;
        }
        if(Input.GetButtonDown("SoftDrop"))
        {
            softDropDown = true;
        }
        if(Input.GetButtonUp("SoftDrop"))
        {
            softDropUp = true;
        }
        if(Input.GetButtonDown("left"))
        {
            leftDown = true;
        }
        if(Input.GetButtonUp("left"))
        {
            leftUp = true;
        }
        if(Input.GetButtonDown("right"))
        {
            rightDown = true;
        }
        if(Input.GetButtonUp("right"))
        {
            rightUp = true;
        }
        if(Input.GetButtonDown("Hold"))
        {
            hold =true;
        }
    }

    void FixedUpdate()
    {
        if(hardDrop)
        {
            controller.HardDrop();
            hardDrop=false;
        }

        if(rotRight)
        {
            controller.RotatePieceRight();
            rotRight=false;
        }

        if(rotLeft)
        {
            controller.RotatePieceLeft();
            rotLeft=false;
        }

        if(softDropDown)
        {
            controller.SetSoftDrop(true);
            softDropDown = false;
        }
        if(softDropUp)
        {
            controller.SetSoftDrop(false);
            softDropUp =false;
        }


        if(leftDown)
        {
            msCount =0;
            DASOn = true;
            firstMove = true;
            leftPressed = true;
            leftDown = false;
        }
        if(leftUp)
        {
            msCount =0;
            leftPressed = false;
            leftUp = false;
        }
        if(rightDown)
        {
            msCount =0;
            DASOn = true;
            firstMove = true;
            rightPressed = true;
            rightDown = false;
        }
        if(rightUp)
        {
            msCount =0;
            rightPressed = false;
            rightUp = false;
        }
        
        if(leftPressed)
        {
            if(firstMove)
            {
                controller.MoveLeft();
                firstMove = false;
            }else
            if(DASOn)
            {
                if(msCount>=DAS)
                {
                    DASOn = false;
                    controller.MoveLeft();
                    msCount=0;
                }
                else
                msCount+=Time.deltaTime*1000;
            }else
            {
                if(msCount>=ARR)
                {
                    controller.MoveLeft();
                    msCount=0;
                }
                else
                msCount+=Time.deltaTime*1000;
            }
        }
        if(rightPressed)
        {
            if(firstMove)
            {
                controller.MoveRight();
                firstMove = false;
            }else
            if(DASOn)
            {
                if(msCount>=DAS)
                {
                    DASOn = false;
                    controller.MoveRight();
                    msCount=0;
                }
                else
                msCount+=Time.deltaTime*1000;
            }else
            {
                if(msCount>=ARR)
                {
                    controller.MoveRight();
                    msCount=0;
                }
                else
                msCount+=Time.deltaTime*1000;
            }
        }
        if(hold)
        {
            controller.Hold();
            hold = false;
        }
    }
}
