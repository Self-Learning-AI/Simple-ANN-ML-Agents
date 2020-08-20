//Put this script on your blue cube.

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using MLAgents;

public class KnightAgent : Agent
{
	public GameObject ground;
    public GameObject area;
	[HideInInspector]
    public Bounds areaBounds;
    PushBlockAcademy academy;
    public GameObject goal;
    public GameObject block;
	[HideInInspector]
    public HitDetect hitDetect;

    public bool useVectorObs;

    Rigidbody blockRB; 
    Rigidbody agentRB;
    Material groundMaterial;
    RayPerception rayPer;

    Animator anim;
    float hp;
    public float damage = 10;
    public int moveAction;
    public int atkAction;
    public KnightAgent enemy;

    Renderer groundRenderer; // to change floor to green

    void Awake()
    {
        academy = FindObjectOfType<PushBlockAcademy>();
        anim = GetComponent<Animator>();
        //isCooldown = false;
        //SetupAnimator();
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        hitDetect = block.GetComponent<HitDetect>();
        hitDetect.agent = this;
        rayPer = GetComponent<RayPerception>();
        agentRB = GetComponent<Rigidbody>();
        blockRB = block.GetComponent<Rigidbody>();
        areaBounds = ground.GetComponent<Collider>().bounds;
        groundRenderer = ground.GetComponent<Renderer>();
        // Starting material
        groundMaterial = groundRenderer.material;
        hp = 100;
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {
            var rayDistance = 12f; // Distance of detect objects
            float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
            var detectableObjects = new[] { "block", "goal", "wall" };
            AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
            AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 1.5f, 0f));
            AddVectorObs(hp);
        }
    }

    /// Use the ground's bounds to pick a random spawn position.
    public Vector3 GetRandomSpawnPos()
    {
        bool foundNewSpawnLocation = false;
        Vector3 randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            float randomPosX = Random.Range(-areaBounds.extents.x * academy.spawnAreaMarginMultiplier,
                                areaBounds.extents.x * academy.spawnAreaMarginMultiplier);

            float randomPosZ = Random.Range(-areaBounds.extents.z * academy.spawnAreaMarginMultiplier,
                                            areaBounds.extents.z * academy.spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.01f, 2.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    public void SuccessfulHit()
    {
        enemy.TakeDamage(damage);
        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(academy.goalScoredMaterial, 0.5f));
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        groundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        groundRenderer.material = groundMaterial;
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
	public void MoveAgent(float[] act)
    {

        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        moveAction = Mathf.FloorToInt(act[0]);
        atkAction = Mathf.FloorToInt(act[1]);

        switch (moveAction)
        {
            case 0:
                anim.SetTrigger("Stop");
                break;
            case 1:
                dirToGo = transform.forward * 1f;
                anim.SetTrigger("WalkForward");
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                anim.SetTrigger("WalkBackward");
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                anim.SetTrigger("WalkForward");
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                anim.SetTrigger("WalkForward");
                break;
            //case 7:
                //isCooldown = true;
                //anim.Play("Attack");
                //cooldown = Time.time + 2f; //2sec cooldown
                //break;
        }
        if (atkAction == 1)
        {
            anim.SetTrigger("Attack");
            //SetActionMask(1, new int[1] {1}); // --add cooldown for agent action
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        agentRB.AddForce(dirToGo * academy.agentRunSpeed,
                         ForceMode.VelocityChange);

    }

	public override void AgentAction(float[] vectorAction, string textAction)
    {
        MoveAgent(vectorAction);
        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / agentParameters.maxStep);
    }


    void ResetBlock()
    {
        block.transform.position = GetRandomSpawnPos();
        blockRB.velocity = Vector3.zero;
        blockRB.angularVelocity = Vector3.zero;
    }

	public override void AgentReset()
    {
        int rotation = Random.Range(0, 4);
        float rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));
        hp = 100;

        ResetBlock();
        transform.position = GetRandomSpawnPos();
        agentRB.velocity = Vector3.zero;
        agentRB.angularVelocity = Vector3.zero;
    }

    public bool IsAttacking()
    {
        if (atkAction == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TakeDamage(float damageTaken)
    {
        if (hp - damageTaken <= 0)
        {
            AddReward(-5.0f);
            enemy.AddReward(5f);
            Done();
        }
        else
        {
            hp -= damageTaken;
            enemy.AddReward(1f);
            AddReward(-0.3f);
        }
    }

    public Text atkText;
    public Text moveText;
    public Text hpText;
	//Update UI
    public void UpdateText()
    {
        atkText.text = atkAction.ToString();
        moveText.text = moveAction.ToString();
        hpText.text = hp.ToString();
    }

    //void UpdateAnimator(float x, float z)
    //{
    //   // anim.SetFloat("Forward", z);
    //   // anim.SetFloat("Turn", x);
    //}

    //void SetupAnimator()
    //{
    //    anim = GetComponent<Animator>();

    //    foreach (var childAnimator in GetComponentsInChildren<Animator>())
    //    {
    //        if (childAnimator != anim)
    //        {
    //            anim.avatar = childAnimator.avatar;
    //            Destroy(childAnimator);
    //            break; //stops searching when first animator is found
    //        }
    //    }
    //}
}
