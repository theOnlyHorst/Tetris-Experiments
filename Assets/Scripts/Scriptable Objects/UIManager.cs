using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName="Tetris/UIManager")]
public class UIManager : ScriptableObject
{
    public Dictionary<string, Text> UITexts = new Dictionary<string, Text>();

    public void RegisterUIText(string componentName,Text registeredText)
    {
        UITexts[componentName] = registeredText;
    }


    public void ShowGameOver()
    {
        if(!UITexts.ContainsKey("GameEndText"))
            return;
        UITexts["GameEndText"].enabled = true;
        UITexts["GameEndText"].text = "Game Over";
    }

    public void ShowVictory()
    {
        if(!UITexts.ContainsKey("GameEndText"))
            return;
        UITexts["GameEndText"].enabled = true;
        UITexts["GameEndText"].text = "You Won";
    }

    public void HideGameEnd()
    {
        if(!UITexts.ContainsKey("GameEndText"))
            return;
        UITexts["GameEndText"].enabled = false;
    }
}
