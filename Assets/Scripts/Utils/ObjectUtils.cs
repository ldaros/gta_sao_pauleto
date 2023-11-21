using System;
using System.Collections;
using UnityEngine;

namespace Utils
{
    public abstract class ObjectUtils
    {
        public static IEnumerator DestroyAfter(GameObject gameObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}