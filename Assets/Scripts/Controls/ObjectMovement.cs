using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using UnityEngine;


/// <summary>
/// Controls all object Movement.
/// Click and drag to move objects.
/// Two fingers on object can rotate it.
/// Long press without Movement picks the object up.
/// </summary>
public class ObjectMovement : MonoBehaviour
{
    private struct TouchIdPlusOffset 
    {
        public int id;
        public Vector3 offset;
        
        public TouchIdPlusOffset(int id, Vector3 offset)
        {
            this.offset = offset;
            this.id = id;
        }
    }
    
    //which heigth will the object be dragged at/picked up
    [SerializeField] private float pickUpHeight;
    [SerializeField] private float dragHeight;
    [SerializeField] private float holdToPickUpTime = 0.5f; 

    private bool _pickedUp, _dragging;
    private Camera _mainCamera;
    private Rigidbody _objRb;

    private float _holdToPickUpTimer;
    private float _dragThreshold = 0.003f;
    private bool _hasDragged;
    

    private List<TouchIdPlusOffset> _objTouchIDWithOffset;
    private List<Vector2> _touchVectors;
    private List<Vector3> _touchOffsets;

    private Vector3 _oldRotationVector;
    private float _rotateAmount;
 
    
    private void Start()
    {
        _touchVectors = new List<Vector2>();
        _touchOffsets = new List<Vector3>();
        _objRb = GetComponent<Rigidbody>();
        _mainCamera = ObjectTouchInput.mainCamera;
    }

    private void OnEnable()
    {
        _objTouchIDWithOffset = new List<TouchIdPlusOffset>(); 
        ObjectTouchInput.OnPickUpObject += StartMovement;
        ObjectTouchInput.OnEndMovement  += EndMovement;
    }

    private void OnDisable()
    {
        ObjectTouchInput.OnPickUpObject -= StartMovement;
        ObjectTouchInput.OnEndMovement  -= EndMovement;
    }

    private void Update()
    {
        _touchOffsets.Clear();
        _touchVectors.Clear();
        
        for (int i = 0; i < _objTouchIDWithOffset.Count; ++i)
        {
            try
            { 
                var touchIdPlusOffset = _objTouchIDWithOffset[i];
                var first = Input.touches.First(x => x.fingerId == touchIdPlusOffset.id);
                _touchVectors.Add(first.position); //position of touch
                _touchOffsets.Add(transform.TransformPoint(touchIdPlusOffset.offset) - transform.position); // offset
            }
            catch (Exception e)
            {
                Console.WriteLine(e); // touch not found in touches, happens e.g. with mouse input 
            }
        }

        
#if UNITY_EDITOR
        
        //Handle Mouse Input
        for (int i = 0; i < _objTouchIDWithOffset.Count; ++i)
        {
            var touchIdPlusOffset = _objTouchIDWithOffset[i];
            if (ObjectTouchInput.MOUSEFINGERID == touchIdPlusOffset.id)
            {
                _touchVectors.Add(Input.mousePosition); //position of touch
                _touchOffsets.Add(transform.TransformPoint(touchIdPlusOffset.offset) - transform.position); // offset
            }
        }
#endif
        
        //multi tap == rotation
        if (_touchVectors.Count > 1)
        {
            var rotationVector = _touchVectors[0] - _touchVectors[1];
            
            _rotateAmount = Vector2.SignedAngle(rotationVector.normalized, _oldRotationVector.normalized);
            _oldRotationVector = rotationVector;
            if (Mathf.Abs(_rotateAmount) > 15) _rotateAmount = 0; //prevent big jumps
            transform.Rotate(Vector3.up, _rotateAmount);
        }
        else
        {
            _rotateAmount = 0;
        }

    }

    private void FixedUpdate()
    {


        //Single tap == movement
        if (_touchVectors.Count > 0)
        {
            if (_pickedUp) MoveTo(_touchVectors[0], pickUpHeight, _touchOffsets[0]);
            else if (_dragging) MoveTo(_touchVectors[0], dragHeight, _touchOffsets[0]);
        }
        
        PickUpCheck();
        
    }

    private void PickUpCheck()
    {
        if (_dragging && !_hasDragged)
        {
            if (holdToPickUpTime < _holdToPickUpTimer) 
            {
                //Start pickup
                _dragging = false;
                _pickedUp = true;

                transform.position = new Vector3(transform.position.x, pickUpHeight, transform.position.z);
                _objRb.constraints |= RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ ;
            }
            else
            {
                _holdToPickUpTimer += Time.fixedDeltaTime;
            }
        }
        else
        {
            _holdToPickUpTimer = 0;
        }
        
        //Detect when the user drags, timer check is used here to negate initial touch wobbles
        if (_dragging && _objRb.velocity.magnitude > _dragThreshold && _holdToPickUpTimer > 0.1f)    _hasDragged = true;
        if (!_dragging && !_pickedUp)    _hasDragged = false;
    }

    private void MoveTo(Vector2 moveToScreenVector,float moveHeight, Vector3 offsetInWorldSpace)
    {
        Vector3 _newPos =   _mainCamera.ScreenToWorldPoint(new Vector3
                    {
                        x = moveToScreenVector.x,
                        y = moveToScreenVector.y,
                        z = _mainCamera.transform.position.y - moveHeight
                    }); //Screen vector to world with correct height
        Vector3 force = _newPos - (transform.position + offsetInWorldSpace);
        
        _objRb.AddForceAtPosition(force,transform.position + offsetInWorldSpace );
    }

    private void StartMovement(Touch touch, GameObject dragObject)
    {
        if (dragObject == this.gameObject)
        {
            if (_objTouchIDWithOffset.Count == 0)    StateChange(touch, true);
            else    AddTouchWithOffset(touch);
        }
    }

    private void EndMovement(Touch touch, GameObject dragObject)
    {

        if(_objTouchIDWithOffset.Exists(x => x.id == touch.fingerId))
        {
            var first = _objTouchIDWithOffset.First(x => x.id == touch.fingerId);
            RemoveTouch(first.id);
            if (_objTouchIDWithOffset.Count == 0)    StateChange(touch, false);
        }

    }
    
    private void StateChange(Touch touch, bool drag)
    {

        _dragging = drag;
        
        if (drag) // movement state
        {
            AddTouchWithOffset(touch);
        }
        else // end movement State
        {
            _objRb.constraints = RigidbodyConstraints.None;
            _objTouchIDWithOffset.Clear();

        }
    }

    private void AddTouchWithOffset(Touch touch)
    {
        float objectDistanceFromCamera = _mainCamera.transform.position.y - transform.position.y;
        Vector3 screenToWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, objectDistanceFromCamera));
        var objectOriginToTouchOffset = transform.InverseTransformPoint(new Vector3(screenToWorldPos.x, transform.position.y, screenToWorldPos.z));
        _objTouchIDWithOffset.Add(new TouchIdPlusOffset(touch.fingerId, objectOriginToTouchOffset));
    }
    
    private void RemoveTouch(int touchID)
    {
        _objTouchIDWithOffset.Remove(_objTouchIDWithOffset.First(x => x.id == touchID));
    }
}
