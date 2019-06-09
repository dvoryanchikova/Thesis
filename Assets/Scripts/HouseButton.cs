using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseButton : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {        
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GetComponentInParent<HouseToHide>().ActivateHouse();
        }
    }
}
