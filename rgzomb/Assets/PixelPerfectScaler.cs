using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPerfectScaler : MonoBehaviour {

    // Every Camera object that will be scaled up
    [SerializeField]
    private Camera[] CAMERAS;

    //The original resolution of the graphics which the script will use to calculate the scale of pixels on the user's screen resolution.
    [SerializeField]
    private Vector2 REFERENCE_RESOLUTION;

    // How often the screen will be updated in the event of a screen resize, increasing this will make it less frequent, but could help performance.
    [SerializeField]
    private float REFRESH_TIME; 

    [SerializeField]
    private int REFERENCE_PPU; // The reference Pixels Per Unit of the titles's sprites, used in the orthographic equation.

    private int scale_factor;

    void InitializeComponents() //Establishes the values for component-based constants.
    {

    }

    void Awake() //Called before the first frame.
    {
        InitializeComponents();
    }

    // Use this for initialization
    void Start () {
        InitializeComponents();
        StartCoroutine("UpdateScreen");
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("f4"))
        {
            ToggleFullscreen();
        }
	}

    IEnumerator UpdateScreen()
    {
        while (true)
        {
            /*Floors the player's screen resolution to how many times the Reference resolution height can go into it.
            Flooring it prevents distorted and uneven looking pixels by keeping it as an integer. */
            scale_factor = Mathf.FloorToInt(Screen.height / REFERENCE_RESOLUTION.y);

            //updates each orthographic size to suit the user's resolution, the reference one, and the PPU of the title's sprites.
            foreach (var item in CAMERAS)
            {
                item.orthographicSize = (Screen.height/(scale_factor*REFERENCE_PPU)) * .5f;
            }
            yield return new WaitForSecondsRealtime(REFRESH_TIME);
        }
    }

    void ToggleFullscreen()
    {
        if (Screen.fullScreen)
        {
            Screen.fullScreen = false; Screen.SetResolution(Mathf.RoundToInt(REFERENCE_RESOLUTION.x) * scale_factor, Mathf.RoundToInt(REFERENCE_RESOLUTION.y) * scale_factor, false);
        }
        else if (!Screen.fullScreen)
        {
            Screen.fullScreen = true; Screen.SetResolution(Screen.currentResolution.width,Screen.currentResolution.height, true);
        }
    }

}
