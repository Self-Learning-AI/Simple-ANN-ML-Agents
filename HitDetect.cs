//Detect when the orange block has touched the goal. 
//Detect when the orange block has touched an obstacle. 
//Put this script onto the orange block. There's nothing you need to set in the editor.
//Make sure the goal is tagged with "goal" in the editor.

using UnityEngine;

public class HitDetect : MonoBehaviour
{
    [HideInInspector]

	public PushAgentBasic agent;  //

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("agent"))
        {
            agent.SuccessfulHit();
        }
    }
}
