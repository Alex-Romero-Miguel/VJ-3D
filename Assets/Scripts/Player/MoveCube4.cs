using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


// MoveCube manages cube movement. WASD + Cursor keys rotate the cube in the
// selected direction. If the cube is not grounded (has a tile under it), it falls.
// Some events trigger corresponding sounds.


public class MoveCube : MonoBehaviour
{
    public float grid;
	public float rotSpeed; 			        // Rotation speed in degrees per second
    public float fallAcceleration; 		    // Fall acceleration in the Y direction
    public AudioClip fallSound; 	        // Sound to play when the cube starts falling
    public AudioClip[] moveSounds; 		    // Sounds to play when the cube rotates

    private bool bMoving = false; 			// Is the object in the middle of moving?
	private bool bFalling = false; 			// Is the object falling?
    private bool animated = false;
    private bool divided = false;
    private float fallSpeed = 0f;
	private float rotRemainder; 			// The angle that the cube still has to rotate before the current movement is completed
    private float rotDir; 					// Has rotRemainder to be applied in the positive or negative direction?
    private Vector3 fullInitSize, halfInitSize; 		                    // Initial position and size of the cube
    private Vector3 fullInitPos, topHalfInitPos, bottomHalfInitPos;
    private Quaternion fullInitRot, bottomHalfInitRot, topHalfInitRot; 	    // Initial rotation of the cube
    private BoxCollider currentBox, fullBox, topHalfBox, bottomHalfBox;
    private Vector3 rotPoint, rotAxis; 		// Rotation movement is performed around the line formed by rotPoint and rotAxis
    private InputAction moveAction;         // Input action to capture player movement (WASD + cursor keys)
    private InputAction changeHalvesAction; 		
    private LayerMask layerMask; 			// LayerMask to detect raycast hits with ground tiles only
    private Vector3 size, halfSize;
    private Transform currentTransform;
    
    private Transform fullTransform, topHalfTransform, bottomHalfTransform;
    private LevelManager levelManager;

    public UnityEvent onMoveMade;

    [SerializeField] private AudioSource sfxSource;


    // Determine if the cube is grounded by shooting a ray down from the cube location and 
    // looking for hits with ground tiles

    private bool isGrounded()
    {
        RaycastHit hit;
        
        Physics.SyncTransforms();
        
        if (isStanding()) return Physics.Raycast(currentBox.bounds.center, Vector3.down, out hit, size.y, layerMask);

        Vector3 offset = Vector3.zero;

        if (isLyingX()) offset = Vector3.right * halfSize.x / 2;
        else if (isLyingZ()) offset = Vector3.forward * halfSize.z / 2;
        
        return Physics.Raycast(currentBox.bounds.center + offset, Vector3.down, out hit, size.y, layerMask) && Physics.Raycast(currentBox.bounds.center - offset, Vector3.down, out hit, size.y, layerMask);
    }

    private void prepareFallingRotation()
    {
        RaycastHit hit;

        if (isStanding()) return;

        Vector3 offset = Vector3.zero;
        bool plus = false;
        bool minus = false;
        
        if (isLyingX()) 
        {
            offset = Vector3.right * halfSize.x / 2;
            plus = Physics.Raycast(currentBox.bounds.center + offset, Vector3.down, out hit, size.y, layerMask);
            minus = Physics.Raycast(currentBox.bounds.center - offset, Vector3.down, out hit, size.y, layerMask);
            
            if (plus == minus) return;
            
            rotAxis = Vector3.forward;
            rotDir = plus ? 1f : -1f;

            rotPoint = currentBox.bounds.center + new Vector3(0f, -halfSize.y, 0f);

            rotRemainder = 90f;
        }
        else if (isLyingZ()) 
        {
            offset = Vector3.forward * halfSize.z / 2;
            plus = Physics.Raycast(currentBox.bounds.center + offset, Vector3.down, out hit, size.y, layerMask);
            minus = Physics.Raycast(currentBox.bounds.center - offset, Vector3.down, out hit, size.y, layerMask);

            if (plus == minus) return;
            
            rotAxis = Vector3.right;
            rotDir = plus ? -1f : 1f;

            rotPoint = currentBox.bounds.center + new Vector3(0f, -halfSize.y, 0f);
            
            rotRemainder = 90f;
        }        
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

    public bool isDivided()
    {
        return divided;
    }

    private void Awake()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();

        fullTransform = transforms[0];
        fullInitPos = fullTransform.position;
        fullInitRot = fullTransform.rotation;
        fullBox = fullTransform.GetComponent<BoxCollider>();
        fullInitSize = fullBox.bounds.size;

        topHalfTransform = transforms[1];
        topHalfInitPos = topHalfTransform.localPosition;
        topHalfInitRot = topHalfTransform.localRotation;
        topHalfBox = topHalfTransform.GetComponent<BoxCollider>();
        halfInitSize = topHalfBox.bounds.size;

        bottomHalfTransform = transforms[3];
        bottomHalfInitPos = bottomHalfTransform.localPosition;
        bottomHalfInitRot = bottomHalfTransform.localRotation;
        bottomHalfBox = bottomHalfTransform.GetComponent<BoxCollider>();
    }

