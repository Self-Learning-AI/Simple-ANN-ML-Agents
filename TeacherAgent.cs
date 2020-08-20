using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class TeacherAgent : Agent
{
    Rigidbody rBody;
    public float hp;
    float rotation;
    float atkAction;
    float cooldown;
    bool isCooldown;
    float defAction;
    float evAction;
    Animator anim;


    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        hp = 100;
        isCooldown = false;
        SetupAnimator();
    }

    public Transform Target;
    public override void AgentReset()
    {
        if (this.transform.position.y < 0)
        {
            // If the Agent fell, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.position = new Vector3(0, 0.5f, 0);
        }

        hp = 100;
        // Move the target to a new spot
        Target.position = new Vector3(Random.value * 8 - 4,
                                      0.5f,
                                      Random.value * 8 - 4);
    }

    public override void CollectObservations()
    {
        // Target and Agent positions
        AddVectorObs(Target.position);
        AddVectorObs(this.transform.position);

        // Agent velocity & rotation
        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);
        AddVectorObs(this.transform.rotation);

        // Agent hitpoints & cooldown
        AddVectorObs(hp);
        AddVectorObs(isCooldown);

    }

    public float speed = 3;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Actions, size = 4
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0] * 100;
        controlSignal.z = vectorAction[1] * 100;
        rotation = vectorAction[2] * 100 * Time.deltaTime;
        atkAction = vectorAction[3];
        
        //do actions
        rBody.AddForce(controlSignal * speed);
        this.transform.Rotate(0, rotation, 0);


        if (cooldown <= Time.time) // check if action cooldown is finished
        {
            isCooldown = false;

            //animations
            UpdateAnimator(controlSignal.x, controlSignal.z);
            if (atkAction > 0.75f)
            {
                isCooldown = true;
                anim.Play("Attack");
                cooldown = Time.time + 2f; //2sec cooldown
            }
        }

        float distanceToTarget = Vector3.Distance(this.transform.position,
                                                  Target.position);

        if (distanceToTarget < 1.42f)
        {
            AddReward(0.1f);
        }

        // Fell off platform
        if (this.transform.position.y < -1)
        {
            //AddReward(-0.3f);
            Done();
        }

    }

    public void SuccessfulHit()
    {
        SetReward(5.0f);
        Done();
    }

    public bool IsAttacking()
    {
        if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
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
        if (hp - damageTaken < 0)
        {
            SetReward(-1.0f);
            Done();
        }
        else
        {
            hp -= damageTaken;
            AddReward(-0.1f);
        }
    }

    void UpdateAnimator(float x, float z)
    {
        anim.SetFloat("Forward", z);
        anim.SetFloat("Turn", x);
    }

    void SetupAnimator()
    {
        anim = GetComponent<Animator>();

        foreach (var childAnimator in GetComponentsInChildren<Animator>())
        {
            if (childAnimator != anim)
            {
                anim.avatar = childAnimator.avatar;
                Destroy(childAnimator);
                break; //stops searching when first animator is found
            }
        }
    }
}