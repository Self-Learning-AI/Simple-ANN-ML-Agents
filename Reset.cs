using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset : MonoBehaviour {
    public KnightAgent agent;

    // Update is called once per frame
    void OnCollisionEnter(Collision col)
    {
        // Touched goal.
        if (col.gameObject.CompareTag("goal") && agent.IsAttacking())
        {
            agent.SuccessfulHit();
        }
    }
}
