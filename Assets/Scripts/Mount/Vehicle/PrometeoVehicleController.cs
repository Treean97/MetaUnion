/*
MESSAGE FROM CREATOR: This script was coded by Mena. You can use it in your games either these are commercial or
personal projects. You can even add or remove functions as you wish. However, you cannot sell copies of this
script by itself, since it is originally distributed as a free product.
I wish you the best for your project. Good luck!

P.S: If you need more cars, you can check my other vehicle assets on the Unity Asset Store, perhaps you could find
something useful for your game. Best regards, Mena.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrometeoVehicleController : MonoBehaviour, IMountMovement, IMountMovementConfigurable
{
    //CAR SETUP
    [Space(20)]
    [Space(10)]
    [Range(20, 190)]
    public int maxSpeed = 90; //The maximum speed that the car can reach in km/h.
    [Range(10, 120)]
    public int maxReverseSpeed = 45; //The maximum speed that the car can reach while going on reverse in km/h.
    [Range(1, 10)]
    public int accelerationMultiplier = 2; // How fast the car can accelerate. 1 is a slow acceleration and 10 is the fastest.
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27; // The maximum angle that the tires can reach while rotating the steering wheel.
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f; // How fast the steering wheel turns.
    [Space(10)]
    [Range(100, 600)]
    public int brakeForce = 350; // The strength of the wheel brakes.
    [Range(1, 10)]
    public int decelerationMultiplier = 2; // How fast the car decelerates when the user is not using the throttle.
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5; // How much grip the car loses when the user hit the handbrake.
    [Space(10)]
    public Vector3 bodyMassCenter;

    //WHEELS
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    //PARTICLE SYSTEMS
    [Space(20)]
    [Space(10)]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    [Space(10)]
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    //SPEED TEXT (UI)
    [Space(20)]
    [Space(10)]
    public bool useUI = false;
    public Text carSpeedText;

    //SOUNDS
    [Space(20)]
    [Space(10)]
    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    //CONTROLS
    [Space(20)]
    [Space(10)]
    public bool useTouchControls = false;
    public GameObject throttleButton;
    PrometeoTouchInput throttlePTI;
    public GameObject reverseButton;
    PrometeoTouchInput reversePTI;
    public GameObject turnRightButton;
    PrometeoTouchInput turnRightPTI;
    public GameObject turnLeftButton;
    PrometeoTouchInput turnLeftPTI;
    public GameObject handbrakeButton;
    PrometeoTouchInput handbrakePTI;

    //CAR DATA
    [HideInInspector] public float carSpeed;
    [HideInInspector] public bool isDrifting;
    [HideInInspector] public bool isTractionLocked;

    //PRIVATE VARIABLES
    Rigidbody carRigidbody;
    float steeringAxis;
    float throttleAxis;
    float driftingAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;
    bool touchControlsSetup = false;

    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;

    // Mount integration
    MountInput _Input;

    [Header("External Control")]
    public bool UseExternalControl = false;

    float _ExtThrottle;   // -1~1
    float _ExtSteer;      // -1~1
    bool  _ExtBrake;      // foot brake(= brakeTorque)
    bool  _ExtHandbrake;  // drift용(필요하면)

    void Awake()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        if (carRigidbody != null)
            carRigidbody.centerOfMass = bodyMassCenter;

        // 외부 조종
        UseExternalControl = true;
    }

    // IMountMovement
    public void SetInput(in MountInput input)
    {
        _Input = input;
    }

    public void FixedTick()
    {
        SetExternalInput(_Input.Throttle, _Input.Steer, _Input.Brake);
        ExternalFixedTick(Time.fixedDeltaTime);
    }

    // IMountMovementConfigurable
    public void ApplyData(MountDataSO data)
    {
        if (data is PrometeoVehicleDataSO v)
        {
            maxSpeed = v.MaxSpeed;
            maxReverseSpeed = v.MaxReverseSpeed;
            accelerationMultiplier = v.AccelerationMultiplier;
            maxSteeringAngle = v.MaxSteeringAngle;
            steeringSpeed = v.SteeringSpeed;
            brakeForce = v.BrakeForce;
            decelerationMultiplier = v.DecelerationMultiplier;
            handbrakeDriftMultiplier = v.HandbrakeDriftMultiplier;
            bodyMassCenter = v.BodyMassCenter;

            if (carRigidbody != null)
                carRigidbody.centerOfMass = bodyMassCenter;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // ===== 원본 로직 유지 =====
        // carRigidbody는 Awake에서 이미 잡았지만, 원본 흐름 유지 위해 남겨둠(중복 대입은 문제 없음)
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;

        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;

        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;

        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

        if (carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        if (useUI)
        {
            InvokeRepeating("CarSpeedUI", 0f, 0.1f);
        }
        else if (!useUI)
        {
            if (carSpeedText != null)
            {
                carSpeedText.text = "0";
            }
        }

        if (useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else if (!useSounds)
        {
            if (carEngineSound != null) carEngineSound.Stop();
            if (tireScreechSound != null) tireScreechSound.Stop();
        }

        if (!useEffects)
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null) RRWParticleSystem.Stop();
            if (RLWTireSkid != null) RLWTireSkid.emitting = false;
            if (RRWTireSkid != null) RRWTireSkid.emitting = false;
        }

        if (useTouchControls)
        {
            if (throttleButton != null && reverseButton != null &&
                turnRightButton != null && turnLeftButton != null &&
                handbrakeButton != null)
            {
                throttlePTI = throttleButton.GetComponent<PrometeoTouchInput>();
                reversePTI = reverseButton.GetComponent<PrometeoTouchInput>();
                turnLeftPTI = turnLeftButton.GetComponent<PrometeoTouchInput>();
                turnRightPTI = turnRightButton.GetComponent<PrometeoTouchInput>();
                handbrakePTI = handbrakeButton.GetComponent<PrometeoTouchInput>();
                touchControlsSetup = true;
            }
            else
            {
                String ex = "Touch controls are not completely set up. You must drag and drop your scene buttons in the" +
                            " PrometeoCarController component.";
                Debug.LogWarning(ex);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // CAR DATA 갱신(사운드/UI가 이 값 씀)
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        // 외부 제어 모드면 여기서 입력 처리(키보드/터치) 절대 안 함
        if (!UseExternalControl)
        {
            if (useTouchControls && touchControlsSetup)
            {
                if (throttlePTI.buttonPressed) { CancelInvoke("DecelerateCar"); deceleratingCar = false; GoForward(); }
                if (reversePTI.buttonPressed) { CancelInvoke("DecelerateCar"); deceleratingCar = false; GoReverse(); }

                if (turnLeftPTI.buttonPressed) TurnLeft();
                if (turnRightPTI.buttonPressed) TurnRight();

                if (handbrakePTI.buttonPressed) { CancelInvoke("DecelerateCar"); deceleratingCar = false; Handbrake(); }
                if (!handbrakePTI.buttonPressed) RecoverTraction();

                if ((!throttlePTI.buttonPressed && !reversePTI.buttonPressed)) ThrottleOff();

                if ((!reversePTI.buttonPressed && !throttlePTI.buttonPressed) && !handbrakePTI.buttonPressed && !deceleratingCar)
                {
                    InvokeRepeating("DecelerateCar", 0f, 0.1f);
                    deceleratingCar = true;
                }

                if (!turnLeftPTI.buttonPressed && !turnRightPTI.buttonPressed && steeringAxis != 0f) ResetSteeringAngle();
            }
            else
            {
                if (Input.GetKey(KeyCode.W)) { CancelInvoke("DecelerateCar"); deceleratingCar = false; GoForward(); }
                if (Input.GetKey(KeyCode.S)) { CancelInvoke("DecelerateCar"); deceleratingCar = false; GoReverse(); }

                if (Input.GetKey(KeyCode.A)) TurnLeft();
                if (Input.GetKey(KeyCode.D)) TurnRight();

                if (Input.GetKey(KeyCode.Space)) { CancelInvoke("DecelerateCar"); deceleratingCar = false; Handbrake(); }
                if (Input.GetKeyUp(KeyCode.Space)) RecoverTraction();

                if ((!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))) ThrottleOff();

                if ((!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) && !Input.GetKey(KeyCode.Space) && !deceleratingCar)
                {
                    InvokeRepeating("DecelerateCar", 0f, 0.1f);
                    deceleratingCar = true;
                }

                if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && steeringAxis != 0f) ResetSteeringAngle();
            }
        }

        // 휠 메시는 항상 갱신
        AnimateWheelMeshes();
    }

    public void CarSpeedUI()
    {
        if (useUI)
        {
            try
            {
                float absoluteCarSpeed = Mathf.Abs(carSpeed);
                carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }

    public void CarSounds()
    {
        if (useSounds)
        {
            try
            {
                if (carEngineSound != null)
                {
                    float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);
                    carEngineSound.pitch = engineSoundPitch;
                }
                if ((isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f))
                {
                    if (!tireScreechSound.isPlaying)
                    {
                        tireScreechSound.Play();
                    }
                }
                else if ((!isDrifting) && (!isTractionLocked || Mathf.Abs(carSpeed) < 12f))
                {
                    tireScreechSound.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else if (!useSounds)
        {
            if (carEngineSound != null && carEngineSound.isPlaying) carEngineSound.Stop();
            if (tireScreechSound != null && tireScreechSound.isPlaying) tireScreechSound.Stop();
        }
    }

    //STEERING METHODS
    public void TurnLeft()
    {
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis < -1f) steeringAxis = -1f;

        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight()
    {
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis > 1f) steeringAxis = 1f;

        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f) steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        else if (steeringAxis > 0f) steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);

        if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f) steeringAxis = 0f;

        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    void AnimateWheelMeshes()
    {
        try
        {
            Quaternion FLWRotation;
            Vector3 FLWPosition;
            frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
            frontLeftMesh.transform.position = FLWPosition;
            frontLeftMesh.transform.rotation = FLWRotation;

            Quaternion FRWRotation;
            Vector3 FRWPosition;
            frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
            frontRightMesh.transform.position = FRWPosition;
            frontRightMesh.transform.rotation = FRWRotation;

            Quaternion RLWRotation;
            Vector3 RLWPosition;
            rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
            rearLeftMesh.transform.position = RLWPosition;
            rearLeftMesh.transform.rotation = RLWRotation;

            Quaternion RRWRotation;
            Vector3 RRWPosition;
            rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
            rearRightMesh.transform.position = RRWPosition;
            rearRightMesh.transform.rotation = RRWRotation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    //ENGINE AND BRAKING METHODS
    public void GoForward()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f) { isDrifting = true; DriftCarPS(); }
        else { isDrifting = false; DriftCarPS(); }

        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f) throttleAxis = 1f;

        if (localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void GoReverse()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f) { isDrifting = true; DriftCarPS(); }
        else { isDrifting = false; DriftCarPS(); }

        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if (throttleAxis < -1f) throttleAxis = -1f;

        if (localVelocityZ > 1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f) { isDrifting = true; DriftCarPS(); }
        else { isDrifting = false; DriftCarPS(); }

        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f) throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            else if (throttleAxis < 0f) throttleAxis = throttleAxis + (Time.deltaTime * 10f);

            if (Mathf.Abs(throttleAxis) < 0.15f) throttleAxis = 0f;
        }

        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));

        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;

        if (carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    public void Brakes()
    {
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");

        driftingAxis = driftingAxis + (Time.deltaTime);
        float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < FLWextremumSlip)
            driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);

        if (driftingAxis > 1f) driftingAxis = 1f;

        if (Mathf.Abs(localVelocityX) > 2.5f) isDrifting = true;
        else isDrifting = false;

        if (driftingAxis < 1f)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;
        }

        isTractionLocked = true;
        DriftCarPS();
    }

    public void DriftCarPS()
    {
        if (useEffects)
        {
            try
            {
                if (isDrifting) { RLWParticleSystem.Play(); RRWParticleSystem.Play(); }
                else { RLWParticleSystem.Stop(); RRWParticleSystem.Stop(); }
            }
            catch (Exception ex) { Debug.LogWarning(ex); }

            try
            {
                if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f)
                {
                    RLWTireSkid.emitting = true;
                    RRWTireSkid.emitting = true;
                }
                else
                {
                    RLWTireSkid.emitting = false;
                    RRWTireSkid.emitting = false;
                }
            }
            catch (Exception ex) { Debug.LogWarning(ex); }
        }
        else if (!useEffects)
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null) RRWParticleSystem.Stop();
            if (RLWTireSkid != null) RLWTireSkid.emitting = false;
            if (RRWTireSkid != null) RRWTireSkid.emitting = false;
        }
    }

    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f) driftingAxis = 0f;

        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            Invoke("RecoverTraction", Time.deltaTime);
        }
        else if (FLwheelFriction.extremumSlip < FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            driftingAxis = 0f;
        }
    }

    // ===== External 전용: Invoke 누적 방지(원본 RecoverTraction은 그대로) =====
    void RecoverTraction_NoInvoke(float dt)
    {
        isTractionLocked = false;

        driftingAxis = driftingAxis - (dt / 1.5f);
        if (driftingAxis < 0f) driftingAxis = 0f;

        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;
        }
        else if (FLwheelFriction.extremumSlip < FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            driftingAxis = 0f;
        }
    }

    // ===== External control API =====
    public void SetExternalInput(float throttle, float steer, bool brake, bool handbrake = false)
    {
        _ExtThrottle = Mathf.Clamp(throttle, -1f, 1f);
        _ExtSteer = Mathf.Clamp(steer, -1f, 1f);
        _ExtBrake = brake;
        _ExtHandbrake = handbrake;
    }

    // MountEntity.FixedUpdate에서만 호출할 것(오너만)
    public void ExternalFixedTick(float dt)
    {
        // CAR DATA 최신화(원본 Update에서 하던 것과 동일)
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        // 1) 조향(아날로그)
        steeringAxis = Mathf.MoveTowards(steeringAxis, _ExtSteer, dt * 10f * steeringSpeed);
        float steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);

        // 2) 브레이크(풋브레이크) / 핸드브레이크(드리프트)
        if (_ExtHandbrake)
        {
            CancelInvoke("RecoverTraction");
            Handbrake();
        }
        else
        {
            // (중요) External 모드에서는 Invoke 누적 방지 버전 사용
            RecoverTraction_NoInvoke(dt);
        }

        if (_ExtBrake)
        {
            frontLeftCollider.motorTorque = 0;
            frontRightCollider.motorTorque = 0;
            rearLeftCollider.motorTorque = 0;
            rearRightCollider.motorTorque = 0;

            Brakes();
            return;
        }
        else
        {
            frontLeftCollider.brakeTorque = 0;
            frontRightCollider.brakeTorque = 0;
            rearLeftCollider.brakeTorque = 0;
            rearRightCollider.brakeTorque = 0;
        }

        // 3) 스로틀(아날로그)
        throttleAxis = Mathf.MoveTowards(throttleAxis, _ExtThrottle, dt * 3f);

        // 드리프트 판단/이펙트(원본 방식 유지)
        if (Mathf.Abs(localVelocityX) > 2.5f) isDrifting = true;
        else isDrifting = false;
        DriftCarPS();

        // 입력이 거의 없으면 감속(원본 DecelerateCar 동작을 고정 dt로 재현)
        if (Mathf.Abs(_ExtThrottle) < 0.05f)
        {
            if (throttleAxis > 0f) throttleAxis = Mathf.Max(0f, throttleAxis - dt * 10f);
            else if (throttleAxis < 0f) throttleAxis = Mathf.Min(0f, throttleAxis + dt * 10f);

            carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));

            frontLeftCollider.motorTorque = 0;
            frontRightCollider.motorTorque = 0;
            rearLeftCollider.motorTorque = 0;
            rearRightCollider.motorTorque = 0;
            return;
        }

        // 4) 전/후진 토크 적용(원본 속도 제한 로직 유지)
        if (_ExtThrottle > 0.05f && localVelocityZ < -1f)
        {
            Brakes();
            return;
        }
        if (_ExtThrottle < -0.05f && localVelocityZ > 1f)
        {
            Brakes();
            return;
        }

        float motor = (accelerationMultiplier * 50f) * throttleAxis;

        if (_ExtThrottle > 0.05f)
        {
            if (Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                frontLeftCollider.motorTorque = motor;
                frontRightCollider.motorTorque = motor;
                rearLeftCollider.motorTorque = motor;
                rearRightCollider.motorTorque = motor;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
        else // reverse
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                frontLeftCollider.motorTorque = motor;
                frontRightCollider.motorTorque = motor;
                rearLeftCollider.motorTorque = motor;
                rearRightCollider.motorTorque = motor;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }
}
