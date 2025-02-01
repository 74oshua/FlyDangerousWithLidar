using System.Security.Cryptography;
using Core.Player;
using UnityEngine;

namespace Core.ShipModel.Modifiers.Boost {
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ModifierBoostThrust : MonoBehaviour, IModifier {
        [SerializeField] private float shipForceAdd = 50000000;
        [SerializeField] private float shipSpeedAdd = 150;
        [SerializeField] private float shipThrustAdd = 50000;

        private AudioSource _boostSound;
        private MeshRenderer _meshRenderer;
        private bool _useDistortion;
        private static readonly int includeDistortion = Shader.PropertyToID("_includeDistortion");

        public bool UseDistortion {
            get => _useDistortion;
            set {
                _useDistortion = value;
                MeshRenderer.sharedMaterial.SetInt(includeDistortion, _useDistortion ? 1 : 0);
            }
        }

        private MeshRenderer MeshRenderer {
            get {
                if (_meshRenderer == null)
                    _meshRenderer = GetComponent<MeshRenderer>();
                return _meshRenderer;
            }
        }

        public void Awake() {
            _boostSound = GetComponent<AudioSource>();
        }

        public void ApplyModifierEffect(Rigidbody shipRigidBody, ref AppliedEffects effects) {
            if (!_boostSound.isPlaying) _boostSound.Play();

            ShipPhysics physics = shipRigidBody.GetComponent<ShipPlayer>().ShipPhysics;

            if (shipRigidBody.gameObject.GetComponent<ShipPlayer>().ShipPhysics.FlightParameters.use_old_boost)
            {
                Debug.Log("using old boost");

                // old implementation
                effects.shipForce += transform.forward * shipForceAdd;
                effects.shipDeltaSpeedCap += shipSpeedAdd;
            }
            else
            {
                Debug.Log("using new boost");

                // new code
                float max_speed = 800 + physics.CurrentBoostedMaxSpeedDelta + effects.shipDeltaSpeedCap;
                effects.shipForce += (transform.forward * max_speed - shipRigidBody.velocity) * shipRigidBody.mass / Mathf.Sqrt(Time.fixedDeltaTime) * 0.5f;
            }
            
            // apply additional thrust if the ship is facing the correct direction
            if (shipRigidBody.gameObject.GetComponent<ShipPlayer>().ShipPhysics.FlightParameters.use_old_boost
                && Vector3.Dot(transform.forward, shipRigidBody.transform.forward) > 0)
            {
                effects.shipDeltaThrust += shipThrustAdd;
            }
        }

        public void ApplyInitialEffect(Rigidbody shipRigidBody, ref AppliedEffects effects)
        {
            Debug.Log("Singularity!");
            if (shipRigidBody.gameObject.GetComponent<ShipPlayer>().ShipPhysics.FlightParameters.use_old_boost)
            {
                return;
            }
            
            // estimates the number of frames the shipDeltaCap would normally be increased in the original implementation
            float num_frames = Mathf.Max(-0.2f * (shipRigidBody.velocity.magnitude / 100f) + 15.2f, 0);
            Debug.Log(num_frames);

            effects.shipDeltaSpeedCap += shipSpeedAdd * num_frames;
            if (Vector3.Dot(transform.forward, shipRigidBody.transform.forward) > 0)
            {
                effects.shipDeltaThrust += shipThrustAdd * num_frames;
            }
        }
    }
}