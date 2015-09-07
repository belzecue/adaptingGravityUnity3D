﻿using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace AdaptingGravity.Physics.Gravity
{
    [RequireComponent(typeof(Rigidbody))]
    public class GravityHandler : MonoBehaviour
    {
        public delegate void GravityEventDelegate(Vector3 gravityDirection);
        public event GravityEventDelegate GravityChanged;
        public bool OnGround { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        public string[] attractingObjectTags = new string[] {"Ground"};
        public float gravityStrength = 15f;
        public float gravityCheckDistance = 5f;
        public float groundCheckDistance = 0.1f;
        public bool UseWeightedAverage = false;
        public GameObject GravityHandles;
        public GameObject GravityHandlePrefab;
        public List<GravityHandle> handles;

        private Vector3 gravityDirection = Vector3.down;
        public float groundDistance { get; private set; }

        private new Rigidbody rigidbody;
        
    
    
        void Awake ()
        {
            groundDistance = 0f;
            rigidbody = GetComponent<Rigidbody>();
            GroundNormal = Vector3.up;
        }
	
        // Update is called once per frame
        void FixedUpdate () {
            if (UseWeightedAverage)
            {
                CalculateGravityDirectionWeightedAverage();
            }
            else
            {
                CalculateGravityDirection();
            }
            
            if (Mathf.Abs(gravityStrength) > 0.02f)
            {
                rigidbody.AddForce(gravityDirection * gravityStrength, ForceMode.Force);
            }
        }

        void CalculateGravityDirection()
        {
            GravityHandle nearestHandle = null;
            float nearestGroundDistance = float.MaxValue;
            foreach (GravityHandle gravityHandle in handles)
            {
                if (gravityHandle.CalculateGravityDirection())
                {
                    if (gravityHandle.GroundDistance < nearestGroundDistance)
                    {
                        nearestGroundDistance = gravityHandle.GroundDistance;
                        nearestHandle = gravityHandle;
                    }
                }
            }
            if (nearestHandle != null)
            {
                nearestHandle.IsActiveGravityDirection = true;
                ApplyNewGroundNormal(nearestHandle.GroundNormal);
            }
        }

        void CalculateGravityDirectionWeightedAverage()
        {
            float sumGroundDistance = 0f;
            Vector3 sumGroundNormals = Vector3.zero;
            foreach (GravityHandle gravityHandle in handles)
            {
                if (gravityHandle.CalculateGravityDirection())
                {
                    //This creates some strange oscillations
                    //float weight = groundCheckDistance - gravityHandle.GroundDistance;
                    float weight = 1f;
                    sumGroundDistance += weight;
                    sumGroundNormals += gravityHandle.GroundNormal*weight;
                    gravityHandle.IsActiveGravityDirection = true;
                }
            }
            Vector3 newGroundNormal = sumGroundNormals/sumGroundDistance;
            Debug.DrawLine(transform.position, transform.position + newGroundNormal * 3f, Color.red);
            ApplyNewGroundNormal(newGroundNormal);
        }

        private void ApplyNewGroundNormal(Vector3 newGroundNormal)
        {
            if (GroundNormal != newGroundNormal)
            {
                GroundNormal = Vector3.Lerp(GroundNormal, newGroundNormal, 2f * Time.fixedDeltaTime);
                gravityDirection = -GroundNormal;
                if (GravityChanged != null)
                {
                    GravityChanged(gravityDirection);
                }
            }
        }

        public void ChangeGravityStrength(float newGravity)
        {
            gravityStrength = newGravity;
        }

        private void Reset()
        {
            if (GravityHandles == null)
            {
                if (transform.FindChild("GravityHandles") != null)
                {
                    GravityHandles = transform.FindChild("GravityHandles").gameObject;
                }
                else
                {
                    GravityHandles = new GameObject("GravityHandles");
                    GravityHandles.transform.SetParent(transform, false);
                }
            }
        }


    }
}
