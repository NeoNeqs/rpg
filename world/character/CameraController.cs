using System;
using RPG.global;
using Godot;

namespace RPG.world.character;

[GlobalClass]
public partial class CameraController : Node {
    private float _cameraSensitivity = 3.0f;
    private float _scrollSensitivity = 0.2f;

    private SpringArm3D Arm => Pivot.GetChild<SpringArm3D>(0);
    private PlayerCharacterBody Body => GetBody();
    private Node3D Pivot => Body.GetPivot();
    private Node3D Model => Body.GetModel();

    private bool _skipMotionEventOnce = true;
    private bool _freeLookEnabled;
    private bool _turnEnabled;
    private float _scrollVelocity;

    private const float MaxScrollVelocity = 1.3f;
    private const float SpringLengthDecay = 8.0f;
    private const float MinSpringLength = 0.5f;
    private const float MaxSpringLength = 9.0f;
    private const float MinArmPitch = -Mathf.Pi / 2f;
    private const float MaxArmPitch = Mathf.Pi / 4f;

    public override void _PhysicsProcess(double pDelta) {
        if (Input.IsActionJustPressed("camera_zoom_in")) {
            _scrollVelocity -= _scrollSensitivity;
        } else if (Input.IsActionJustPressed("camera_zoom_out") &&
                   Math.Abs(Arm.GetHitLength() - Arm.SpringLength) < 0.1) {
            _scrollVelocity += _scrollSensitivity;
        }

        _scrollVelocity = Mathf.MoveToward(_scrollVelocity, 0, (float)pDelta);
        _scrollVelocity = Mathf.Clamp(_scrollVelocity, -MaxScrollVelocity, MaxScrollVelocity);

        // ExpDecay feels smoother than Lerp
        Arm.SpringLength = ExpDecay(Arm.SpringLength, Arm.SpringLength + _scrollVelocity, (float)pDelta,
            SpringLengthDecay);
        Arm.SpringLength = Mathf.Clamp(Arm.SpringLength, MinSpringLength, MaxSpringLength);

        if (Arm.SpringLength is <= MinSpringLength or >= MaxSpringLength) {
            _scrollVelocity = 0;
        }
    }

    public override void _UnhandledInput(InputEvent pEvent) {
        switch (pEvent) {
            case InputEventMouseButton mouseButtonEvent:
                HandleMouseButton(mouseButtonEvent);

                break;
            case InputEventMouseMotion motionEvent:
                HandleMouseMotion(motionEvent);
                break;
        }
    }

    private void HandleMouseButton(InputEventMouseButton pEventMouseButton) {
        if (pEventMouseButton.IsAction("camera_free_look")) {
            _freeLookEnabled = pEventMouseButton.Pressed;
        }

        if (pEventMouseButton.IsAction("camera_turn")) {
            _turnEnabled = pEventMouseButton.Pressed;
        }

        // Yes, skip the first frame of camera motion. Otherwise, camera gets glitchy and bitchy about it.
        if (_freeLookEnabled || _turnEnabled) {
            _skipMotionEventOnce = true;
        } else {
            // Defer the state release by 1 frame. 
            // This allows AOE spells to be cast properly in PlayerCharacter
            MouseStateMachine.Instance.RequestStateDeferred(MouseStateMachine.State.Free);
        }
    }

    private void HandleMouseMotion(InputEventMouseMotion pEventMouseMotion) {
        // if (MouseStateMachine.Instance.CurrentState != MouseStateMachine.State.CameraControl) {
        //     return;
        // }

        // HACK: When mouse mode is set to Captured, USING ANY mouse button 
        //       triggers an InputEventMouseMotion with a relative Vector that 
        //       is unrealistically large, even though the mouse has not moved 
        //       (or can't move) that much. This causes the camera to rotate 
        //       rapidly in a short amount of time. The following flag prevents that.
        if (_skipMotionEventOnce) {
            _skipMotionEventOnce = false;
            return;
        }

        if (_freeLookEnabled || _turnEnabled) {
            MouseStateMachine.Instance.RequestState(MouseStateMachine.State.CameraControl);
            Pivot.RotateY(-pEventMouseMotion.Relative.X * _cameraSensitivity * 0.001f);
            Arm.RotateX(-pEventMouseMotion.Relative.Y * _cameraSensitivity * 0.001f);
            Arm.Rotation = Arm.Rotation with {
                X = Mathf.Clamp(Arm.Rotation.X, MinArmPitch, MaxArmPitch)
            };
        }

        if (_turnEnabled) {
            Model.RotateY(-pEventMouseMotion.Relative.X * _cameraSensitivity * 0.001f);

            // Note: `GetRotationQuaternion` already Orthonormalizes the rotation matrix
            Quaternion from = Model.GlobalBasis.GetRotationQuaternion();
            Quaternion to = Pivot.GlobalBasis.GetRotationQuaternion();
            if (from != to) {
                Model.GlobalBasis = new Basis(from.Slerp(to, 0.8f));
            }
        }
    }

    // Credit: https://github.com/FreyaHolmer
    // Best range for pDecay is [1; 25]
    // Source: https://youtu.be/LSNQuFEDOyQ?t=2978
    private static float ExpDecay(float pFrom, float pTo, float pDelta, float pDecay) {
        return pTo + (pFrom - pTo) * Mathf.Exp(-pDecay * pDelta);
    }

    private PlayerCharacterBody GetBody() {
        return GetParent<PlayerCharacterBody>();
    }
}