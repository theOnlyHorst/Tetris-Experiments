using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{

    private static readonly int LAST_GRID_POS = 199;


    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private TetriminoPropertyContainer propertyContainer;

    private Dictionary<int,MinoController> gridMap;

    private Queue<Tetrimino> nextQueue;

    private TetriminoController active;

    private TetriminoController activeGhost;

    [SerializeField]
    private float gridStartCellOffsetX;
    [SerializeField]
    private float gridStartCellOffsetY;
    [SerializeField]
    private float gridEndCellOffsetX;
    [SerializeField]
    private float gridEndCellOffsetY;
    [SerializeField]
    private Transform previews;
    [SerializeField]
    private Transform holdPos;

    private float SDF;

    private int activeLevel;

    private float msCount;

    private int activeGridPos;


    private bool flagLockDelayActive;
    private float lockDelayMsCount;

    private int stepResetCount;


    private bool softDrop;

    private GameObject[] prevs;

    private Tetrimino hold = Tetrimino.NONE;

    private GameObject holdPrev;

    private bool holdAllowed;

    

    // Start is called before the first frame update
    void Start()
    {
        prevs = new GameObject[4];
        gridMap = new Dictionary<int, MinoController>();
        nextQueue = new Queue<Tetrimino>();
        gameManager.RegisterGrid(this);
        SDF = ControlProperties.Instance.SDF;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(gameManager.currentState !=GameManager.GameState.Running)
            return;
        flagLockDelayActive = ActiveHitGround();
        if(flagLockDelayActive)
        {
            
            
                int lockDelayMS = gameManager.GetLevel(activeLevel).lockDelayMS;
                if(softDrop)
                {
                    lockDelayMS = 500;
                }
                if(lockDelayMsCount>=lockDelayMS)
                {
                    LockPiece();
                    NextPiece();
                }
                else
                    lockDelayMsCount+=Time.deltaTime*1000;
            
        }else
        {
            int dropSpeedMS = gameManager.GetLevel(activeLevel).dropSpeedMS;
            if(softDrop)
            {
                dropSpeedMS=(int)(dropSpeedMS/SDF);
            }
            if(msCount>=dropSpeedMS)
            {
                  
                active.GravityEffect();
                activeGridPos+=10;
                lockDelayMsCount = 0;
                
                msCount =0;
            }
            msCount+=Time.deltaTime*1000;
        }
    }
    public void StartGame()
    {
        foreach(var min in gridMap)
        {
            min.Value.Clear();
        }
        if(active!=null)
        {
            foreach(var min in active.MinoControllers)
            {
                min.Clear();
            }
            foreach(Transform t in transform)
                GameObject.Destroy(t.gameObject);
        }
        if(activeGhost!=null)
        {
            foreach(var min in activeGhost.MinoControllers)
            {
                min.Clear();
            }
        }
        if(hold!=Tetrimino.NONE)
        {
            hold = Tetrimino.NONE;
            GameObject.Destroy(holdPrev);
        }
        gridMap.Clear();
        nextQueue.Clear();
        gameManager.clearBags();
        nextQueue.Enqueue(gameManager.GetNextPiece());
        nextQueue.Enqueue(gameManager.GetNextPiece());
        nextQueue.Enqueue(gameManager.GetNextPiece());
        nextQueue.Enqueue(gameManager.GetNextPiece());
        nextQueue.Enqueue(gameManager.GetNextPiece());

        foreach(var x in prevs)
        {
            GameObject.Destroy(x);
        }
        Tetrimino[] arr = nextQueue.ToArray();

        SetPreview(0,arr[0]);
        SetPreview(1,arr[1]);
        SetPreview(2,arr[2]);
        SetPreview(3,arr[3]);
        NextPiece();
        activeLevel =1;
        lockDelayMsCount =0;
        stepResetCount =0;
    }

    private void NextPiece()
    {
        var VecPos = new Vector2(transform.position.x+gridStartCellOffsetX+4,transform.position.y+gridStartCellOffsetY);
        var tetri = nextQueue.Dequeue();
        var nPiece = GameObject.Instantiate(propertyContainer.GetPropertiesOf(tetri).prefab,VecPos,transform.rotation);
        nPiece.transform.parent = transform;
        if(activeGhost!=null)
            GameObject.Destroy(activeGhost.gameObject);
        active = nPiece.GetComponent<TetriminoController>();
        active.tetrimino = tetri;
        active.Shape();
        nextQueue.Enqueue(gameManager.GetNextPiece());
        activeGridPos = 4;
        var offsets = active.Properties.GetOffsets(active.rotation);
        if(HasMinoOnGridPos(activeGridPos))
        {
            GameOver();
            return;
        }
        foreach (var x in offsets)
        {
            if(HasMinoOnGridPos(activeGridPos+x))
            {
                GameOver();
                return;
            }
        }
        var ghostPiece = GameObject.Instantiate(propertyContainer.GetPropertiesOf(tetri).prefab,VecPos,transform.rotation);
        var ghostController = ghostPiece.GetComponent<TetriminoController>();
        ghostController.tetrimino = tetri;
        ghostController.ghost = true;
        ghostController.Shape();
        ghostController.SetPosition(GridPosToVec(GetGroundPos()));
        activeGhost = ghostController;

        msCount =0;
        lockDelayMsCount =0;
        stepResetCount = 0;
        flagLockDelayActive=false;
        AdvancePreview();
        Tetrimino[] arr = nextQueue.ToArray();
        SetPreview(3,arr[3]);
        holdAllowed = true;
    }

    public void SetPreview(int pos,Tetrimino tet)
    {
        if(pos<prevs.Length)
        {
            Vector2 vecPos = previews.GetChild(pos).position;
            prevs[pos] = GameObject.Instantiate(propertyContainer.GetPropertiesOf(tet).prefab,vecPos,transform.rotation);
            var controller = prevs[pos].GetComponent<TetriminoController>();
            controller.tetrimino = tet;
            controller.prev = true;
            controller.Shape();
            prevs[pos].transform.position = vecPos+controller.Properties.previewCenterOffset;
        }
    }

    public void AdvancePreview()
    {
        var temp = prevs[0];
        GameObject.Destroy(temp);
        Vector2 vecPos = previews.GetChild(0).position;
        prevs[0] = prevs[1];
        var cont = prevs[0].GetComponent<TetriminoController>();
        prevs[0].transform.position = vecPos+cont.Properties.previewCenterOffset;
        vecPos = previews.GetChild(1).position;
        prevs[1] = prevs[2];
        cont = prevs[1].GetComponent<TetriminoController>();
        prevs[1].transform.position = vecPos+cont.Properties.previewCenterOffset;
        vecPos = previews.GetChild(2).position;
        prevs[2] = prevs[3];
        cont = prevs[2].GetComponent<TetriminoController>();
        prevs[2].transform.position = vecPos+cont.Properties.previewCenterOffset;
    }

    public void GameOver()
    {
        gameManager.OnGameEnd.Invoke();
    }
    public void EndGame()
    {
        GreyOut();
    }

    public Vector2 GridPosToVec(int gridPos)
    {
        

        float x = gridStartCellOffsetX;
        float y = gridStartCellOffsetY;
        if(gridPos<0)
        {
            for(int i=0;i>=gridPos/10;i--)
            {
                y+=1;
            }
            for(int i=0;i<10+(gridPos%10);i++)
            {
                x+=1;
            }
        }
        else
        {
            for(int i=0;i<gridPos/10;i++)
            {
                y-=1;
            }
        
            for(int i=0;i<gridPos%10;i++)
            {
                x+=1;
            }
        }
        return new Vector2(x,y);
    }

    public bool HasMinoOnGridPos(int gridPos)
    {
        return gridMap.TryGetValue(gridPos,out var min);
    }

    public bool ActiveHitGround()
    {

        return GridPosHitGround(activeGridPos);
    }

    public bool GridPosHitGround(int gridPos)
    {

        var offsets= active.Properties.GetOffsets(active.rotation);
        if(HasMinoOnGridPos(gridPos+10)||gridPos+10>LAST_GRID_POS)
        {
            return true;
        }
        for(int i=0;i<offsets.Length;i+=2)
        {
            int x = offsets[i];
            int y = offsets[i+1];

            int mGridPos = gridPos+(-(y*10)+x);
            
            if(HasMinoOnGridPos(mGridPos+10)||mGridPos+10>LAST_GRID_POS)
            {
                return true;
            }
        }
        return false;
    }

    public void LockPiece()
    {
        var mins = active.MinoControllers;
        foreach(var min in mins)
        {
            var x = min.OffX;
            var y = min.OffY;

            int minNGridPos = activeGridPos+(-(y*10)+x);
            PutMino(min,minNGridPos);
        }
        ClearLines();
    }

    public void PutMino(MinoController cont,int gridPos)
    {
        gridMap[gridPos] = cont;
    }

    public void HardDrop()
    {
        if(gameManager.currentState!=GameManager.GameState.Running)
            return;

        activeGridPos = GetGroundPos();
        active.SetPosition(GridPosToVec(activeGridPos));
        LockPiece();
        NextPiece();
    }

    public int GetGroundPos()
    {
        for(int i=activeGridPos;i<LAST_GRID_POS;i+=10)
        {
            if(GridPosHitGround(i))
            {
                
                return i;
            }
        }
        return -1;
    }

    public void RotatePieceRight()
    {
         if(gameManager.currentState!=GameManager.GameState.Running)
            return;
        int newPos=activeGridPos;
        TetriminoPropertyContainer.RotationOffsetTable offTable = propertyContainer.GetOffsetTable(active.tetrimino);
        TetriminoPropertyContainer.RotationOffset offset;

        switch(active.rotation)
        {
            case 0:
                offset = offTable.OToR;
                break;
            case 1:
                offset = offTable.RToT;
                break;
            case 2:
                offset = offTable.TToL;
                break;
            case 3:
                offset = offTable.LToO;
                break;
            default:
                offset = offTable.OToR;
                break;
        }
        
        if(active.tetrimino==Tetrimino.I)
        {
            switch(active.rotation)
            {
                case 0: 
                    newPos++;
                    break;
                case 1:
                    newPos+=10;
                    break;
                case 2:
                    newPos--;
                    break;
                case 3:
                    newPos-=10;
                    break;
            }
        }else if(active.tetrimino==Tetrimino.O)
        {
            switch(active.rotation)
            {
                case 0: 
                    newPos-=10;
                    break;
                case 1:
                    newPos++;
                    break;
                case 2:
                    newPos+=10;
                    break;
                case 3:
                    newPos--;
                    break;
            }
            
        }
        int oldNPos = newPos;
        int x = offset.offset1.x*-1;
        int y = offset.offset1.y*-1;
        newPos+= (-(y*10)+x);
        

        if(!CanRotate(newPos,active.rotation==3?0:active.rotation+1))
        {
            newPos = oldNPos;
            x = offset.offset2.x;
            y = offset.offset2.y;
            newPos+= (-(y*10)+x);
                                                                            //Handling an edge case, might want to figure out more elegant solution
            if(!CanRotate(newPos,active.rotation==3?0:active.rotation+1)||(activeGridPos%10==9&&active.tetrimino==Tetrimino.I&&active.rotation==3))
            {
                newPos = oldNPos;
                x = offset.offset3.x;
                y = offset.offset3.y;

                newPos+= (-(y*10)+x);

                if(!CanRotate(newPos,active.rotation==3?0:active.rotation+1))
                {
                    newPos = oldNPos;
                    x = offset.offset4.x;
                    y = offset.offset4.y;

                    newPos+= (-(y*10)+x);

                    if(!CanRotate(newPos,active.rotation==3?0:active.rotation+1))
                    {
                        newPos = oldNPos;
                        x = offset.offset5.x;
                        y = offset.offset5.y;

                        newPos+= (-(y*10)+x);

                        if(!CanRotate(newPos,active.rotation==3?0:active.rotation+1))
                        {
                            return;
                
                        }
                
                    }
                
                }
                
            }
        }


        activeGridPos = newPos;
        active.SetPosition(GridPosToVec(activeGridPos));
        active.RotateRight();
        activeGhost.SetPosition(GridPosToVec(GetGroundPos()));
        activeGhost.RotateRight();
        if(stepResetCount<15)
        {
            lockDelayMsCount =0;
            stepResetCount++;
        }
    }

    public void RotatePieceLeft()
    {
         if(gameManager.currentState!=GameManager.GameState.Running)
            return;
        int newPos=activeGridPos;
        TetriminoPropertyContainer.RotationOffsetTable offTable = propertyContainer.GetOffsetTable(active.tetrimino);
        TetriminoPropertyContainer.RotationOffset offset;

        switch(active.rotation)
        {
            case 0:
                offset = offTable.LToO;
                
                break;
            case 1:
                offset = offTable.OToR;
                
                break;
            case 2:
                offset = offTable.RToT;
                
                break;
            case 3:
                offset = offTable.TToL;
                break;
            default:
                offset = offTable.OToR;
                break;
        }
        if(active.tetrimino==Tetrimino.I)
        {
            switch(active.rotation)
            {
                case 0: 
                    newPos+=10;
                    break;
                case 1:
                    newPos--;
                    break;
                case 2:
                    newPos-=10;
                    break;
                case 3:
                    newPos++;
                    break;
            }
        }else if(active.tetrimino==Tetrimino.O)
        {
            switch(active.rotation)
            {
                case 0: 
                    newPos++;
                    break;
                case 1:
                    
                    newPos+=10;
                    break;
                case 2:
                    
                    newPos--;
                    break;
                case 3:
                    
                    newPos-=10;
                    break;
            }
                
        }
        int oldNPos = newPos;
        int x = offset.offset1.x*-1;
        int y = offset.offset1.y*-1;
        newPos+= (-(y*10)+x);
        if(!CanRotate(newPos,active.rotation==0?3:active.rotation-1))
        {
            newPos = oldNPos;
            x = offset.offset2.x*-1;
            y = offset.offset2.y*-1;
            newPos+= (-(y*10)+x);

            if(!CanRotate(newPos,active.rotation==0?3:active.rotation-1))
            {
                newPos = oldNPos;
                x = offset.offset3.x*-1;
                y = offset.offset3.y*-1;

                newPos+= (-(y*10)+x);

                if(!CanRotate(newPos,active.rotation==0?3:active.rotation-1))
                {
                    newPos = oldNPos;
                    x = offset.offset4.x*-1;
                    y = offset.offset4.y*-1;

                    newPos+= (-(y*10)+x);

                    if(!CanRotate(newPos,active.rotation==0?3:active.rotation-1))
                    {
                        newPos = oldNPos;
                        x = offset.offset5.x*-1;
                        y = offset.offset5.y*-1;

                        newPos+= (-(y*10)+x);

                        if(!CanRotate(newPos,active.rotation==0?3:active.rotation-1))
                        {
                            return;
                
                        }
                
                    }
                
                }
                
            }
        }
        
        activeGridPos = newPos;
        active.SetPosition(GridPosToVec(activeGridPos));
        active.RotateLeft();
        activeGhost.SetPosition(GridPosToVec(GetGroundPos()));
        activeGhost.RotateLeft();
        if(stepResetCount<15)
        {
            lockDelayMsCount =0;
            stepResetCount++;
        }
    }

    public bool CanRotate(int gridPos,int rotationAfter)
    {
        var offsetsAfter = active.Properties.GetOffsets(rotationAfter);
        bool flagBorderL = false;
        bool flagBorderR = false;

        int modI =gridPos%10;
        if(modI<0) modI=10+modI;

        if(modI==0)
            flagBorderL =true;
        if(modI==9)
            flagBorderR = true;
        if(HasMinoOnGridPos(gridPos)||gridPos>LAST_GRID_POS)
        {
               return false;
        }
        for(int i=0;i<offsetsAfter.Length;i+=2)
        {

            int xA = offsetsAfter[i];
            int yA = offsetsAfter[i+1];
            int mGridPosA = gridPos+(-(yA*10)+xA);

            int modA = mGridPosA%10;

            if(modA<0) modA=10+modA;

            if(modA==9)
            {
                
                flagBorderR=true;
            }
            if(modA==0)
            {
                
                flagBorderL=true;
            }
            
            if(HasMinoOnGridPos(mGridPosA)||mGridPosA>LAST_GRID_POS)
            {
                return false;
            }
        }
        if(flagBorderL&&flagBorderR)
        {
                return false;
        }
        return true;
    }

    public void SetSoftDrop(bool set)
    {
         if(gameManager.currentState!=GameManager.GameState.Running)
            return;
        softDrop = set;
    }

    private bool CanMovePiece(int movement)
    {
        if(active==null)
            return false;
        var offsets = active.Properties.GetOffsets(active.rotation);
        if((activeGridPos % 10 == 0&&movement==-1)||(activeGridPos % 10==9&&movement==1))
        {
            return false;
        }
        if(HasMinoOnGridPos(activeGridPos+movement))
            return false;
        for(int i=0;i<offsets.Length;i+=2)
        {
            int x = offsets[i];
            int y = offsets[i+1];

            int mGridPos = activeGridPos+(-(y*10)+x);

            if(mGridPos<0)
                mGridPos=10+mGridPos;

            if((mGridPos % 10 == 0&&movement==-1)||(mGridPos % 10==9&&movement==1))
            {
                return false;
            }
            if(HasMinoOnGridPos(mGridPos+movement))
            {
                return false;
            }
        }
        return true;
    }

    public void MoveLeft()
    {
         if(gameManager.currentState!=GameManager.GameState.Running)
            return;
        if(CanMovePiece(-1))
        {
            activeGridPos--;
            active.SetPosition(GridPosToVec(activeGridPos));
            activeGhost.SetPosition(GridPosToVec(GetGroundPos()));
            if(stepResetCount<15)
            {
                lockDelayMsCount =0;
                stepResetCount++;
            }
        }
    }

    public void MoveRight()
    {
         if(gameManager.currentState!=GameManager.GameState.Running)
            return;
        if(CanMovePiece(1))
        {
            activeGridPos++;
            active.SetPosition(GridPosToVec(activeGridPos));
            activeGhost.SetPosition(GridPosToVec(GetGroundPos()));
            if(stepResetCount<15)
            {
                lockDelayMsCount =0;
                stepResetCount++;
            }
        }
    }

    public bool PosIsValid(int position)
    {
        var offsets = active.Properties.GetOffsets(active.rotation);

        bool flagBorderR = false;

        bool flagBorderL = false;

        int modI =position%10;
        if(modI<0) modI=10+modI;

        if(modI==0)
            flagBorderL =true;
        if(modI==9)
            flagBorderR = true;

        for(int i=0;i<offsets.Length;i+=2)
        {
            int x = offsets[i];
            int y = offsets[i+1];

            int mGridPos = position+(-(y*10)+x);

            int modA = mGridPos%10;

            if(modA<0) modA=10+modA;

            if(modA==9)
            {
                
                flagBorderR=true;
            }
            if(modA==0)
            {
                
                flagBorderL=true;
            }

            if(HasMinoOnGridPos(mGridPos))
            {
                return false;
            }
        }
        if(flagBorderL&&flagBorderR)
            return false;

        return true;
    }

    public bool LineFull(int lineStart)
    {

        for(int i =0;i<10;i++)
        {
            if(!gridMap.ContainsKey(lineStart+i))
            {
                return false;
            }
        }
        return true;
    }
    public void LineGravity(int clearedLine)
    {
        for(int i =clearedLine-10;i>-20;i-=10)
        {
            for(int x = 0;x<10;x++)
            {
                if(gridMap.ContainsKey(i+x))
                {
                    var temp = gridMap[i+x];
                    gridMap.Remove(i+x);
                    gridMap.Add(i+x+10,temp);
                    temp.SetAbsolutePos(GridPosToVec(i+x+10));
                }
            }
        }
    }



    public void ClearLines()
    {
        for(int i=0;i<LAST_GRID_POS+1;i+=10)
        {
            if(LineFull(i))
            {
                for(int x =0;x<10;x++)
                {
                    gridMap[i+x].Clear();
                    gridMap.Remove(i+x);
                }
                LineGravity(i);
            }
        }
    }


    public void Hold()
    {
        if(!holdAllowed)
            return;
        Vector2 holPosV = holdPos.position;
        if(hold==Tetrimino.NONE)
        {
            hold = active.tetrimino;
            holdPrev = GameObject.Instantiate(propertyContainer.GetPropertiesOf(hold).prefab,holPosV,transform.rotation);
            var cont = holdPrev.GetComponent<TetriminoController>();
            cont.prev = true;
            cont.tetrimino = hold;
            cont.Shape();
            holdPrev.transform.position = holPosV+ cont.Properties.previewCenterOffset;
            GameObject.Destroy(active.gameObject);
            msCount =0;
            lockDelayMsCount =0;
            flagLockDelayActive=false;
            NextPiece();
        }
        else
        {
            Tetrimino tetri = hold;
            hold = active.tetrimino;
            GameObject.Destroy(active.gameObject);
            GameObject.Destroy(holdPrev);
            var VecPos = new Vector2(transform.position.x+gridStartCellOffsetX+4,transform.position.y+gridStartCellOffsetY);
            var nPiece = GameObject.Instantiate(propertyContainer.GetPropertiesOf(tetri).prefab,VecPos,transform.rotation);
            nPiece.transform.parent = transform;
            if(activeGhost!=null)
                GameObject.Destroy(activeGhost.gameObject);
            active = nPiece.GetComponent<TetriminoController>();
            active.tetrimino = tetri;
            active.Shape();
            activeGridPos = 4;
            var offsets = active.Properties.GetOffsets(active.rotation);
            if(HasMinoOnGridPos(activeGridPos))
            {
                GameOver();
                return;
            }
            foreach (var x in offsets)
            {
                if(HasMinoOnGridPos(activeGridPos+x))
                {
                    GameOver();
                    return;
                }
            }
            var ghostPiece = GameObject.Instantiate(propertyContainer.GetPropertiesOf(tetri).prefab,VecPos,transform.rotation);
            var ghostController = ghostPiece.GetComponent<TetriminoController>();
            ghostController.tetrimino = tetri;
            ghostController.ghost = true;
            ghostController.Shape();
            ghostController.SetPosition(GridPosToVec(GetGroundPos()));
            activeGhost = ghostController;

            msCount =0;
            lockDelayMsCount =0;
            flagLockDelayActive=false;
            holdPrev = GameObject.Instantiate(propertyContainer.GetPropertiesOf(hold).prefab,holPosV,transform.rotation);
            var cont = holdPrev.GetComponent<TetriminoController>();
            cont.prev = true;
            cont.tetrimino = hold;
            cont.Shape();
            holdPrev.transform.position = holPosV+ cont.Properties.previewCenterOffset;
        }
        holdAllowed = false;
    }

    public void GreyOut()
    {
        foreach(var x in gridMap.Values)
        {
            x.SetColor(Color.gray);
        }
        active.GreyOut();
    }

}
