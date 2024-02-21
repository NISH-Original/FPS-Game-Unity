using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    Renderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        meshRenderer.enabled = false;
    }
}
