using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;

namespace KartGame.KartSystems
{
    public class ArcadeKart : MonoBehaviour
    {
        [System.Serializable]
        public class StatPowerup
        {
            public ArcadeKart.Stats modifiers;
            public string PowerUpID;
            public float ElapsedTime;
            public float MaxTime;
        }

        [System.Serializable]
        public struct Stats
        {
            [Header("Movement Settings")]
            [Min(0.001f), Tooltip("Top speed attainable when moving forward.")]
            public float TopSpeed;

            [Tooltip("How quickly the kart reaches top speed.")]
            public float Acceleration;

            [Min(0.001f), Tooltip("Top speed attainable when moving backward.")]
            public float ReverseSpeed;

            [Tooltip("How quickly the kart reaches top speed, when moving backward.")]
            public float ReverseAcceleration;

            [Tooltip("How quickly the kart starts accelerating from 0. A higher number means it accelerates faster sooner.")]
            [Range(0.2f, 1)]
            public float AccelerationCurve;

            [Tooltip("How quickly the kart slows down when the brake is applied.")]
            public float Braking;

            [Tooltip("How quickly the kart will reach a full stop when no inputs are made.")]
            public float CoastingDrag;

            [Range(0.0f, 1.0f)]
            [Tooltip("The amount of side-to-side friction.")]
            public float Grip;

            [Tooltip("How tightly the kart can turn left or right.")]
            public float Steer;

            [Tooltip("Additional gravity for when the kart is in the air.")]
            public float AddedGravity;

            // allow for stat adding for powerups.
            public static Stats operator +(Stats a, Stats b)
            {
                return new Stats
                {
                    Acceleration        = a.Acceleration + b.Acceleration,
                    AccelerationCurve   = a.AccelerationCurve + b.AccelerationCurve,
                    Braking             = a.Braking + b.Braking,
                    CoastingDrag        = a.CoastingDrag + b.CoastingDrag,
                    AddedGravity        = a.AddedGravity + b.AddedGravity,
                    Grip                = a.Grip + b.Grip,
                    ReverseAcceleration = a.ReverseAcceleration + b.ReverseAcceleration,
                    ReverseSpeed        = a.ReverseSpeed + b.ReverseSpeed,
                    TopSpeed            = a.TopSpeed + b.TopSpeed,
                    Steer               = a.Steer + b.Steer,
                };
            }
        }

        public Rigidbody Rigidbody { get; private set; }
        public InputData Input     { get; private set; }
        public float AirPercent    { get; private set; }
        public float GroundPercent { get; private set; }

        public ArcadeKart.Stats baseStats = new ArcadeKart.Stats
        {
            TopSpeed            = 10f,
            Acceleration        = 5f,
            AccelerationCurve   = 4f,
            Braking             = 10f,
            ReverseAcceleration = 5f,
            ReverseSpeed        = 5f,
            Steer               = 5f,
            CoastingDrag        = 4f,
            Grip                = .95f,
            AddedGravity        = 1f,
        };

        [Header("Vehicle Visual")] 
        public List<GameObject> m_VisualWheels;

        [Header("Vehicle Physics")]
        [Tooltip("The transform that determines the position of the kart's mass.")]
        public Transform CenterOfMass;

        [Range(0.0f, 20.0f), Tooltip("Coefficient used to reorient the kart in the air. The higher the number, the faster the kart will readjust itself along the horizontal plane.")]
        public float AirborneReorientationCoefficient = 3.0f;

