using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateOnComplete : MonoBehaviour
{
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
