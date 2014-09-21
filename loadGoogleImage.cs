using UnityEngine;
using System.Collections;
using System.Threading;
using System; 

public class loadGoogleImage : MonoBehaviour {

	private int imageWidth = 1080;
	private int imageHeight = 960;
	private double latitude = 40.7586486;
	private double longitude = -73.985376;
	private double dLatitude = 0.000001;
	private double dLongitude = 0.000001;
	private Texture2D[] textures;
	private int skyBoxFaces = 6;

	//Called upon initiliazation of the Unity engine.
	void Start() {
		//Download the textures from the Google Maps Street View API corresponding to the current latitude and longitude.
		DownloadTextures(latitude, longitude);
	}

	//Download textures from the Google Maps Street View API corresponding to the current latitude and longitude.
	void DownloadTextures(double latitude, double longitude) {

		//Initialize textures array for SkyBox mapping.
		textures = new Texture2D[skyBoxFaces];

		string url;
	
		//Rotate around the horizontal axis 360 degrees, downloading each corresponding Google Maps Street View image
		//at 90 degree intervals.
		for (int i = 0; i < skyBoxFaces; i++) {
			url = "http://maps.googleapis.com/maps/api/streetview" + "?size=" + imageHeight + "x" + imageWidth + "&location=" + latitude + "," + longitude + "&fov=90";

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

			//Download the image corresponding to the current URL using the Google Maps Street View API.
			WWW www = new WWW(url);

			//TODO: Make asynchronous?
			while (!www.isDone) {
				Debug.Log("Loading image from URL: " + url);
			}

			textures[i] = www.texture;
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
	void MoveForward() {
		latitude += dLatitude;
		longitude += dLongitude;
	}

	//Update the current latitude and longitude values by the specified differentials
	//in order to move backward.
	void MoveBackward() {
		latitude -= dLatitude;
		longitude -= dLongitude;
	}

	//Handle keyboard input.
	void HandleInput() {
		if (Input.GetKey("up")) {
			Debug.Log("UP Arrow Key Pressed.");
			MoveForward();
		}
		if (Input.GetKey("down")) {
			Debug.Log("DOWN Arrow Key Pressed.");
			MoveBackward();
		}
	}

	//Called once every frame.
	void Update() {
		//Handle all keyboard input appropriately.
		HandleInput();
	}
}
