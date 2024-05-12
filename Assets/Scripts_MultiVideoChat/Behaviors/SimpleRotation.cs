using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{
    [SerializeField] private int rotationAngle = 1;

    private void Update()
    {
        transform.Rotate(rotationAngle, rotationAngle, rotationAngle, Space.Self);
    }
}
