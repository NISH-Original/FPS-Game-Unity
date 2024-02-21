using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SecondaryDropdown : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] GunHandler m1911;
    [SerializeField] GunHandler tec9;
    
    public void DropdownSample(int index)
    {
        switch(index)
        {
            case 0:
                playerController.items[1].gunObject.SetActive(false);
                playerController.items[1] = m1911;
                playerController.items[1].gunObject.SetActive(true);
                break;
            case 1:
                playerController.items[1].gunObject.SetActive(false);
                playerController.items[1] = tec9;
                playerController.items[1].gunObject.SetActive(true);
                break;
        }

        playerController.EquipItem(1);
    }
}
