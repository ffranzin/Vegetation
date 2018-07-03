using UnityEngine;


public class FlyCamera : MonoBehaviour
{
	public bool dynamicNearPlane = true;
	public float minSpeed = 0.5f;
	public float mainSpeed = 10f; // Regular speed.
	public float shiftMultiplier = 2f;  // Multiplied by how long shift is held.  Basically running.
	public float camMouseSens = .35f;  // Camera sensitivity by mouse input.
	public float camJoyStickSens = 100f;  // Camera sensitivity by mouse input.
	private Vector3 lastMouse = new Vector3(Screen.width / 2, Screen.height / 2, 0); // Kind of in the middle of the screen, rather than at the top (play).
	//public float heightToMatchSurfaceUp = 40000;

	public bool clickToMove = true;

	public Vector3 currentUp = new Vector3(0, 1, 0);

	public static float velocity;

	private void Start()
	{
		MatchSurfaceNormal();
	}

	void MatchSurfaceNormal(float maxDegreesDelta = 9999)
	{
		//dvec3 planetCenter = SphericalTerrain.centerWithOffset;
		//currentUp = (transform.position.ToDvec3() - planetCenter).Normalized.ToVector3();
		//transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(transform.forward, currentUp), maxDegreesDelta);
	}

	void Update()
	{
		Vector3 lastPosition = transform.position;

		if (Input.GetKey(KeyCode.X))
			MatchSurfaceNormal(1f);
        
		ProcessMouse();

		float distance = (transform.position - lastPosition).magnitude;
		velocity = distance / Time.deltaTime;
		velocity *= 3.6f;
	}

	void ProcessJoystick()
	{
		transform.Rotate(currentUp, Input.GetAxis("RightAnalog_Horizontal") * Time.unscaledDeltaTime * camJoyStickSens, Space.World);

		Vector3 surfaceRight = Vector3.Cross(currentUp, transform.forward);
		transform.Rotate(surfaceRight, -Input.GetAxis("RightAnalog_Vertical") * Time.unscaledDeltaTime * camJoyStickSens, Space.World);

		mainSpeed += (Input.GetAxis("10th Joystick Axis") - Input.GetAxis("9th Joystick Axis")) * mainSpeed * 1.5f * Time.unscaledDeltaTime;
		if (mainSpeed < minSpeed)
			mainSpeed = minSpeed;

		float translateX = Input.GetAxis("LeftAnalog_Horizontal") * mainSpeed * Time.unscaledDeltaTime;
		float translateZ = -Input.GetAxis("LeftAnalog_Vertical") * mainSpeed * Time.unscaledDeltaTime;

		transform.Translate(new Vector3(translateX, 0, translateZ));
	}

	void ProcessMouse()
	{
		if (clickToMove)
		{
			if (!Input.GetMouseButton(0))
				return;

			if (Input.GetMouseButtonDown(0))
			{
				lastMouse = Input.mousePosition;
				return;
			}
		}

		mainSpeed += Input.GetAxis("Mouse ScrollWheel") * mainSpeed * 2f;
		mainSpeed += (Input.GetKey(KeyCode.Q) ? 0.01f : 0)  * mainSpeed * 2f;
		mainSpeed += (Input.GetKey(KeyCode.E) ? -0.01f : 0)  * mainSpeed * 2f;

		if (mainSpeed < minSpeed)
			mainSpeed = minSpeed;

		// Mouse input.
		Vector3 mouseDelta = Input.mousePosition - lastMouse;
		lastMouse = Input.mousePosition;

		// Mouse look
		transform.Rotate(currentUp, mouseDelta.x * camMouseSens, Space.World);

		Vector3 surfaceRight = Vector3.Cross(currentUp, transform.forward);
		transform.Rotate(surfaceRight, -mouseDelta.y * camMouseSens, Space.World);

		// Movement
		Vector3 p = getDirection();

		if (Input.GetKey(KeyCode.LeftShift))
			p = p * mainSpeed * shiftMultiplier;
		else
			p = p * mainSpeed;

		p = p * Time.unscaledDeltaTime;
		
		transform.Translate(p);
	}

	private Vector3 getDirection()
	{
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey(KeyCode.W))
		{
			p_Velocity += new Vector3(0, 0, 1);
		}
		if (Input.GetKey(KeyCode.S))
		{
			p_Velocity += new Vector3(0, 0, -1);
		}
		if (Input.GetKey(KeyCode.A))
		{
			p_Velocity += new Vector3(-1, 0, 0);
		}
		if (Input.GetKey(KeyCode.D))
		{
			p_Velocity += new Vector3(1, 0, 0);
		}
		if (Input.GetKey(KeyCode.Q))
		{
			p_Velocity += new Vector3(0, 1, 0);
		}
		if (Input.GetKey(KeyCode.E))
		{
			p_Velocity += new Vector3(0, -1, 0);
		}
		return p_Velocity;
	}

	public void resetRotation(Vector3 lookAt)
	{
		transform.LookAt(lookAt);
	}
}