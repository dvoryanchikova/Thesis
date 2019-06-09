using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseToHide : MonoBehaviour
{
    public GameObject[] walls;
    public GameObject button;
    public float timeBeforeActivate = 0.1f;
    public float timeToHide = 10f;
    public float cooldownTime = 10f;

    public void ActivateHouse()
    {
        StartCoroutine(ActivateHouseCoroutine());
    }

    private IEnumerator ActivateHouseCoroutine()
    {
        button.SetActive(false);
        yield return new WaitForSeconds(timeBeforeActivate);
        foreach (GameObject wall in walls)
        {
            wall.SetActive(true);
        }
        yield return new WaitForSeconds(timeToHide);
        foreach (GameObject wall in walls)
        {
            wall.SetActive(false);
        }
        Invoke("SetButtonActive", cooldownTime);
    }

    private void SetButtonActive()
    {
        button.SetActive(true);
    }

}
