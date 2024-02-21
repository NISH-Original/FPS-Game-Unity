using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
    [Header("Gun Properties")]
    public bool isAutomatic;
    public float fireRate;
    public int pelletsPerAttack;
    public float spread;
    
    [Header("Scpoing")]
    public float scopeInSpeed;
    public float scopeZoomMult;
    public float scopeFOV;
    public Vector3 adsFirePos;
}
