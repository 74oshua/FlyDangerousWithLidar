using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Player;
using UnityEngine.UI;
using Misc;

[RequireComponent(typeof(Camera))]
public class ExternalCamera : MonoBehaviour
{
    private Camera _externalCamera;
    private RenderTexture _outputTexture;

    // public ref RenderTexture outputTexture => ref _outputTexture;

    public RawImage outputImage = null;

    void Start()
    {
        _externalCamera = GetComponent<Camera>();
        _outputTexture = new RenderTexture(256, 256, 1, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
        _externalCamera.targetTexture = _outputTexture;

        if (outputImage != null)
        {
            outputImage.texture = _outputTexture;
        }
    }

    void Update()
    {
        // rotate external camera to face our direction of travel
        // if velocity is zero, face forward
        var player = FdPlayer.FindLocalShipPlayer;
        if (player == null)
        {
            return;
        }

        var velocity = player.ShipPhysics.Velocity;
        var up = Camera.main.transform.up;

        _externalCamera.transform.LookAt(_externalCamera.transform.position + velocity, up);
        if (velocity.magnitude < 0.0001f)
        {
            _externalCamera.transform.LookAt(Camera.main.transform.forward, up);
        }
    }
}
