using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

/// <summary>
/// Controls all 3D Scene Input
/// Conveys touch information to the touched movable object. 
/// </summary>
public class ObjectTouchInput : MonoBehaviour
{

    public static Camera mainCamera;
    
    public delegate void PickUpInput(Touch touch, GameObject pickUpObject);
    public static event PickUpInput OnPickUpObject;
    public delegate void EndMovement(Touch touch, GameObject pickUpObject);
    public static event EndMovement OnEndMovement;

    private int maxTapCount = 0;
    private string multiTouchInfo;
    
    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

    }

#if UNITY_EDITOR
    public static int MOUSEFINGERID = 999; 
#endif
    
    // Update is called once per frame
    void Update()
    {
        
        for (int i = 0; i < Input.touchCount; i++)
        {
            TouchInputHandling(Input.GetTouch(i));
        }
        
#if UNITY_EDITOR
        //Make touch testable on PC
        if (Input.GetMouseButtonDown(0))
        {
            Touch touch = new Touch();
            touch.position = Input.mousePosition;
            touch.phase = TouchPhase.Began;
            touch.fingerId = 999;
            TouchInputHandling(touch);
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            Touch touch = new Touch();
            touch.position = Input.mousePosition;
            touch.phase = TouchPhase.Ended;
            touch.fingerId = 999;
            TouchInputHandling(touch);
        }
        
#endif

    }

    private void TouchInputHandling(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            GameObject hitGO = PickObject(touch.position);
            OnPickUpObject?.Invoke(touch, hitGO);
        }

        if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
        {
            GameObject hitGO = PickObject(touch.position);
            OnEndMovement?.Invoke(touch, hitGO);
        }
    }


    /// <summary>
    /// Pick an object based on the mouse/touch position
    /// </summary>
    /// <returns> object if an object was hit, null if nothing was hit</returns>
    private GameObject PickObject(Vector2 screenPosition)
    {
        
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            
            int layer = LayerMask.NameToLayer("Movable");
            if (hit.transform.gameObject.layer == layer)
            {
                return hit.transform.gameObject;
            }
        }
        return null;
    }
}
