﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

    public int joy_num;

    [Range(0f, 50f)]
    public float speed = 10;

    [Range(0f, 3f)]
    public float dash_time = 1;

    [Range(0f, 50f)]
    public float jump_velocity = 10;

    [Range(0f, 10f)]
    public float against_range = 1.5f;

    [Range(0f, 2f)]
    public float jump_range = 0.5f;

    [Range(0f, 5f)]
    public float fall_multiplier = 1f;

    public bool DEBUG = true;

    Rigidbody rb;
    Animator animator;
    BoxCollider boxCollider;
    TrailRenderer trailRenderer;

    // state
    bool jumping;
    bool hanging;

    // controls
    string hAxis;
    string vAxis;
    string jumpBtn;
    string dashBtn;


    //box collider info
    float center_y; // center of
    float size_y; // height of box collider in y axis
    float size_z; // z axis width
    Vector3 top_start;
    Vector3 hang_start;
    Vector3 bottom_start;
    Vector3 ahead_start;
    Vector3 behind_start;
    float bottom;
    float half_width;


    // Use this for initialization
    void Start () {
        Debug.Log(string.Format("Player {0} joined", joy_num));
        //Game Objects
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();
        boxCollider = GetComponent<BoxCollider>();

        // initial settings double check
        trailRenderer.time = 0;
        trailRenderer.enabled = false;

        //controls
        hAxis = string.Format("Horizontal_P{0}", joy_num);
        vAxis = string.Format("Vertical_P{0}", joy_num);
        jumpBtn = string.Format("Jump_P{0}", joy_num);
        dashBtn = string.Format("Dash_P{0}", joy_num);

        //constants
        size_z = boxCollider.size.z;
        size_y = boxCollider.size.y;
        center_y = boxCollider.center.y;
        bottom = center_y - (size_y / 2f);
        half_width = size_z / 2f;

        top_start = new Vector3(0, center_y + size_y / 2, 0);
        hang_start = top_start + new Vector3(0, -.85f, 0);
        bottom_start = new Vector3(0, center_y - size_y / 4, 0);

        ahead_start = new Vector3(0f, bottom + 0.1f, half_width - 0.1f);
        behind_start = new Vector3(0f, bottom + 0.1f, -half_width + 0.1f);

    }

    // return true if the ray cast hits, show a debug line if DEBUG is true
    bool isAgainst(Vector3 position, Vector3 direction, float range, Color color) {
      if (DEBUG)
      {
        Debug.DrawRay(transform.position + position, direction * range, color);
      }

      return Physics.Raycast(transform.position + position, direction, range);
    }

    void Update()
    {
        float hMov = Input.GetAxis(hAxis);
        float vMov = Input.GetAxis(vAxis);
        bool jump_btn = Input.GetButtonDown(jumpBtn);
        bool dash_btn = Input.GetButtonDown(dashBtn);

        bool grounded = (
          isAgainst(ahead_start, Vector3.down, jump_range, Color.magenta) ||
          isAgainst(behind_start, Vector3.down, jump_range, Color.magenta)
        );
        bool against_front_top = isAgainst(top_start, Vector3.forward, against_range, Color.green);
        bool against_front_hang = isAgainst(hang_start, Vector3.forward, against_range, Color.grey);
        bool against_front_bottom = isAgainst(bottom_start, Vector3.forward, against_range, Color.green);

        Vector3 deltaRun = new Vector3(0, 0, rb.velocity.z);
        Vector3 deltaJump = new Vector3(0, rb.velocity.y, 0);
        Vector3 deltaDash = Vector3.zero;


        // DASHING
        if (dash_time <= 0f)
        {
            dash_time = 1f;
            trailRenderer.enabled = false;
        }

        if (dash_btn && dash_time == 1f)
        {
            dash_time = dash_time - Time.deltaTime;
        }

        if (dash_time > 0f && dash_time < 1f)
        {

            trailRenderer.enabled = true;
            trailRenderer.time = dash_time * 0.6f;

            //Move left
            if (hMov < 0f)
            {
                deltaDash = new Vector3(0f, 0f, -10);

            }
            //Move right
            if (hMov > 0f)
            {
                deltaDash = new Vector3(0f, 0f, 10);
            }
            if (!dash_btn)
            {
                dash_time = dash_time - Time.deltaTime;
            }
        }





        // RUNNING
        if (hMov != 0 )
        {
            deltaRun = new Vector3(0f, 0f, hMov * speed);

            if (hMov < 0)
            {
                Quaternion target_back = Quaternion.Euler(0, 180f, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, target_back, Time.deltaTime * speed);
            }
            else if (hMov > 0)
            {
                Quaternion target_forward = Quaternion.Euler(0, 0, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, target_forward, Time.deltaTime * speed);

            }
        }



        // JUMPING
        // has landed again after jump
        if (jumping && grounded && rb.velocity.y < .05 )
        {
            jumping = false;
            animator.SetBool("JUMP", false);
        }

        // engage jump
        if (jump_btn && (grounded || hanging))
        {
            deltaJump = Vector3.up * jump_velocity;
            jumping = true;
            animator.SetBool("JUMP", true);
        }


        Vector3 delta = deltaRun + deltaJump + deltaDash;

        // LEDGE HANG
        if (!grounded && !against_front_top && against_front_hang && !jump_btn)
        {
            delta = Vector3.zero;
            rb.useGravity = false;
            animator.SetBool("HANG", true);
            hanging = true;
        }
        else if (hanging)
        {
            rb.useGravity = true;
            animator.SetBool("HANG", false);
            hanging = false;
        }


        rb.velocity = delta;


        // update animator parameters
        animator.SetFloat("ZVEL", rb.velocity.z / speed, .1f, Time.deltaTime);
        animator.SetFloat("YVEL", rb.velocity.y);
    }

    void FixedUpdate()
    {
        // on fall
        if (rb.velocity.y < 0)
        {
            // increase fall speed by gravitational pull * fall_multiplier
            rb.velocity += Vector3.up * Physics.gravity.y * fall_multiplier * Time.deltaTime;
        }

    }
}
