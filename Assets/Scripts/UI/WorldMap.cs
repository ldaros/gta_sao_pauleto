using UnityEngine;
using UnityEngine.Serialization;

namespace GTASP.UI
{
    public class WorldMap : MonoBehaviour
    {
        [SerializeField] private GameObject map;
        [FormerlySerializedAs("camera")] [SerializeField]
        private GameObject mapCamera;

        public void ToggleMap()
        {
            map.SetActive(!map.activeSelf);
            mapCamera.SetActive(!mapCamera.activeSelf);
        }
    }
}