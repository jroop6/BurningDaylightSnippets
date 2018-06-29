using UnityEngine;

public class CameraControl : MonoBehaviour {

	private Transform cameraTransform; // A handle to the main camera
	private MouseHandler mouseHandler; // A handle to the mouseHandler, for determining whether the camera is panning outside of the game area
	private Vector3 mainCenterPos;
	public float cameraZoomSpeed = 100.0f; // meters per second
	public float cameraPanSpeed = 20.0f; // meters per second
	public float cameraRotateSpeed = 60.0f; // degrees per second
	public float scrollThreshold = 5.0f; // The camera will begin panning when the mouse cursor is this close to the edge of the screen (in pixels)

    public float initialZoom = -40.0f;
	private float currentTheta = 0.0f;

	private bool controlsDisabled = false;

	// constraints:
	private float maxZoom = -90;
	private float minZoom = -20;
	private float minTheta = -10;
	private float maxTheta = 190;


	// Use this for initialization
	void Start () {
		// Gather references
		cameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
		mainCenterPos = GameObject.FindGameObjectWithTag ("MainCenter").transform.position;
		mouseHandler = GameObject.FindGameObjectWithTag ("MouseHandler").GetComponent<MouseHandler> ();

		// Place the camera at a default distance from the CameraTarget object
		cameraTransform.localPosition = new Vector3(0f,0f,initialZoom);

		// Place the camera over the main center:
		transform.position = mainCenterPos;

		// Rotate the camera to the default phi and theta angles:
		transform.rotation = Quaternion.identity;
		RotateTheta (40f);
		transform.Rotate (-Vector3.up*45f, Space.World);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (controlsDisabled) return;

		// If the player is using the scrollwheel, zoom the camera in or out:
		if(Input.mouseScrollDelta.y>0) cameraTransform.localPosition += new Vector3(0.0f, 0.0f, cameraZoomSpeed*RealTime.deltaTime);
		if(Input.mouseScrollDelta.y<0) cameraTransform.localPosition -= new Vector3(0.0f, 0.0f, cameraZoomSpeed*RealTime.deltaTime);

		// If the user is pressing the hotkeys for phi-rotation, rotate the camera:
		if(Input.GetKey(KeyCode.A)) transform.Rotate (Vector3.up*cameraRotateSpeed*RealTime.deltaTime, Space.World);
		if(Input.GetKey(KeyCode.D)) transform.Rotate (-Vector3.up*cameraRotateSpeed*RealTime.deltaTime, Space.World);

		// If the user is pressing the hotkeys for theta-rotation, rotate the camera:
		if (Input.GetKey (KeyCode.X)) RotateTheta (cameraRotateSpeed * RealTime.deltaTime);
		if (Input.GetKey (KeyCode.Z)) RotateTheta (-cameraRotateSpeed * RealTime.deltaTime);

		// If the cursor is close to the edge of the screen, pan the camera:
		Vector3 panMotion = Vector3.zero;
		Vector3 xzForward = Vector3.ProjectOnPlane (cameraTransform.up, Vector3.up).normalized;
		Vector3 xzRight = Vector3.ProjectOnPlane (cameraTransform.right, Vector3.up).normalized;
		if (Input.mousePosition.x < scrollThreshold) panMotion -= cameraPanSpeed * RealTime.deltaTime * xzRight;                   // pan left
		if (Input.mousePosition.y < scrollThreshold) panMotion -= cameraPanSpeed * RealTime.deltaTime * xzForward;                 // pan down
		if (Input.mousePosition.x > Screen.width - scrollThreshold) panMotion += cameraPanSpeed * RealTime.deltaTime * xzRight;    // pan right
		if (Input.mousePosition.y > Screen.height - scrollThreshold) panMotion += cameraPanSpeed * RealTime.deltaTime * xzForward; // pan up

		// If the pan motion would take the camera outside of the play area, then modify the pan motion to slide along an edge of the map.
		if(mouseHandler.xyz2ijk(transform.position + panMotion).x == -1) 
		{
			// Determine which edge we hit (well, actually, just the closest edge to our current position)
			MouseHandler.Edge edge = mouseHandler.FindClosestEdge(transform.position);
			switch (edge)
			{
			case MouseHandler.Edge.X_MIN:
			case MouseHandler.Edge.X_MAX:
				if (panMotion.z < 0) panMotion = new Vector3 (0f, 0f, -panMotion.magnitude);
				else panMotion = new Vector3 (0f, 0f, panMotion.magnitude);
				break;
			case MouseHandler.Edge.Z_MIN:
			case MouseHandler.Edge.Z_MAX:
				if (panMotion.x < 0) panMotion = new Vector3 (-panMotion.magnitude, 0f, 0f);
				else panMotion = new Vector3 (panMotion.magnitude, 0f, 0f);
				break;
			}
				
			// if we're STILL going off the edge of the map (i.e. we've hit a corner), stop camera movement.
			if(mouseHandler.xyz2ijk(transform.position + panMotion).x == -1) panMotion = Vector3.zero;
		}
		transform.position = transform.position + panMotion;

		// If the player hits the 'M' key, move the camera to the main center:
		if (Input.GetKeyDown (KeyCode.M)) transform.position = mainCenterPos;

		// Enforce camera constraints:
		if (cameraTransform.localPosition.z < maxZoom) cameraTransform.localPosition = new Vector3 (0f, 0f, maxZoom);
		if (cameraTransform.localPosition.z > minZoom) cameraTransform.localPosition = new Vector3 (0f, 0f, minZoom);
		if (currentTheta < minTheta) RotateTheta (minTheta - currentTheta);
		if (currentTheta > maxTheta) RotateTheta (maxTheta - currentTheta);
	}

	private void RotateTheta(float amount)
	{
		transform.Rotate(Vector3.right*amount);
		currentTheta += amount;
	}

	public void DisableControls()
	{
		controlsDisabled = true;
	}

	public void EnableControls()
	{
		controlsDisabled = false;
	}
}
