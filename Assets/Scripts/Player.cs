using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Transform cam;
    private World world;

    //movement
    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float velocityMomentum = 0;    

    // Place/Delete cubes 
    public Transform markedCube;
    private float lastPosX = 0;
    private float lastPosY = 0;
    private float lastPosZ = 0;
    public Transform placeCube;
     private int clickCounter = 0;

    public Text placeCubeColor;
    public byte defaultCubeIndex = 1; 

    //moving flags 
    private bool standing;
    private bool sprinting;
    private bool jumping;

    private void Start()
    {
        // I dont want to see mouse 
        Cursor.lockState = CursorLockMode.Locked;

        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
        
        placeCubeColor.text = "place " + Constants.cubeTypes[defaultCubeIndex].color + " cube";
    }

    private void Update()
    {
        GetPlayerInputs();
        HighlightCube();
        GetVelocity();

        if(jumping)
        {
            velocityMomentum = Constants.jumpPower;
            standing = false;
            jumping = false;
        }

        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * (-mouseVertical));
        transform.Translate(velocity, Space.World);
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if(Input.GetButtonDown("Sprint"))
        {
            sprinting = true;
        }

        if(Input.GetButtonUp("Sprint"))
        {
            sprinting = false;
        }

        if(standing && Input.GetButtonDown("Jump"))
        {
            jumping = true;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        //Change cube color to place
        if(scroll != 0)
        {
            if(scroll > 0)
            {
                defaultCubeIndex++;
            }
            else
            {
                defaultCubeIndex--;
            }

            if(defaultCubeIndex > (byte)(Constants.cubeTypes.Length - 1))
            {
                defaultCubeIndex = 1;
            }

            if(defaultCubeIndex < 1)
            {
                defaultCubeIndex = (byte)(Constants.cubeTypes.Length - 1);
            }

            placeCubeColor.text = "place " + Constants.cubeTypes[defaultCubeIndex].color + " cube";
        }

        if(markedCube.gameObject.activeSelf)
        {
            if(lastPosX != markedCube.position.x || lastPosY != markedCube.position.y || lastPosZ != markedCube.position.z)
            {
                clickCounter = 0;
            }

            if(Input.GetMouseButtonDown(1))
            {
                clickCounter++;
                lastPosX = markedCube.position.x;
                lastPosY = markedCube.position.y;
                lastPosZ = markedCube.position.z;
            }

            //destroy cubes
            if(Input.GetMouseButtonDown(1) && clickCounter >= world.GetVoxelHardness(markedCube.position.x, markedCube.position.y, markedCube.position.z))
            {
                clickCounter = 0;
                world.GetChunkFromVector3(markedCube.position).EditVoxel(markedCube.position, 0);
            }

            //create cube
            if(Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(placeCube.position).EditVoxel(placeCube.position, defaultCubeIndex);
            }
        }
    }

    private void HighlightCube()
    {
        float step = 0.1f;
        Vector3 lastPos = new Vector3();

        while(step < Constants.playerReach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if(world.IsVoxelHard(pos.x, pos.y, pos.z))
            {
                markedCube.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeCube.position = lastPos;

                markedCube.gameObject.SetActive(true);
                placeCube.gameObject.SetActive(true);

                return;
            }
            else
            {
                lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

                step += 0.1f; 
            }
        }

        markedCube.gameObject.SetActive(false);
        placeCube.gameObject.SetActive(false);
    }

    private void GetVelocity()
    {
        // Affect vertical momentum with gravity
        if(velocityMomentum > Constants.g)
        {
            velocityMomentum += Time.fixedDeltaTime * Constants.g; 
        }

        // move with player 
        if(sprinting)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * Constants.sprintSpeed;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * Constants.walkSpeed;
        }

        //Apply vertical momentum(falling/sprinting)
        velocity += Vector3.up * velocityMomentum * Time.fixedDeltaTime;

        // check if I can moves
        if((velocity.z > 0 && frontCheck()) || (velocity.z < 0 && backCheck())) // forward / back
        {
            velocity.z = 0;
        }
        if((velocity.x > 0 && rightCheck()) || (velocity.x < 0 && leftCheck())) // left / right
        {
            velocity.x = 0;
        }
        if(velocity.y < 0) // falling down
        {
            velocity.y = SpeedDown(velocity.y);
        }
        else if(velocity.y > 0) // jumping 
        {
            velocity.y = SpeedUp(velocity.y);
        }  
        
    }

    private float SpeedDown(float downSpeed)
    {
        //Check for cube for centre of the playet to radius of the player 
        if(world.IsVoxelHard(transform.position.x - Constants.playerWidth, transform.position.y + downSpeed, transform.position.z - Constants.playerWidth) ||
            world.IsVoxelHard(transform.position.x + Constants.playerWidth, transform.position.y + downSpeed, transform.position.z - Constants.playerWidth) ||
            world.IsVoxelHard(transform.position.x + Constants.playerWidth, transform.position.y + downSpeed, transform.position.z + Constants.playerWidth) ||
            world.IsVoxelHard(transform.position.x - Constants.playerWidth, transform.position.y + downSpeed, transform.position.z + Constants.playerWidth))
        {
            standing = true;
            return 0;
        }
        else
        {
            standing = false;
            return downSpeed;
        }
    }

    private float SpeedUp(float upSpeed)
    {
        //Check for playet for centre of the playet to radius of the player 
        if(world.IsVoxelHard(transform.position.x - Constants.playerWidth, transform.position.y + 2f /* up need to be positive and more then char high (1.8)*/ + upSpeed, transform.position.z - Constants.playerWidth) ||
            world.IsVoxelHard(transform.position.x + Constants.playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - Constants.playerWidth) ||
            world.IsVoxelHard(transform.position.x + Constants.playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + Constants.playerWidth) ||
            world.IsVoxelHard(transform.position.x - Constants.playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + Constants.playerWidth))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool frontCheck()
    {
        if(world.IsVoxelHard(transform.position.x, transform.position.y, transform.position.z + Constants.playerWidth) ||
            world.IsVoxelHard(transform.position.x, transform.position.y + 1f, transform.position.z + Constants.playerWidth))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool backCheck()
    {
        if(world.IsVoxelHard(transform.position.x, transform.position.y, transform.position.z + Constants.playerWidth) ||
            world.IsVoxelHard(transform.position.x, transform.position.y + 1f, transform.position.z + Constants.playerWidth))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool leftCheck()
    {
        if(world.IsVoxelHard(transform.position.x - Constants.playerWidth, transform.position.y, transform.position.z) ||
            world.IsVoxelHard(transform.position.x - Constants.playerWidth, transform.position.y + 1f, transform.position.z - Constants.playerWidth))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool rightCheck()
    {
        if(world.IsVoxelHard(transform.position.x + Constants.playerWidth, transform.position.y, transform.position.z) ||
            world.IsVoxelHard(transform.position.x + Constants.playerWidth, transform.position.y + 1f, transform.position.z))
        {
            return true;
        }
        else
        {
            return false;
        }
    }    
}
