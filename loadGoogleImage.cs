using UnityEngine;
using System.Collections;
using System.Threading;
using System; 

public class loadGoogleImage : MonoBehaviour {
	
	private int imageWidth = 960;
	private int imageHeight = 1080;
	private double latitude = 40.7586486;
	private double longitude = -73.985376;
	private double dLatitudeLongitude = 0.000120;
	private int skyBoxFaces = 6;
	private Vector3 angles;
	private Texture2D[] textures;
	
	//Called upon initiliazation of the Unity engine.
	void Start() {
		//Download the textures from the Google Maps Street View API corresponding to the current latitudeitude and longitude.
		DownloadTextures(latitude, longitude);
	}
	
	//Download textures from the Google Maps Street View API corresponding to the current latitude and longitude.
	void DownloadTextures(double latitude, double longitude) {
		
		//Initialize textures array for SkyBox mapping.
		textures = new Texture2D[skyBoxFaces];

		WWW[] wwws = new WWW[skyBoxFaces];

		string url;
		
		//Rotate around the horizontal axis 360 degrees, downloading each corresponding Google Maps Street View image
		//at 90 degree intervals.
		for (int i = 0; i < skyBoxFaces; i++) {

			url = "http://maps.googleapis.com/maps/api/streetview?size=" + imageHeight + "x" + imageWidth + "&location=" + latitude + "," + longitude + "&fov=90";
			
			switch (i) {
			case 0:
			case 1:
			case 2:
			case 3:
				url += "&heading=" + (i * 90);
				break;
			case 4:
				url += "&pitch=90&heading=270";
				break;
			case 5:
				url += "&pitch=270&heading=270";
				break;
			}

			//api key
			url += "&key=AIzaSyB7zNiWruoxwecO8LLXsvN-uEeo3zd93Sg"; 

			//Download the image corresponding to the current URL using the Google Maps Street View API.
			wwws[i] = new WWW(url);

			//Debug.Log("Loading image from URL: " + url);


		}
		bool allWWWLoaded = false; //true so the first iteration can fire
		while (!allWWWLoaded) {
			//if allWWWLoaded is set to false there is a texture still loading.
			allWWWLoaded = true; 
			for (int i=0; i<skyBoxFaces; i++) {
				//TODO: Make asynchronous? - Cannot. Unity doesn't allow it.
				if(wwws[i].isDone) 
					textures[i] = wwws[i].texture;
				else
					allWWWLoaded = false;
			}
		}


		//Map all of the downloaded textures to the appropriate positions on the SkyBox map.
		MapTextures();
	}
	
	////Map all downloaded images to the correct locations on the unwrapped texture map of the SkyBox.
	void MapTextures() {

		//Grab both of the cameras for each lens on the Oculus Rift.
		GameObject[] cameras = {GameObject.FindWithTag("LeftCam"), GameObject.FindWithTag("RightCam")};
		
		//Map each image to the correct location on the unwrapped texture map of the SkyBox.
		foreach (GameObject camera in cameras) {
			Skybox skyBox = camera.GetComponent<Skybox>();
			Material material = skyBox.material;
			//The unwrapped SkyBox texture map is inverted along the y axis (counter-clockwise rotation).
			material.SetTexture("_LeftTex", textures[0]);
			material.SetTexture("_BackTex", textures[1]);
			material.SetTexture("_RightTex", textures[2]);
			material.SetTexture("_FrontTex", textures[3]);
			material.SetTexture("_UpTex", textures[4]);
			material.SetTexture("_DownTex", textures[5]);
		}

	}
	
	//Update the current latitude and longitude values by the specified differentials
	//in order to move forward.
	void Move() {
		bool keyPressed = false;
		//angles.y - 90f -- the 90f is the degree to fix the position of the occulus straight against the camera
		if (Input.GetKey ("up")) {
			latitude += dLatitudeLongitude * Math.Cos ((angles.y - 90f) * Math.PI / 180);
			longitude += dLatitudeLongitude * Math.Sin ((angles.y - 90f) * Math.PI / 180);
			keyPressed = true;
		}
		if (Input.GetKey ("down")) {
			latitude -= dLatitudeLongitude * Math.Cos ((angles.y - 90f) * Math.PI / 180);
			longitude -= dLatitudeLongitude * Math.Sin ((angles.y - 90f) * Math.PI / 180);
			keyPressed = true;
		}
		if (Input.GetKey ("left")) {
			latitude -= dLatitudeLongitude * Math.Sin ((angles.y - 90f) * Math.PI / 180);
			longitude -= dLatitudeLongitude * Math.Cos ((angles.y - 90f) * Math.PI / 180);
			keyPressed = true;
		}
		if (Input.GetKey ("right")) {
			latitude += dLatitudeLongitude * Math.Sin ((angles.y - 90f) * Math.PI / 180);
			longitude += dLatitudeLongitude * Math.Cos ((angles.y - 90f) * Math.PI / 180);
			keyPressed = true;
		}
		
		//TODO: Download the new textures after updating the current latitudeitude and longitude.
		if(keyPressed)
			DownloadTextures(latitude, longitude);
	}
	DateTime throttle = DateTime.Now;
	//Handle keyboard input.
	void HandleInput() {
		if ((DateTime.Now - throttle).TotalMilliseconds > 100) {
			Move();
			throttle = DateTime.Now;
		}
	}

	void MenuWindow(int id) {

	}

	void UpdateOrientation() {
		angles = GameObject.FindWithTag ("LeftCam").transform.eulerAngles;
	}

	//Called once every frame.
	void Update() {
		UpdateOrientation();
		//Handle all keyboard input appropriately.
		HandleInput();
	}
}