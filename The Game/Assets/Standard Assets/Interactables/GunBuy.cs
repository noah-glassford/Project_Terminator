using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBuy : Interactablle
{
    public GameObject gun;

    public string gunToBuy = "Default Gun Name";
    public int gunPrice = 5000;

    public override string getDescription()
    {
        return "Press [E] To Purchase " + gunToBuy + " For " + gunPrice.ToString() + " Gears";
    }
    public override void Interact(PlayerGameData pgd)
    {
        if (pgd.currentPoints >= gunPrice)
        {
            pgd.currentPoints -= gunPrice;

            pgd.gunM.addNewGun(gun, pgd.gunM.index);
        }
    }
}
