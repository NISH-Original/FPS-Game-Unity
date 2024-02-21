using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PrimaryDropdown : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] GunHandler ar15;
    [SerializeField] GunHandler shotgun;
    [SerializeField] GunHandler sniper;
    [SerializeField] GunHandler smg;
    [SerializeField] GunHandler lmg;
    
    public void DropdownSample(int index)
    {
        switch(index)
        {
            case 0:
                playerController.items[0].gunObject.SetActive(false);
                playerController.items[0] = ar15;
                playerController.items[0].gunObject.SetActive(true);
                break;
            case 1:
                playerController.items[0].gunObject.SetActive(false);
                playerController.items[0] = shotgun;
                playerController.items[0].gunObject.SetActive(true);
                break;
            case 2:
                playerController.items[0].gunObject.SetActive(false);
                playerController.items[0] = sniper;
                playerController.items[0].gunObject.SetActive(true);
                break;
            case 3:
                playerController.items[0].gunObject.SetActive(false);
                playerController.items[0] = smg;
                playerController.items[0].gunObject.SetActive(true);
                break;
            case 4:
                playerController.items[0].gunObject.SetActive(false);
                playerController.items[0] = lmg;
                playerController.items[0].gunObject.SetActive(true);
                break;
        }
        playerController.EquipItem(0);
    }
}