        [Header("Drifting")]
        [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
        public float DriftGrip = 0.4f;
        [Range(0.0f, 10.0f), Tooltip("Additional steer when the kart is drifting.")]
        public float DriftAdditionalSteer = 5.0f;
        [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
        public float MinAngleToFinishDrift = 10.0f;
        [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
        public float MinSpeedPercentToFinishDrift = 0.5f;
        [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
        public float DriftControl = 10.0f;
        [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
        public float DriftDampening = 10.0f;

        [Header("VFX")]
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public ParticleSystem DriftSparkVFX;
        [Range(0.0f, 0.2f), Tooltip("Offset to displace the VFX to the side.")]
        public float DriftSparkHorizontalOffset = 0.1f;
        [Range(0.0f, 90.0f), Tooltip("Angle to rotate the VFX.")]
        public float DriftSparkRotation = 17.0f;
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public GameObject DriftTrailPrefab;
        [Range(-0.1f, 0.1f), Tooltip("Vertical to move the trails up or down and ensure they are above the ground.")]
        public float DriftTrailVerticalOffset;
        [Tooltip("VFX that will spawn upon landing, after a jump.")]
        public GameObject JumpVFX;
        [Tooltip("VFX that is spawn on the nozzles of the kart.")]
        public GameObject NozzleVFX;
        [Tooltip("List of the kart's nozzles.")]
        public List<Transform> Nozzles;

        [Header("Suspensions")]
        [Tooltip("The maximum extension possible between the kart's body and the wheels.")]
        [Range(0.0f, 1.0f)]
        public float SuspensionHeight = 0.2f;
        [Range(10.0f, 100000.0f), Tooltip("The higher the value, the stiffer the suspension will be.")]
        public float SuspensionSpring = 20000.0f;
        [Range(0.0f, 5000.0f), Tooltip("The higher the value, the faster the kart will stabilize itself.")]
        public float SuspensionDamp = 500.0f;
        [Tooltip("Vertical offset to adjust the position of the wheels relative to the kart's body.")]
        [Range(-1.0f, 1.0f)]
        public float WheelsPositionVerticalOffset = 0.0f;

        [Header("Physical Wheels")]
        [Tooltip("The physical representations of the Kart's wheels.")]
        public WheelCollider FrontLeftWheel;
        public WheelCollider FrontRightWheel;
        public WheelCollider RearLeftWheel;
        public WheelCollider RearRightWheel;

        [Tooltip("Which layers the wheels will detect.")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

        // the input sources that can control the kart
        IInput[] m_Inputs;

        const float k_NullInput = 0.01f;
        const float k_NullSpeed = 0.01f;
        Vector3 m_VerticalReference = Vector3.up;

        // Drift params
        public bool WantsToDrift { get; private set; } = false;
        public bool IsDrifting { get; private set; } = false;
        float m_CurrentGrip = 1.0f;
        float m_DriftTurningPower = 0.0f;
        float m_PreviousGroundPercent = 1.0f;
        readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();
        readonly List<(WheelCollider wheel, float horizontalOffset, float rotation, ParticleSystem sparks)> m_DriftSparkInstances = new List<(WheelCollider, float, float, ParticleSystem)>();

        // can the kart move?
        bool m_CanMove = true;
        List<StatPowerup> m_ActivePowerupList = new List<StatPowerup>();
        ArcadeKart.Stats m_FinalStats;

        Quaternion m_LastValidRotation;
        Vector3 m_LastValidPosition;
        Vector3 m_LastCollisionNormal;
        bool m_HasCollision;
		
	    private const int NUM_WHEELS = 4;
		private WheelCollider[] wheels = new WheelCollider[NUM_WHEELS];
		private bool[] wheelsGrounded = new bool[NUM_WHEELS];

        public void AddPowerup(StatPowerup statPowerup) => m_ActivePowerupList.Add(statPowerup);
        public void SetCanMove(bool move) => m_CanMove = move;
        public float GetMaxSpeed() => Mathf.Max(m_FinalStats.TopSpeed, m_FinalStats.ReverseSpeed);

        private void ActivateDriftVFX(bool active)
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                if (active && vfx.wheel.GetGroundHit(out WheelHit hit))
                {
                    if (!vfx.sparks.isPlaying)
                        vfx.sparks.Play();
                }
                else
                {
                    if (vfx.sparks.isPlaying)
                        vfx.sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
                    
            }

            foreach (var trail in m_DriftTrailInstances)
                trail.Item3.emitting = active && trail.wheel.GetGroundHit(out WheelHit hit);
        }

        private void UpdateDriftVFXOrientation()
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                vfx.sparks.transform.position = vfx.wheel.transform.position - (vfx.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up) + (transform.right * vfx.horizontalOffset);
                vfx.sparks.transform.rotation = transform.rotation * Quaternion.Euler(0.0f, 0.0f, vfx.rotation);
            }

            foreach (var trail in m_DriftTrailInstances)
            {
                trail.trailRoot.transform.position = trail.wheel.transform.position - (trail.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up);
                trail.trailRoot.transform.rotation = transform.rotation;
            }
        }

        void UpdateSuspensionParams(WheelCollider wheel)
        {
            wheel.suspensionDistance = SuspensionHeight;
            wheel.center = new Vector3(0.0f, WheelsPositionVerticalOffset, 0.0f);
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = SuspensionSpring;
            spring.damper = SuspensionDamp;
            wheel.suspensionSpring = spring;
        }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            m_Inputs = GetComponents<IInput>();
	      
		    wheels[0] = FrontLeftWheel;
	        wheels[1] = FrontRightWheel;
	        wheels[2] = RearLeftWheel;
	        wheels[3] = RearRightWheel;
			
			for (int i = 0; i < NUM_WHEELS; i++) {
				wheelsGrounded[i] = false;
				UpdateSuspensionParams(wheels[i]);
			}
           
            m_CurrentGrip = baseStats.Grip;

            if (DriftSparkVFX != null)
            {
                AddSparkToWheel(RearLeftWheel, -DriftSparkHorizontalOffset, -DriftSparkRotation);
                AddSparkToWheel(RearRightWheel, DriftSparkHorizontalOffset, DriftSparkRotation);
            }

            if (DriftTrailPrefab != null)
            {
                AddTrailToWheel(RearLeftWheel);
                AddTrailToWheel(RearRightWheel);
            }

            if (NozzleVFX != null)
            {
                foreach (var nozzle in Nozzles)
                {
                    Instantiate(NozzleVFX, nozzle, false);
                }
            }
        }

        void AddTrailToWheel(WheelCollider wheel)
        {
            GameObject trailRoot = Instantiate(DriftTrailPrefab, gameObject.transform, false);
            TrailRenderer trail = trailRoot.GetComponentInChildren<TrailRenderer>();
            trail.emitting = false;
            m_DriftTrailInstances.Add((trailRoot, wheel, trail));
        }

        void AddSparkToWheel(WheelCollider wheel, float horizontalOffset, float rotation)
        {
            GameObject vfx = Instantiate(DriftSparkVFX.gameObject, wheel.transform, false);
            ParticleSystem spark = vfx.GetComponent<ParticleSystem>();
            spark.Stop();
            m_DriftSparkInstances.Add((wheel, horizontalOffset, -rotation, spark));
        }

        void FixedUpdate()
        {
			// pretty sure it's not necessary to set these every update
	        wheels[0] = FrontLeftWheel;
	        wheels[1] = FrontRightWheel;
	        wheels[2] = RearLeftWheel;
	        wheels[3] = RearRightWheel;
			
			for (int i = 0; i < NUM_WHEELS; i++) {
				UpdateSuspensionParams(wheels[i]);
			}
           
            GatherInputs();

            // apply our powerups to create our finalStats
            TickPowerups();

            // apply our physics properties
            Rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);
	
            int groundedCount = 0;
			
			for (int i = 0; i < NUM_WHEELS; i++) {
				wheelsGrounded[i] = false;
				UpdateSuspensionParams(wheels[i]);
           
	            if (wheels[i].isGrounded && wheels[i].GetGroundHit(out WheelHit hit)) {
	                groundedCount++;
					wheelsGrounded[i] = true;
				}
			}
           
            // calculate how grounded and airborne we are
            GroundPercent = (float) groundedCount / 4.0f;
            AirPercent = 1 - GroundPercent;
			// Debug.Log(GroundPercent);
			
            // apply vehicle physics
            if (m_CanMove)
            {
                MoveVehicle(Input.Accelerate, Input.Brake, Input.TurnInput);
            }
            GroundAirbourne();

            m_PreviousGroundPercent = GroundPercent;

            UpdateDriftVFXOrientation();
        }

        void GatherInputs()
        {
            // reset input
            Input = new InputData();
            WantsToDrift = false;

            // gather nonzero input from our sources
            for (int i = 0; i < m_Inputs.Length; i++)
            {
                Input = m_Inputs[i].GenerateInput();
                WantsToDrift = Input.Brake && Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;
            }
        }

        void TickPowerups()
        {
            // remove all elapsed powerups
            m_ActivePowerupList.RemoveAll((p) => { return p.ElapsedTime > p.MaxTime; });

            // zero out powerups before we add them all up
            var powerups = new Stats();

            // add up all our powerups
            for (int i = 0; i < m_ActivePowerupList.Count; i++)
            {
                var p = m_ActivePowerupList[i];

                // add elapsed time
                p.ElapsedTime += Time.fixedDeltaTime;

                // add up the powerups
                powerups += p.modifiers;
            }

            // add powerups to our final stats
            m_FinalStats = baseStats + powerups;

            // clamp values in finalstats
            m_FinalStats.Grip = Mathf.Clamp(m_FinalStats.Grip, 0, 1);
        }

        void GroundAirbourne()
        {
            // while in the air, fall faster
            if (AirPercent >= 1)
            {
                Rigidbody.velocity += Physics.gravity * Time.fixedDeltaTime * m_FinalStats.AddedGravity;
            }
        }

        public void Reset()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = euler.z = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        public float LocalSpeed()
        {
            if (m_CanMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = Rigidbody.velocity.magnitude;
                    return dot < 0 ? -(speed / m_FinalStats.ReverseSpeed) : (speed / m_FinalStats.TopSpeed);
                }
                return 0f;
            }
            else
            {
                // use this value to play kart sound when it is waiting the race start countdown.
                return Input.Accelerate ? 1.0f : 0.0f;
            }
        }

