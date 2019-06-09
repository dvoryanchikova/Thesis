using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    public float speed = 13f;
    public float radiusToMove = 10f;
    Vector3 direction = Vector3.forward;
    RaycastHit hit;
    float rayDist = 3f;
    public LayerMask layerMask;
    Rigidbody rb;
    Vector3 startPos;
    float yPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        yPos = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        Debug.DrawLine(transform.position, transform.position + transform.forward*rayDist, Color.magenta);
        bool isOutOfRadius = Vector3.Distance(transform.position, startPos) > radiusToMove;
        if (Physics.Raycast(ray,rayDist, layerMask) || isOutOfRadius)
        {            
            if (isOutOfRadius)
            {
                transform.position = startPos + new Vector3(Random.Range(0, radiusToMove - 1), yPos, Random.Range(0, radiusToMove - 1));
                    //(startPos - transform.position).normalized * (radiusToMove - 1);
            }
            float angle = Random.Range(15, 180);
            transform.Rotate(new Vector3(0, angle, 0));
        }

        Vector3 newPos = transform.position + transform.forward * speed * Time.deltaTime;
        rb.MovePosition(newPos);
    }

    public void setNewStartPos(Vector3 newPoos)
    {
        startPos = newPoos;
    }
}
