using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class God : MonoBehaviour {

    public float cameraSmooth = 0.3f;
    public GameObject cube;
    public GameObject score_display;
    public GameObject start_display;
    public bool spawn_cubes = true;

    float start_display_alpha = 255f;
    float start_display_position = 0f;

    int score = 1000;

    GameObject[] players;
    Camera cam;
    

    // Use this for initialization
    void Start () {
        string[] joysticks = Input.GetJoystickNames();
        score_display = GameObject.Find("Score");
        start_display = GameObject.Find("Go");
        Debug.Log(string.Format("There are {0} joysticks connected:", joysticks.Length));
        foreach (string i in joysticks)
        {
            Debug.Log(i);
        }

        // spawning boxes
        if (spawn_cubes)
        {
            for (var i = 0; i < 500; i++)
            {
                float z_pos = Random.Range(50f, 1000f);
                float x_rot = Random.Range(0f, 360f);
                float y_pos = Random.Range(10f, 20f);
                float size = Random.value;

                float mass = size * 500;

                GameObject new_cube = Instantiate(cube, new Vector3(0f, y_pos, z_pos), Quaternion.Euler(x_rot, 0f, 0f));
                new_cube.transform.localScale = new Vector3(size, size, size);
                new_cube.GetComponent<Rigidbody>().mass = mass;
            }
        }
      


        players = GameObject.FindGameObjectsWithTag("Player");
        cam = (Camera)FindObjectOfType(typeof(Camera));
    }
	
	// Update is called once per frame
	void Update () {


        if (start_display_position < 1000)
        {
            // for some reason this doesnt work, going with a movement animation instead
            //start_display_alpha -= Mathf.Ceil(Time.deltaTime * 10);
            //Color start_display_color = new Color(255f, 255f, 255f, start_display_alpha);
            //start_display.GetComponent<Text>().color = start_display_color;
            start_display.GetComponent<Text>().transform.position += new Vector3(7.5f, 0, 0);
            start_display_position += 7.5f; 

        }

        Vector3 target = Vector3.zero;
        float target_z = 0f;
        float target_y = 0f;
        Vector3 vel = cam.velocity;
        foreach (GameObject player in players)
        {
            target_z += player.transform.position.z;
            target_y += player.transform.position.y;
        }

        target_z /= players.Length;
        target_y /= players.Length;

        //min and max camera height enforcing
        target_y = Mathf.Clamp(target_y, 5f, 16f);


        if (target_z > 0)
        {
            score = 1000 - Mathf.CeilToInt(target_z);
            score_display.GetComponent<Text>().text = score.ToString();
        }

        target += new Vector3(cam.transform.position.x, target_y, target_z);

        // 6 to 12 on the y axis seems like a good range to allow for given common height of block stacking.  
        // though a generalized yz axis movement could prove useful (perhaps give favor to y being high given the room for play in the foreground)
        cam.transform.position = target;
        // this seems unneeded
        // cam.transform.position = Vector3.SmoothDamp(cam.transform.position, target, ref vel, cameraSmooth, Time.deltaTime);
    }
}
