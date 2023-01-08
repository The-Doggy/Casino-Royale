using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFollowCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // The only thing this does is make buttons in the main lobby scene look towards the camera while panning
        transform.LookAt(Camera.main.transform, Vector3.up);
    }
}
