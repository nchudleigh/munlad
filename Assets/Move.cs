using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

    public int joy_num;

    [Range(0f, 50f)]
    public float speed = 10;

    [Range(0f, 3f)]
    public float dash_time = 1;
    [Range(0f, 50f)]
    public float dash_vel = 20;

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
    bool jump_request;
    bool hanging;
    bool dashing;
    // keeps remaining time of dash
    float dash_ctr;

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
        // starting positions for Raycasts
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

    // Runs every Frame, input should be captured here
    void Update()
    {
        bool jump_btn = Input.GetButtonDown(jumpBtn);
        bool dash_btn = Input.GetButtonDown(dashBtn);

        bool grounded = (
          isAgainst(ahead_start, Vector3.down, jump_range, Color.magenta) ||
          isAgainst(behind_start, Vector3.down, jump_range, Color.magenta)
        );
        bool against_front_top = isAgainst(top_start, Vector3.forward, against_range, Color.green);
        bool against_front_hang = isAgainst(hang_start, Vector3.forward, against_range, Color.grey);
        bool against_front_bottom = isAgainst(bottom_start, Vector3.forward, against_range, Color.green);


        // DASH

        // Turn Dash On
        if (dash_btn && !dashing && dash_ctr <= 0) {
          dash_ctr = dash_time;
          dashing = true;
          trailRenderer.enabled = true;
        }

        if (dashing) {
          // Decrement Dash Counter
          dash_ctr -= Time.deltaTime;
          // If out of time, turn Dash off
          if (dash_ctr <= 0) {
            dashing = false;
            trailRenderer.enabled = false;
          }
        }

        // JUMP
        if (jump_btn && (grounded || hanging)) {
          jump_request = true;
          animator.SetBool("JUMP", true);
        }

        if (grounded && jumping) {
          jumping = false;
          animator.SetBool("JUMP", false);
        }

        // LEDGE HANG
        if (!grounded && !against_front_top && against_front_hang && !jump_btn)
        {
            animator.SetBool("HANG", true);
            hanging = true;
        }
        else if (hanging)
        {
            animator.SetBool("HANG", false);
            hanging = false;
        }

        // update animator parameters
        animator.SetFloat("ZVEL", rb.velocity.z / speed, .1f, Time.deltaTime);
        animator.SetFloat("YVEL", rb.velocity.y);
    }


    // Runs at the same clip no matter the frame rate / machine being run on
    // Physics updates should be made here
    void FixedUpdate() {
        float hMov = Input.GetAxis(hAxis);
        float vMov = Input.GetAxis(vAxis);

        // RUN
        if (hMov != 0) {
          // which direction to apply running in
          int sign = hMov > 0 ? 1 : -1;
          // calculate required run speed
          float delta_run_vel = speed * sign - rb.velocity.z;
          // apply run speed
          rb.velocity += Vector3.forward * delta_run_vel;
          // rotate player to correct direction
          Quaternion rotation_target = Quaternion.Euler(0, sign==1?0:180, 0);
          transform.rotation = Quaternion.Slerp(transform.rotation, rotation_target, Time.deltaTime * speed);
        }
        // If there is no input make sure he comes to a complete stop
        else if (Mathf.Abs(rb.velocity.z) < 0.05f) {
          rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }

        // DASH
        if (dashing) {
          int sign = hMov > 0 ? 1 : -1;
          // calculate required dash velocity
          float delta_dash_vel = dash_vel * sign - rb.velocity.z;
          // apply dash speed
          rb.velocity += Vector3.forward * delta_dash_vel;
          // rb.velocity += Vector3.Normalize(rb.velocity) * delta_dash_vel;
          // rb.velocity += Vector3.Normalize(new Vector3(0f, vMov, hMov)) * delta_dash_vel;
        }

        // JUMP
        if (jump_request) {
          jumping = true;
          rb.velocity += Vector3.up * jump_velocity;
          jump_request = false;
        }


        if (hanging) {
          rb.useGravity = false;
          rb.velocity = Vector3.zero;
        } else {
          rb.useGravity = true;
        }

        // FALL
        if (rb.velocity.y < 0) {
            // increase fall speed by gravitational pull * fall_multiplier
            rb.velocity += Vector3.up * Physics.gravity.y * fall_multiplier * Time.deltaTime;
        }

    }
}