    // Start is called once after the MonoBehaviour is created
    private void Start()
    {
		// Find the move action by name. Done once in the Start method to avoid doing it every Update call.
        moveAction = InputSystem.actions.FindAction("Move");
        changeHalvesAction = InputSystem.actions.FindAction("ChangeHalves");
		
		// Create the layer mask for ground tiles. Done once in the Start method to avoid doing it every Update call.
        layerMask = LayerMask.GetMask("Ground");

        levelManager = LevelManager.Instance;

        currentTransform = fullTransform;
        currentBox = fullBox;

        size = fullInitSize;
        halfSize = size/2.0f; 
    }

    // Update is called once per frame
    private void Update()
    {
        if (animated) return;

        if(bFalling)
        {
            if (rotRemainder > 0f)
            {
                float amount = 2f * rotSpeed * Time.deltaTime;
                currentTransform.RotateAround(rotPoint, rotAxis, amount * rotDir);
                rotRemainder -= amount;
            }
            else
            {
                fallSpeed += fallAcceleration * Time.deltaTime;
                currentTransform.position += Vector3.down * fallSpeed * Time.deltaTime;
            
                float amount = 2f * rotSpeed * Time.deltaTime;
                currentTransform.rotation = Quaternion.AngleAxis(amount * rotDir, rotAxis) * currentTransform.rotation;

                if (currentTransform.position.y < -10f) levelManager.Restart();
            }
        }
        else if (bMoving)
        {
			// If we are moving, we rotate around the line formed by rotPoint and rotAxis an angle depending on deltaTime
			// If this angle is larger than the remainder, we stop the movement
            float amount = rotSpeed * Time.deltaTime;
            if(amount <= rotRemainder)
            {
                currentTransform.RotateAround(rotPoint, rotAxis, amount * rotDir);
                rotRemainder -= amount;
            }
            else
            {
                currentTransform.RotateAround(rotPoint, rotAxis, rotRemainder * rotDir);
                currentTransform.position = Snap(currentTransform.position);

                rotRemainder = 0f;
                bMoving = false;

                if (divided)
                {
                    Physics.SyncTransforms();

                    Vector3 topHalfCenter = topHalfBox.bounds.center;
                    Vector3 bottomHalfCenter = bottomHalfBox.bounds.center;

                    if (Vector3.Distance(topHalfCenter, bottomHalfCenter) <= 1.1f * size.x) Recombine();
                }
            }
        }
        else if (!isGrounded())
        {
            prepareFallingRotation();

            bFalling = true;
            fallSpeed = fallAcceleration;
            
            // Play sound associated to falling
            sfxSource.PlayOneShot(fallSound, 1.5f);
        }
        else
        {
            if (divided)
            {
                if (changeHalvesAction.WasPressedThisFrame())
                    if (currentTransform == topHalfTransform) 
                    {
                        currentTransform = bottomHalfTransform;
                        currentBox = bottomHalfBox;
                    }
                    else 
                    {
                        currentTransform = topHalfTransform;
                        currentBox = topHalfBox;
                    }
            }

			// Read the move action for input
            Vector2 dir = moveAction.ReadValue<Vector2>();
            if(Math.Abs(dir.x) > 0.99 || Math.Abs(dir.y) > 0.99)
            {
				// If the absolute value of one of the axis is larger than 0.99, the player wants to move in a non diagonal direction
                bMoving = true;

                onMoveMade.Invoke();

                // We play a random movemnt sound
                int iSound = UnityEngine.Random.Range(0, moveSounds.Length);
                sfxSource.PlayOneShot(moveSounds[iSound], 1.0f);

                // Set rotDir, rotRemainder, rotPoint, and rotAxis according to the movement the player wants to make
                if (dir.x > 0.99)
                {   // right
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = Vector3.forward;
                    rotPoint = currentBox.bounds.center + new Vector3(halfSize.x, -halfSize.y, 0.0f);
                    if (!isLyingZ()) halfSize = new Vector3(halfSize.y, halfSize.x, halfSize.z);
                }
                else if (dir.x < -0.99) 
                {   // left
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = Vector3.forward;
                    rotPoint = currentBox.bounds.center + new Vector3(-halfSize.x, -halfSize.y, 0.0f);
                    if (!isLyingZ()) halfSize = new Vector3(halfSize.y, halfSize.x, halfSize.z);
                }
                else if (dir.y > 0.99)  
                {   // forward
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = Vector3.right;
                    rotPoint = currentBox.bounds.center + new Vector3(0.0f, -halfSize.y, halfSize.z);
                    if (!isLyingX()) halfSize = new Vector3(halfSize.x, halfSize.z, halfSize.y);
                }
                else if (dir.y < -0.99) 
                {   // back
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = Vector3.right;
                    rotPoint = currentBox.bounds.center + new Vector3(0.0f, -halfSize.y, -halfSize.z);
                    if (!isLyingX()) halfSize = new Vector3(halfSize.x, halfSize.z, halfSize.y);
                }

                size = 2*halfSize;
            }
        }
    }

