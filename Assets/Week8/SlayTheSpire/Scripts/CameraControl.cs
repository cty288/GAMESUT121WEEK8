using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    [SerializeField] 
    private float cameraSpeed = 1;

    [SerializeField] 
    private float lerp = 0.1f;

    [SerializeField] private float minimumX = 0;
    private float targetY;
    private void Update() {
        if (Input.GetKey(KeyCode.W)) {
            targetY += cameraSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S)) {
            targetY -= cameraSpeed * Time.deltaTime;
        }

        targetY = Mathf.Max(minimumX, targetY);

        transform.position = Vector3.Lerp(transform.position,
            new Vector3(transform.position.x, targetY, -10), lerp);
    }
}
