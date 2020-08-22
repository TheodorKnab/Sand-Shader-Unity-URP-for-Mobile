using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MovmentAudio : MonoBehaviour
{
    public float loudnessScale = 1f;
    private float _triggerVelocity = 0.005f;
    
    private AudioSource _source;
    private ObjectMovement _objectMovement;
    private Rigidbody _rb;
    
    private float _currentLoudness = 0;
        
    // Start is called before the first frame update
    void Start()
    {
        _source = GetComponent<AudioSource>();
        _objectMovement = GetComponent<ObjectMovement>();
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontalVelocityMagnitude;
        horizontalVelocityMagnitude = Vector3.ProjectOnPlane(_rb.velocity, Vector3.up).magnitude;
        
        _currentLoudness = horizontalVelocityMagnitude * loudnessScale;
        _currentLoudness = Mathf.Clamp01(_currentLoudness);
        
        if (_currentLoudness > 0)
        {
            if (!_source.isPlaying)    _source.Play();
            _source.volume = _currentLoudness;
        }
        else
        {
            if (_source.isPlaying)    _source.Stop();
        }
    }
}
