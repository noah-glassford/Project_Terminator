using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gunManager : MonoBehaviour
{
    [Header("Gun Data")]
    public GameObject[] guns;

    [Header("UI Data")]
    public Text ammoDisplay;

    public Gun currentGun;

    private int index;

    bool isSwitching;

    private void Start()
    {
        InitializeWeapons();
    }

    private void InitializeWeapons() 
    {
        switchWeapons(0);
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && !isSwitching) {
            index++;
            if (index > guns.Length-1) index = 0;
            StartCoroutine(SwitchAfterDelay(index));
        }
        ammoDisplay.text = currentGun.roundsRemaining + "/" + currentGun.magazineSize;
    }

    private IEnumerator SwitchAfterDelay(int _index) {

        isSwitching = true;
        yield return new WaitForSeconds(0.5f);
        switchWeapons(_index);

        isSwitching = false;
    }

    private void switchWeapons(int newIndex) 
    {
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].SetActive(false);
        }
        guns[newIndex].SetActive(true);

        currentGun = guns[newIndex].GetComponentInChildren<Gun>();
        ammoDisplay.text = currentGun.roundsRemaining + "/" + currentGun.magazineSize;
    }
}
