using System.Collections.Generic;
using UnityEngine;

public class ReturnPlayerToSafeZone : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Settings")]
    public float maxY = -100f;

    private List<Transform> _safeZones;

    private void Start()
    {
        InitializeSafeZones();
    }

    private void Update()
    {
        if (PlayerIsOutOfBounds())
        {
            Transform safeZone = GetClosestSafeZone();
            ReturnPlayerInBounds(safeZone);
        }
    }

    private void InitializeSafeZones()
    {
        _safeZones = new List<Transform>();
        GameObject[] safeZoneObjects = GameObject.FindGameObjectsWithTag("SafeZone");
        foreach (GameObject safeZone in safeZoneObjects)
        {
            _safeZones.Add(safeZone.transform);
        }
    }

    private bool PlayerIsOutOfBounds()
    {
        return player.position.y < maxY;
    }

    private Transform GetClosestSafeZone()
    {
        Transform closestSafeZone = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform safeZone in _safeZones)
        {
            float distance = Vector3.Distance(player.position, safeZone.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSafeZone = safeZone;
            }
        }

        return closestSafeZone;
    }

    private void ReturnPlayerInBounds(Transform safeZone)
    {
        player.position = safeZone.position;

        Rigidbody rigidBody = player.GetComponent<Rigidbody>();
        rigidBody.velocity = Vector3.zero;

        Debug.Log("Player fell out of the world. Returning player to safe zone.");
    }
}
