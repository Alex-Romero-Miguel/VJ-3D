using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections;


// MoveCube manages cube movement. WASD + Cursor keys rotate the cube in the
// selected direction. If the cube is not grounded (has a tile under it), it falls.
// Some events trigger corresponding sounds.


public class MoveCube : MonoBehaviour
{
	public float rotSpeed; 			// Rotation speed in degrees per second
    public float fallSpeed; 		// Fall speed in the Y direction
    public AudioClip fallSound; 	// Sound to play when the cube starts falling
    public AudioClip[] moveSounds; 		// Sounds to play when the cube rotates

    private bool bMoving = false; 			// Is the object in the middle of moving?
	private bool bFalling = false; 			// Is the object falling?
	private float rotRemainder; 			// The angle that the cube still has to rotate before the current movement is completed
    private float rotDir; 					// Has rotRemainder to be applied in the positive or negative direction?
    private Vector3 initPos, initSize; 		// Initial position and size of the cube
    private Quaternion initRot; 	        // Initial rotation of the cube
    private Vector3 rotPoint, rotAxis; 		// Rotation movement is performed around the line formed by rotPoint and rotAxis
    private InputAction moveAction; 		// Input action to capture player movement (WASD + cursor keys)
    private LayerMask layerMask; 			// LayerMask to detect raycast hits with ground tiles only
    private Vector3 size, halfSize;
	
	
	// Determine if the cube is grounded by shooting a ray down from the cube location and 
	// looking for hits with ground tiles

    private bool isGrounded()
    {
        RaycastHit hit;

        if (isStanding()) return Physics.Raycast(transform.position, Vector3.down, out hit, size.y, layerMask);

        Vector3 offset = Vector3.zero;

        if (isLyingX()) offset = Vector3.right * halfSize.x / 2;
        else if (isLyingZ()) offset = Vector3.forward * halfSize.z / 2;
        
        return Physics.Raycast(transform.position + offset, Vector3.down, out hit, size.y, layerMask) && Physics.Raycast(transform.position - offset, Vector3.down, out hit, size.y, layerMask);
    }

    public bool isStanding()
    {
        return halfSize.x == halfSize.z;
    }

    public bool isLyingX()
    {
        return halfSize.y == halfSize.z;
    }

    public bool isLyingZ()
    {
        return halfSize.x == halfSize.y;
    }

    private void Awake()
    {
        initPos = transform.position;
        initRot = transform.rotation;

        BoxCollider box = GetComponent<BoxCollider>();
        initSize = box.bounds.size;
    }

    public void startGoalFalling()
    {
        bFalling = false;
        StartCoroutine(GoalFallAnimation());
    }

    private IEnumerator GoalFallAnimation()
    {
        while (bMoving)
            yield return null;

        float duration = 0.8f;
        Vector3 start = transform.position;
        Vector3 end = transform.position + Vector3.down * 3f;

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }

        LevelManager.Instance.CompleteLevel();
    }


    // Start is called once after the MonoBehaviour is created
    private void Start()
    {
		// Find the move action by name. Done once in the Start method to avoid doing it every Update call.
        moveAction = InputSystem.actions.FindAction("Move");
		
		// Create the layer mask for ground tiles. Done once in the Start method to avoid doing it every Update call.
        layerMask = LayerMask.GetMask("Ground");

        size = initSize;
        halfSize = initSize/2.0f; 
    }

    // Update is called once per frame
    private void Update()
    {
        if(bFalling)
        {
			// If we have fallen, we just move down
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            if (transform.position.y < -10f)
            {
                this.enabled = false;
                LevelManager.Instance.StartCoroutine(LevelManager.Instance.RestartLevel());
            }
        }
        else if (bMoving)
        {
			// If we are moving, we rotate around the line formed by rotPoint and rotAxis an angle depending on deltaTime
			// If this angle is larger than the remainder, we stop the movement
            float amount = rotSpeed * Time.deltaTime;
            if(amount > rotRemainder)
            {
                transform.RotateAround(rotPoint, rotAxis, rotRemainder * rotDir);
                bMoving = false;
            }
            else
            {
                transform.RotateAround(rotPoint, rotAxis, amount * rotDir);
                rotRemainder -= amount;
            }
        }
        else
        {
			// If we are not falling, nor moving, we check first if we should fall, then if we have to move
            if (!isGrounded())
            {
                bFalling = true;
				
				// Play sound associated to falling
                AudioSource.PlayClipAtPoint(fallSound, transform.position, 1.5f);
            }
			
			// Read the move action for input
            Vector2 dir = moveAction.ReadValue<Vector2>();
            if(Math.Abs(dir.x) > 0.99 || Math.Abs(dir.y) > 0.99)
            {
				// If the absolute value of one of the axis is larger than 0.99, the player wants to move in a non diagonal direction
                bMoving = true;
				
				// We play a random movemnt sound
                int iSound = UnityEngine.Random.Range(0, moveSounds.Length);
                AudioSource.PlayClipAtPoint(moveSounds[iSound], transform.position, 1.0f);
				
				// Set rotDir, rotRemainder, rotPoint, and rotAxis according to the movement the player wants to make
                if (dir.x > 0.99)
                {   // right
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = Vector3.forward;
                    rotPoint = transform.position + new Vector3(halfSize.x, -halfSize.y, 0.0f);
                    if (!isLyingZ()) halfSize = new Vector3(halfSize.y, halfSize.x, halfSize.z);
                }
                else if (dir.x < -0.99) 
                {   // left
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = Vector3.forward;
                    rotPoint = transform.position + new Vector3(-halfSize.x, -halfSize.y, 0.0f);
                    if (!isLyingZ()) halfSize = new Vector3(halfSize.y, halfSize.x, halfSize.z);
                }
                else if (dir.y > 0.99)  
                {   // forward
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = Vector3.right;
                    rotPoint = transform.position + new Vector3(0.0f, -halfSize.y, halfSize.z);
                    if (!isLyingX()) halfSize = new Vector3(halfSize.x, halfSize.z, halfSize.y);
                }
                else if (dir.y < -0.99) 
                {   // back
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = Vector3.right;
                    rotPoint = transform.position + new Vector3(0.0f, -halfSize.y, -halfSize.z);
                    if (!isLyingX()) halfSize = new Vector3(halfSize.x, halfSize.z, halfSize.y);
                }

                size = 2*halfSize;

            }
        }
    }

    public void Reset()
    {       
        transform.rotation = initRot;
        transform.position = initPos;

        size = initSize;
        halfSize = initSize/2.0f;

        bFalling = false;
        bMoving = false;
    }

    public bool isMoving()
    {
        return bMoving;
    }
}
