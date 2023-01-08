using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuCamera : MonoBehaviour
{
    // Controls the speed that the camera will rotate
    [SerializeField] private float speed = 3.5f;

    // Update is called once per frame
    void Update()
    {
        // Make sure the player is touching the screen
        if (Touchscreen.current.primaryTouch.press.isPressed && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            // If the player is touching the left half of the screen direction should be -1.0 otherwise direction should be 1.0
            float direction = 1f;
            if(Camera.main.ScreenToViewportPoint(Touchscreen.current.primaryTouch.position.ReadValue()).x <= 0.5f)
            {
                direction = -1f;
            }

            // Rotate the camera towards the direction the player is touching
            transform.Rotate(0f, direction * speed, 0f);
        }
    }
}
