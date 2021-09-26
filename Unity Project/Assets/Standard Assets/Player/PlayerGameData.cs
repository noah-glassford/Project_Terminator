using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerGameData : MonoBehaviour
{
    public Text roundDisplay;
    public Text[] pointDisplay;

    public GameManager gm;

    public int playerCount;

    public int playerNumber = 0;
    public int currentRound;
    public int currentPoints;


    private void Start()
    { 
    }

    public void setData(GameManager _gm, int _playerCount) {
        SetGM(_gm);
        SetPlayerCount(_playerCount);
    }

    private void SetGM(GameManager g) {

        gm = g;

    }
    private void SetPlayerCount(int p) {
        playerCount = p;
        for (int i = 0; i < playerCount; i++) {
            pointDisplay[i].text = "500";
        }
    }
}