        void OnCollisionEnter(Collision collision) => m_HasCollision = true;
        void OnCollisionExit(Collision collision) => m_HasCollision = false;

        void OnCollisionStay(Collision collision)
        {
            m_HasCollision = true;
            m_LastCollisionNormal = Vector3.zero;
            float dot = -1.0f;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                    m_LastCollisionNormal = contact.normal;
            }
        }

        void MoveVehicle(bool accelerate, bool brake, float turnInput)
        {
	        {
	            float accelInput = (accelerate ? 1.0f : 0.0f) - (brake ? 1.0f : 0.0f);

	            Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

	            bool accelDirectionIsFwd = accelInput >= 0;
	            bool localVelDirectionIsFwd = localVel.z >= 0;

	            // use the max speed for the direction we are going--forward or reverse.
	            float maxSpeed = localVelDirectionIsFwd ? m_FinalStats.TopSpeed : m_FinalStats.ReverseSpeed;
	            float accelPower = accelDirectionIsFwd ? m_FinalStats.Acceleration : m_FinalStats.ReverseAcceleration;

	            float currentSpeed = Rigidbody.velocity.magnitude;
	            float accelRampT = currentSpeed / maxSpeed;

	            // manual acceleration curve coefficient scalar
	            float accelerationCurveCoeff = 5;
	            float multipliedAccelerationCurve = m_FinalStats.AccelerationCurve * accelerationCurveCoeff;
	            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

	            bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

	            // if we are braking (moving reverse to where we are going)
	            // use the braking accleration instead
	            float finalAccelPower = isBraking ? m_FinalStats.Braking : accelPower;

	            float finalAcceleration = finalAccelPower * accelRamp;

	            // apply inputs to forward/backward
	            float turningPower = IsDrifting ? m_DriftTurningPower : turnInput * m_FinalStats.Steer;

	            Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
	            Vector3 fwd = turnAngle * transform.forward;
	            Vector3 movement = fwd * accelInput * finalAcceleration * ((m_HasCollision || GroundPercent > 0.0f) ? 1.0f : 0.0f);

	            // forward movement
	            bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

	            // if over max speed, cannot accelerate faster.
	            if (wasOverMaxSpeed && !isBraking) 
	                movement *= 0.0f;

	            Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
	            newVelocity.y = Rigidbody.velocity.y;

	            //  clamp max speed if we are on ground
	            if (GroundPercent > 0.0f && !wasOverMaxSpeed)
	            {
	                newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
	            }

	            // coasting is when we aren't touching accelerate
	            if (Mathf.Abs(accelInput) < k_NullInput && GroundPercent > 0.0f)
	            {
	                newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.velocity.y, 0), Time.fixedDeltaTime * m_FinalStats.CoastingDrag);
	            }

