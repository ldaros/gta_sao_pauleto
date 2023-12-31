﻿using UnityEngine;

namespace GTASP.WaterShader.Example_Scenes.Environment.Scripts
{
    public class Lighthouse : MonoBehaviour
    {
        Transform tr;

        void Awake()
        {
            tr = transform;
        }

        void LateUpdate()
        {
            tr.Rotate(Vector3.up, Time.deltaTime * 40);
        }
    }
}
