using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDown : MonoBehaviour {
    Rigidbody rigidBody;
    public float speed = 0.9f;
    Vector3 lookPos;
    Animator animate;
    Transform cam;
    Vector3 camForward;
    Vector3 move;
    Vector3 moveInput;
    float forwardAmount;
    float turnAmount;
    Vector3 movement;

    void Start ()
    {
        rigidBody = GetComponent<Rigidbody>();
        SetupAnimator();
        cam = Camera.main.transform;

	}

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            lookPos = hit.point;
        }

        Vector3 lookDir = lookPos - transform.position;
        lookDir.y = 0;
        transform.LookAt(transform.position + lookDir, Vector3.up);
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Fire1"))
        {
            animate.Play("Attack");
        }

        if (Input.GetButtonDown("Fire2"))
        {
            //animate.Play("Dodge");
            //rigidBody.AddForce(movement * (speed + 2) / Time.deltaTime);
            
        }

        if (cam != null)
        {
            camForward = Vector3.Scale(cam.up, new Vector3(1, 0, 1)).normalized;
            move = vertical * camForward + horizontal * cam.right;
        }
        else
        {
            move = vertical * Vector3.forward + horizontal * Vector3.right;
        }

        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        Move(move);

        movement = new Vector3(horizontal, 0, vertical);
        rigidBody.AddForce(movement * speed / Time.deltaTime);
	}

    void Move(Vector3 move)
    {
        if (move.magnitude > 1)
        {
            move.Normalize();
        }
        this.moveInput = move;

        ConvertMoveInput();
        UpdateAnimator();
    }

    void ConvertMoveInput()
    {
        Vector3 localMove = transform.InverseTransformDirection(moveInput);
        turnAmount = localMove.x;
        forwardAmount = localMove.z;
    }

    void UpdateAnimator()
    {
        animate.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animate.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
    }

    void SetupAnimator()
    {
        animate = GetComponent<Animator>();

        foreach (var childAnimator in GetComponentsInChildren<Animator>())
        {
            if (childAnimator != animate)
            {
                animate.avatar = childAnimator.avatar;
                //Destroy(childAnimator);
                break; //stops searching when first animator is found
            }
        }
    }
}