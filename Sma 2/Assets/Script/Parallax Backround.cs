using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackround : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float parallaxEffectMultiplierX;
    [SerializeField]
    [Range(0.01f, 1f)]
    private float parallaxEffectMultiplierY;
    private void Start()
    {
        
        cameraTransform = GameObject.FindGameObjectWithTag("Vcam").transform;
        lastCameraPosition = cameraTransform.position;
    }
    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplierX, deltaMovement.y * parallaxEffectMultiplierY);
        lastCameraPosition = cameraTransform.position;
    }
}