	            Rigidbody.velocity = newVelocity;

				for (int i = 0; i < 1; i++) {
					WheelCollider w = wheels[i];
					// TODO : all physics need to be done here, and make the current code here 1/4 the strength
					if (wheelsGrounded[i]) {
	                	// manual angular velocity coefficient
	                	float angularVelocitySteering = 0.4f;
	               	 	float angularVelocitySmoothSpeed = 20f;

	                	// turning is reversed if we're going in reverse and pressing reverse
	                	if (!localVelDirectionIsFwd && !accelDirectionIsFwd) 
	                		angularVelocitySteering *= -1.0f;
						
	                	var angularVel = Rigidbody.angularVelocity;

	                	// move the Y angular velocity towards our target
	                	angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);

	                	// apply the angular velocity
	                	Rigidbody.angularVelocity = angularVel;

	                	// rotate rigidbody's velocity as well to generate immediate velocity redirection
	               	 	// manual velocity steering coefficient
	                	float velocitySteering = 25f;

	                	// rotate our velocity based on current steer value
	                	Rigidbody.velocity = Quaternion.AngleAxis(turningPower * Mathf.Sign(localVel.z) * velocitySteering * m_CurrentGrip * Time.fixedDeltaTime, transform.up) * Rigidbody.velocity;
					}
				}
	            
	            bool validPosition = false;
	            if (Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, 3.0f, 1 << 9 | 1 << 10 | 1 << 11)) // Layer: ground (9) / Environment(10) / Track (11)
	            {
	                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > hit.normal.y) ? m_LastCollisionNormal : hit.normal;
	                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime * (GroundPercent > 0.0f ? 10.0f : 1.0f)));    // Blend faster if on ground
	            }
	            else
	            {
	                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > 0.0f) ? m_LastCollisionNormal : Vector3.up;
	                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime));
	            }

	            validPosition = GroundPercent > 0.7f && !m_HasCollision && Vector3.Dot(m_VerticalReference, Vector3.up) > 0.9f;

	            // Airborne / Half on ground management
	            if (GroundPercent < 0.7f)
	            {
	                Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
	                Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
	                finalOrientationDirection.Normalize();
	                if (finalOrientationDirection.sqrMagnitude > 0.0f)
	                {
	                    Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime)));
	                }
	            }
	            else if (validPosition)
	            {
	                m_LastValidPosition = transform.position;
	                m_LastValidRotation.eulerAngles = new Vector3(0.0f, transform.rotation.y, 0.0f);
	            }

	            ActivateDriftVFX(IsDrifting && GroundPercent > 0.0f);
	        }
			/*
			sinAngle = (float) Math.sin(angle);
			cosAngle = (float) Math.cos(angle);
			sinSteerAngle = (float) Math.sin(-steerAngle);
			cosSteerAngle = (float) Math.cos(-steerAngle);

			localVelocity.setX(cosAngle * worldVelocity.getZ() + sinAngle * worldVelocity.getX());
			localVelocity.setZ(-sinAngle * worldVelocity.getZ() + cosAngle * worldVelocity.getX());

			if (localVelocity.getX() >= 0) {
				yawSpeed = type.halfWheelbase * angularVelocity;

				affectedSteeringAngle = (float) Math.atan2(yawSpeed, localVelocity.getX());

				sideSlip = (float) Math.atan2(localVelocity.getZ(), localVelocity.getX());

				slipAngleFront = sideSlip + affectedSteeringAngle - steerAngle;
				slipAngleRear = sideSlip - affectedSteeringAngle;

				weightDiffLongitudinal = (int) (type.heightRatio * weight * accelerationRateLongitudinal);

				weightDiffLateral = (int) (type.heightRatio * weight * accelerationRateLateral);

				weightFront = weight - weightDiffLongitudinal;
				weightRear = weight + weightDiffLongitudinal;

				weightLeft = weight - weightDiffLateral;
				weightRight = weight + weightDiffLateral;

				lateralForceFront = type.corneringStiffnessFront * slipAngleFront;
				lateralForceFront = Math.min(gripFront, lateralForceFront);
				lateralForceFront = Math.max(-gripFront, lateralForceFront);
				lateralForceFront *= weightFront;

				lateralForceRear = type.corneringStiffnessRear * slipAngleRear;
				lateralForceRear = Math.min(gripRear, lateralForceRear);
				lateralForceRear = Math.max(-gripRear, lateralForceRear);
				lateralForceRear *= weightRear;
			} else {
				yawSpeed = type.halfWheelbase * angularVelocity;

				affectedSteeringAngle = (float) Math.atan2(-yawSpeed, -localVelocity.getX());

				sideSlip = (float) Math.atan2(-localVelocity.getZ(), -localVelocity.getX());

				slipAngleFront = sideSlip + affectedSteeringAngle - steerAngle;
				slipAngleRear = sideSlip - affectedSteeringAngle;

				weightDiffLongitudinal = (int) (type.heightRatio * weight * accelerationRateLongitudinal);

				weightDiffLateral = (int) (type.heightRatio * weight * accelerationRateLateral);

				weightFront = weight - weightDiffLongitudinal;
				weightRear = weight + weightDiffLongitudinal;

				weightLeft = weight - weightDiffLateral;
				weightRight = weight + weightDiffLateral;

				lateralForceFront = -type.corneringStiffnessFront * slipAngleFront;
				lateralForceFront = Math.min(gripFront, lateralForceFront);
				lateralForceFront = Math.max(-gripFront, lateralForceFront);
				lateralForceFront *= weightFront;

				lateralForceRear = -type.corneringStiffnessRear * slipAngleRear;
				lateralForceRear = Math.min(gripRear, lateralForceRear);
				lateralForceRear = Math.max(-gripRear, lateralForceRear);
				lateralForceRear *= weightRear;
			}
			float trueBrake = brake;
			if (localVelocity.getX() < 0) {
				trueBrake = -trueBrake;
			}
			rpm += throttle * 100.0f * rpm * type.gr[gear] / type.diffratio * 0.0004f;
			boolean maxed = false;
		
			int random = VSimUtil.getRandom();
			if (rpm > curve.getMaxRPM()) {
				rpm = curve.getMaxRPM() + random;
				maxed = true;
			}

			if (rpm < curve.getMinRPM()) {
				rpm = curve.getMinRPM();
			}

			float max_engine_torque = interp.linearInterpolation(rpm + ((maxed && !clutch) ? random * 5000.0f : 0));

			if (max_engine_torque < 0) {
				max_engine_torque = 0;
			}

			if (throttle > 0 || trueBrake > 0) {
				engine_torque = (throttle - trueBrake) * max_engine_torque;
			} else {
				engine_torque = 0;
			}
			drive_torque = engine_torque * type.gr[gear] * type.diffratio * type.traneff;

			if (gearboxDamage >= 100) {
				drive_torque = 0;
			}

			if (MiscUtil.abs(drive_torque) < 0.1f) {
				drive_torque = 0.0f;
			}

			horsepower = (int) (0.737562149 * max_engine_torque * rpm / 5252);
			float torquePerWheel = drive_torque / 2.0f;
			float totalRollingResistance = 0.0f;
			Wheel w;
			for (int i = 0; i < 4; i++) {
				w = wheels[i];

				if (i == 0 || i == 1) {
					if (localVelocity.getX() < 0.0f) {
						w.angularVelocity = -speedmps;
					} else {
						w.angularVelocity = speedmps;
					}
				}

				totalRollingResistance += getRollingResistance(i);
				if (i == 2 || i == 3) {
					if ((throttle != 0 || trueBrake != 0) && !clutch) {
						if (speedmps != 0 && gear > 0) {
							w.slip_ratio = (w.angularVelocity / speedmps) - 1.0f;
							if (w.slip_ratio > s_curve.getMaxSlip()) {
								w.slip_ratio = s_curve.getMaxSlip();
							}
							if (trueBrake > 0.0f) {
								if (w.slip_ratio < -1.0f) {
									w.slip_ratio = -1.0f;
								}
							} else {
								if (w.slip_ratio < 0.0f) {
									w.slip_ratio = 0.0f;
								}
							}
						} else {
							int surfaceId = w.surfaceId;
							if (surfaceId != 1 && surfaceId != 6) {
								w.slip_ratio += 0.25f;
							}
							if (gear == 0) {
								w.slip_ratio = -0.04f;
							} else {
								w.slip_ratio = 0.05f;
							}
						}
						float mu = s_interp.linearInterpolation(w.slip_ratio);
						float normalForceOnWheel = getWeightOnWheel() + (weightDiffLongitudinal);
						w.tractionForce = mu * normalForceOnWheel;
						w.tractionTorque = -(w.tractionForce * wheelRadius) * 2.0f;
						w.brakeTorque = -(1250.0f * trueBrake);

						if (speedmps == 0) {
							w.brakeTorque = 0.0f;
						}
						w.torque = torquePerWheel + w.tractionTorque + w.brakeTorque;
						// force = torque / radius
						if (MiscUtil.abs(w.torque) < 0.1f) {
							w.torque = 0.0f;
						}
						w.force = w.tractionForce;

						w.angularAcceleration = w.torque / systemInertia;
						w.angularVelocity += DT * w.angularAcceleration;
					} else {
						w.slip_ratio = 0.0f;
						w.force = 0.0f;
						if (localVelocity.getX() < 0.0f) {
							w.angularVelocity = -speedmps;
						} else {
							w.angularVelocity = speedmps;
						}
					}
				}
			}
			if (!clutch) {
				if (brake == 0) {
					forceOnRearWheels = (wheels[2].force + wheels[3].force) * 4;
				} else {
					forceOnRearWheels = (wheels[2].force + wheels[3].force) * 4;
				}
			} else {
				forceOnRearWheels = 0;
			}

			resistance = (float) (totalRollingResistance + getAirResistance() + (weight * Math.sin(pitchCline)));

			if (MiscUtil.abs(localVelocity.getX()) < 0.01f) {
				resistance = 0.0f;
			}
			if (gear == 0) {
				resistance = -resistance;
			}
			netForce.setX(forceOnRearWheels - resistance);
			netForce.setZ(lateralForceRear + lateralForceFront * cosSteerAngle);

			torque = type.b * lateralForceFront - type.c * lateralForceRear;

			localAcceleration.setX(netForce.getX() / type.mass);
			localAcceleration.setZ(netForce.getZ() / type.mass);
			angularAcceleration = torque / type.inertia;

			worldAcceleration.setX(cosAngle * localAcceleration.getZ() + sinAngle * localAcceleration.getX());
			worldAcceleration.setZ(-sinAngle * localAcceleration.getZ() + cosAngle * localAcceleration.getX());

			worldVelocity.setX(worldVelocity.getX() + DT * worldAcceleration.getX());
			worldVelocity.setZ(worldVelocity.getZ() + DT * worldAcceleration.getZ());

			oldPosition.setTo(worldPosition);

			worldPosition.setX(worldPosition.getX() + DT * worldVelocity.getX());
			worldPosition.setZ(worldPosition.getZ() + DT * worldVelocity.getZ());

			angularVelocity += DT * angularAcceleration;

			angle += DT * angularVelocity;

			accelerationRateLongitudinal = DT * netForce.getX() / type.mass;
			accelerationRateLateral = DT * netForce.getZ() / type.mass;
			rpm -= 0.1f * (rpm * (type.gr[gear] / type.diffratio * 0.25f));
			*/
        }
    }
}
