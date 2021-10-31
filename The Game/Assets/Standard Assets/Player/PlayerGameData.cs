using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PlayerGameData : MonoBehaviour
{
    public Text roundDisplay;
    public Text[] pointDisplay;

    public GameManager gm;
    public gunManager gunM;

    public int playerCount;

    public int playerNumber = 0;
    public int currentRound;
    public int currentPoints;

    public DamageFlicker df;
    public int health = 6;


    private void Start()
    { 
    }

    public void setData(GameManager _gm, int _playerCount, int _playerNumber) {
        SetGM(_gm);
        SetPlayerCount(_playerCount);
        playerNumber = _playerNumber;
        gunM = gameObject.GetComponent<gunManager>();
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

    public void TakeDamage(int damage)
    {
        health -= damage;
        df.FadeDamage();
        if(health <= 0) {
            PlayerDie();
            }
    }
    private void PlayerDie() {
        //VERY TEMP || DON'T FORGET TO REMOVE THIS WHEN NETWORKING COMES UP

        SceneManager.LoadScene(0);
    }
}
