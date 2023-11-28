/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("")]
  public class SimpleFacingCameraCallbacks_MiniLauncher : MonoBehaviour {

    public Transform toFaceCamera;
    public Camera cameraToFace;

    private bool _initialized = false;
    private bool _isFacingCamera = false;

    public UnityEvent OnBeginFacingCamera;
    public UnityEvent OnEndFacingCamera;

    void Start() {
      if (toFaceCamera != null) initialize();
    }

    private void initialize() {
      if(cameraToFace == null) { cameraToFace = Camera.main; }
      // Set "_isFacingCamera" to be whatever the current state ISN'T, so that we are
      // guaranteed to fire a UnityEvent on the first initialized Update().
      _isFacingCamera = !GetIsFacingCamera(toFaceCamera, cameraToFace);
      _initialized = true;
    }

    void Update() {

      if (toFaceCamera != null && !_initialized)
      {
            initialize();
      }

      if (!_initialized) return;

        if (GetIsFacingCamera(toFaceCamera, cameraToFace) != _isFacingCamera)
        {
            _isFacingCamera = !_isFacingCamera;

            if (_isFacingCamera) {
              OnBeginFacingCamera.Invoke();
            }
            else {
              OnEndFacingCamera.Invoke();
            }
        }
    }

    public static bool GetIsFacingCamera(Transform facingTransform, Camera camera)
    { 
            float res = Vector3.Dot((camera.transform.position - facingTransform.position).normalized, facingTransform.forward);

            //if (res < -0.72f) // Old value for Palm
            if (res < -0.50f)   // Correct value for Wrist
            return false;
            else
                return true;
    }
  }