    public void Reset()
    {
        fullTransform.rotation = fullInitRot;
        fullTransform.position = fullInitPos;

        topHalfTransform.localRotation = topHalfInitRot;
        topHalfTransform.localPosition = topHalfInitPos;

        bottomHalfTransform.localRotation = bottomHalfInitRot;
        bottomHalfTransform.localPosition = bottomHalfInitPos;

        currentTransform = fullTransform;
        currentBox = fullBox;

        size = fullInitSize;
        halfSize = size/2.0f;

        divided = false;
        bFalling = false;
        bMoving = false;
    }

    public bool isMoving()
    {
        return bMoving;
    }

    public void Divide(Vector3 topHalfPos, Vector3 bottomHalfPos)
    {
        divided = true;
        bMoving = false;
        bFalling = false;

        size = halfInitSize;
        halfSize = size/2.0f;

        topHalfBox.enabled = true;
        bottomHalfBox.enabled = true;

        fullTransform.position = fullInitPos;
        fullTransform.rotation = fullInitRot;

        topHalfTransform.rotation = topHalfInitRot;
        topHalfTransform.localPosition = topHalfInitPos;
        topHalfTransform.position = topHalfPos;

        bottomHalfTransform.rotation = bottomHalfInitRot;
        bottomHalfTransform.localPosition = bottomHalfInitPos;
        bottomHalfTransform.position = bottomHalfPos;

        currentTransform = topHalfTransform;
        currentBox = topHalfBox;

        fullBox.enabled = false;
    }

    public void Recombine()
    {
        divided = false;

        fullBox.enabled = true;

        Vector3 aux = (topHalfBox.bounds.center + bottomHalfBox.bounds.center) / 2.0f;
        fullTransform.position = Snap(aux);

        Vector3 dir = (topHalfBox.bounds.center - bottomHalfBox.bounds.center).normalized;
        fullTransform.rotation = Quaternion.LookRotation(Vector3.up, dir);

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z)) size = new Vector3(fullInitSize.y, fullInitSize.x, fullInitSize.z);
        else size = new Vector3(fullInitSize.x, fullInitSize.z, fullInitSize.y);
        halfSize = size/2.0f;
        
        topHalfTransform.localRotation = topHalfInitRot;
        topHalfTransform.localPosition = topHalfInitPos;

        bottomHalfTransform.localRotation = bottomHalfInitRot;
        bottomHalfTransform.localPosition = bottomHalfInitPos;

        currentTransform = fullTransform;
        currentBox = fullBox;
        
        topHalfBox.enabled = false;
        bottomHalfBox.enabled = false;
    }

    public IEnumerator AnimateSlide(float duration = 0.8f)
    {
        yield return new WaitUntil(() => !bMoving);

        animated = true;
        Vector3 start = currentTransform.position;
        Vector3 end = currentTransform.position + Vector3.down * 3f;

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            currentTransform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
        
        animated = false;
    }

    public void DisableMovement()
    {
        bMoving = false;
        animated = false;
    }

    public void SetInitPos(Vector3 pos)
    {
        fullInitPos = pos;
    }

    private Vector3 Snap(Vector3 p) {
        return new Vector3(
            Mathf.Round(p.x / grid) * grid,
            Mathf.Round(p.y / grid) * grid,
            Mathf.Round(p.z / grid) * grid
        );
    }
}
