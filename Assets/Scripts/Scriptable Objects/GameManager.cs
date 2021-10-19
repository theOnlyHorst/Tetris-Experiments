using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Tetris/GameManager")]
public class GameManager : ScriptableObject
{
#region RNG

    private int[] bag1 = null;
    private int[] bag2 = null;

    private int bagPtr = 0;
    [SerializeField]
    private TetriminoPropertyContainer propertyContainer;

    public Tetrimino GetNextPiece()
    {
        if(bag1==null)
            bag1 = makeBag();
        if(bag2==null||bag2.Length==0)
            bag2 = makeBag();
        if(bagPtr==7)
        {
            bagPtr =0;
            bag1 = bag2;
            bag2 = null;
        }
        Tetrimino ret = (Tetrimino)bag1[bagPtr];
        bagPtr++;
        return ret;

    }

    public void clearBags()
    {
        bag1 = null;
        bag2 =null;
        bagPtr = 0;
    }
    private int[] makeBag()
    {
        int[] ret = Enumerable.Range(0,7).OrderBy(t => Random.value).ToArray();

        return ret;
    }


#endregion

#region GAME-EVENTS

    public UnityEvent OnGameEnd;

    public UnityEvent OnGameStart;


#endregion

#region GAME-RULESETS

    public struct GameRuleset
    {

    }


#endregion
#region LEVEL-DEFINITIONS
    public struct Level{
        public int dropSpeedMS;
        public float scoreMult;
        public int lockDelayMS;
    }

    public Level[] levels = {
        new Level{dropSpeedMS = 500, scoreMult=1,lockDelayMS=1000},
    };

    public Level GetLevel(int num)
    {
        return levels[num-1];
    }
#endregion

    void OnEnable()
    {
        bagPtr =0;
        bag1 = null;
        bag2 = null;
        currentState = GameState.Ended;
    }
#region States

    public enum GameState
    {
        Running,
        Paused,
        Ended
    }
    public GameState currentState;



#endregion
#region GridLogic

    private GridController actController;
    public void RegisterGrid(GridController controller)
    {
        actController = controller;
    }

    public void EndGame()
    {
        currentState = GameState.Ended;
        actController.EndGame();
        uIManager.ShowGameOver();
    }

    public void StartGame()
    {
        if(currentState!=GameState.Ended)
            OnGameEnd.Invoke();
        currentState = GameState.Running;
        actController.StartGame();
        uIManager.HideGameEnd();
    }


#endregion
#region UI
    public UIManager uIManager;

#endregion

}
