/*	This script will be attached to the player and will give him the various abilities and functions such as sprinting, zooming in the 
 * 	viewpoint, and crouching. This script will handle the speed of the player in these different states, as well as his height.
 * 	This merges the Run, CrouchTrigger and CrouchHeight scripts from the previous ProjectSpikes project. It also provides the ability to climb ladders
 * 	and pipes, in conjunction with LadderTriggers.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour {
	
	public static PlayerControl playerCont;	//public static reference to this script so we can access it through PlayerControl.playerCont
	public Transform player;				//a reference to the player so we can change his y-coord during crouching
	public CharacterController charCon;		//	and another reference, to the character controller
	public CharacterMotor charMot;			//a reference to the camera motor, so we can change the player speed between walking/running
	private Camera mainCam;					//a reference to the main camera attached to the player

	private bool isZoomed = false;			//is the camera currently zoomed?
	private float initZoom;					//the starting field of view of the camera
	private float playerZoom;				//	and the current zoom level the camera will lerp to

	private bool crouching = false;			//is the player currently crouching?
	private bool running = false;			//is the player running right now?
	private bool climbing = false;			//are we currently in a ladder trigger?

	private float initHeight;				//the initial player height
	private float playerHeight;				//	and the multiplier which determines what height we will lerp to
	private float lastHeight;				//a variable that's used each frame for smoothing the crouch process
	private List<float> initSpeed;			//a list that stores forward speed, sideways speed, backward speed, jump height, gravity and max fall speed
	private List<float> playerSpeed;		//	and another list that stores the speeds the player will currently be at, without lerping.

	public LayerMask mask;					//the layer mask used for the crouching raycast so we ignore triggers and the player itself

	void Start () {
		playerCont = this;					//set the public static reference to this script

		mainCam = Camera.main.camera;		//find the main camera and set its reference
		initZoom = mainCam.fieldOfView;		//now set the initial field of view variable to whatever the camera's fov is at the start
		playerZoom = initZoom;				//	and set the current field of view to the default
		initHeight = charCon.height;		//set the initial height variable to the starting y scale of the player transform

		initSpeed = new List<float>();						//create the list that stores initial speeds
		initSpeed.Add (charMot.movement.maxForwardSpeed);	//then set the initial speeds as the character motor's starting values
		initSpeed.Add (charMot.movement.maxSidewaysSpeed);	//	and then use multipliers on these values to determine the target speeds,
		initSpeed.Add (charMot.movement.maxBackwardsSpeed);	//		(playerSpeed variables)
		initSpeed.Add (charMot.jumping.extraHeight);		//and this variable is the default 'extra height' setting for jumping
		initSpeed.Add (40.0f);								//this is the default gravity for the character motor
		initSpeed.Add (50.0f);								//	and this is the default maximum falling speed

		playerSpeed = new List<float>();		//create the list that stores current speeds
		for(int i = 0; i < 6; i++)
			playerSpeed.Add (initSpeed[i]);		//assign the six initial speeds to playerSpeeds, so that the playerSpeeds list is six long
	}

	void Update () {
		if(Input.GetAxis ("Mouse ScrollWheel") < -0.05f && isZoomed)			//if we scroll back while zoomed in, zoom out.
		{
			isZoomed = false;				//now we're not zoomed in
			playerZoom = initZoom;			//	and the current FOV goes back to default
		}
		else if(Input.GetAxis ("Mouse ScrollWheel") > 0.05f && !isZoomed)		//else, when the player scrolls in, zoom in.
		{
			isZoomed = true;				//now we are zoomed in
			playerZoom = 25.0f;				//	and the FOV reduced, so we zoom inwards
		}
		mainCam.fieldOfView = Mathf.Lerp (mainCam.fieldOfView, playerZoom, Time.deltaTime * 10.0f);		//lerp to what the FOV should be

		for(int i = 0; i < 6; i++)
			playerSpeed[i] = initSpeed[i];		//set all six target speeds to default speeds, and only change them if we're crouching/running/climbing

		playerHeight = initHeight;				//each frame, set the target height to the start height, then change it if we're crouching
		crouching = false;						//	similarly, set crouching to false then change it if we're actually crouching
		//check if we are crouching. We're crouching if we press the crouch key, or if the ceiling is low (hence the raycast)
		if((Input.GetButton ("Crouch") && !running && !climbing) || Physics.Raycast(player.position, Vector3.up, 1.5f, mask))
		{
			crouching = true;					//we are now crouching, so half the player height and slow him down
			playerHeight = initHeight / 2.0f;	//our target height is half initial height
			for(int i = 0; i < 4; i++)
				playerSpeed[i] *= 0.25f;		//quarter all the playerSpeeds so we move much slower
		}

		lastHeight = charCon.height;										//before changing the player's height, what is his height now?
		charCon.height = Mathf.Lerp(charCon.height, playerHeight, Time.deltaTime * 5.0f);	//now, lerp to our target height (playerHeight)
		player.position += new Vector3(0.0f, (charCon.height - lastHeight) / 2, 0.0f);		//now fix the vertical position using height deltas

		running = false;							//set running to false then change to true if we are running
		if(Input.GetButton ("Run") && !crouching && !climbing)	//left control is the running key
		{
			running = true;					//Yes, script. We are running. Isn't it obvious? Stupid script.
			for(int j = 0; j < 4; j++)		//After C# has eaten me for insulting him, speed up the player and make his jump a bit higher
				playerSpeed[j] *= 1.5f;
		}

		if(climbing)						//if we are on a ladder, then we need to also check if we're pressing jump.
		{
			if(Input.GetButton ("Jump"))
			{
				playerSpeed[4] = 0.0f;		//if we are, the player experiences no gravity
				playerSpeed[5] = -3.5f;		//	and 'falls' upwards instead
			}
			else
			{
				playerSpeed[4] = 100.0f;	//if we're not, then the player has lots of gravity downwards, so he falls down
				playerSpeed[5] = 3.5f;		//	but the max fall speed is quite low
			}
		}

		charMot.movement.maxForwardSpeed = playerSpeed[0];		//now, set the player speeds to whatever has been determined in the script so far
		charMot.movement.maxSidewaysSpeed = playerSpeed[1];
		charMot.movement.maxBackwardsSpeed = playerSpeed[2];
		charMot.jumping.extraHeight = playerSpeed[3];
		charMot.movement.gravity = playerSpeed[4];
		charMot.movement.maxFallSpeed = playerSpeed[5];
	}

	public void ToggleClimbing () {
		climbing = !climbing;		//this switches the climbing variable to whatever it currently isn't
	}

	public bool Crouching () {
		return crouching;			//use this function in other scripts to find out our crouching status, without making crouching public
	}
}