using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    CanvasGroup crosshairCanvas;
    [SerializeField] PlayerController pc;

    void Start()
    {
        crosshairCanvas = GetComponent<CanvasGroup>();
    }
    
    void Update()
    {
        if(pc.isScoped)
            crosshairCanvas.alpha = 0;
        else
            crosshairCanvas.alpha = 1;
    }
        
}
