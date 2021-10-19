using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tetris/TetriminoPropertyContainer")]
public class TetriminoPropertyContainer : ScriptableObject
{
    [SerializeField]
    List<TetriminoPropertyTeriminoPair> propList;

    public RotationOffsetTable commonOffTable;

    public RotationOffsetTable iOffTable;


    public RotationOffsetTable GetOffsetTable(Tetrimino tet)
    {
        if(tet==Tetrimino.I)
            return iOffTable;
        return commonOffTable;
    }

    [Serializable]
    public class TetriminoProperties
    {
        [SerializeField]
        public Color color;
        [SerializeField]
        public GameObject prefab;

        public Vector2 previewCenterOffset;
        
        [SerializeField]
        public Rotation[] rotations;

        public int[] GetOffsets(int rotation)
        {
            return new int[] {rotations[rotation].offsetM1X,rotations[rotation].offsetM1Y,rotations[rotation].offsetM2X,rotations[rotation].offsetM2Y,rotations[rotation].offsetM3X,rotations[rotation].offsetM3Y};
        }

        
    }
    [Serializable]
    class TetriminoPropertyTeriminoPair
    {
        [SerializeField]
        string name;
        [SerializeField]
        public Tetrimino tetrimino;
        [SerializeField]
        public TetriminoProperties tetriminoProperties;
    }

    public TetriminoProperties GetPropertiesOf(Tetrimino tet)
    {
        return propList.Find(tetri => tetri.tetrimino ==tet).tetriminoProperties;
    }
    
    [Serializable]
    public struct Rotation
    {   
        [SerializeField]
        private string name;
        [SerializeField]
        public int offsetM1X;
        [SerializeField]
        public int offsetM1Y;
        [SerializeField]
        public int offsetM2X;
        [SerializeField]
        public int offsetM2Y;
        [SerializeField]
        public int offsetM3X;
        [SerializeField]
        public int offsetM3Y;

    }
    [Serializable]
    public struct RotationOffsetTable
    {
        public RotationOffset OToR;
        public RotationOffset RToT;
        public RotationOffset TToL;
        public RotationOffset LToO;

    }

    [Serializable]
    public struct RotationOffset
    {
        public Vector2Int offset1;
        public Vector2Int offset2;
        public Vector2Int offset3;
        public Vector2Int offset4;
        public Vector2Int offset5;

    }
}
public enum Tetrimino{

        I,
        O,
        S,
        Z,
        J,
        L,
        T,
        NONE
}