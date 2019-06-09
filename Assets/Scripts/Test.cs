using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    enum StateMachine
    {
        Allive = 100,
        HalfDead = 50,
        Dead = 0
    }
    StateMachine currentState = StateMachine.Allive;
     

    // Start is called before the first frame update
    void Start()
    {
        currentState = StateMachine.HalfDead;
        Debug.Log(currentState);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
