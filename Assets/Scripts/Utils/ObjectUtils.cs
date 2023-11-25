using System.Collections;
using UnityEngine;

namespace GTASP.Utils
{
    public abstract class ObjectUtils
    {
        public static IEnumerator DestroyAfter(GameObject gameObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            Object.Destroy(gameObject);
        }
    }
}