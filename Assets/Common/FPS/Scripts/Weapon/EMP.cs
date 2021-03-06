﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OperationTrident.FPS.Weapons {

    public class EMP : MonoBehaviour {
        public float scale = 1.0f;
        public float speed = 0.1f;
        public float alpha = 1.0f;
        public float r = 20.0f;
        public float t = 10.0f;

        float DistortStrength;
        float IntersectPower;

        Collider[] cols;

        // Use this for initialization
        void Start() {
            this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            DistortStrength = 1;
            IntersectPower = 0;

            cols = Physics.OverlapSphere(transform.position, r);

            foreach (Collider col in cols)
            {
                //Debug.Log(col);
                col.SendMessageUpwards("OnEMP", t, SendMessageOptions.DontRequireReceiver);
            }
        }

        // Update is called once per frame
        void Update() {
            if (this.transform.localScale.x <= scale) {
                DistortStrength -= 1 * speed / scale;
                IntersectPower += 3 * speed / scale;
                alpha -= 1.2f * speed / scale;
                if (alpha < 0.0f) alpha = 0.0f;
                this.transform.localScale += new Vector3(speed, speed, speed);
                this.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_DistortStrength", DistortStrength);
                this.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_IntersectPower", IntersectPower);
                this.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_ColorMultiplier", new Vector4(alpha, alpha, alpha, alpha));
            } else {
                Destroy(this.gameObject);
            }
        }
    }

}