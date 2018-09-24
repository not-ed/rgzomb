using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour {

    //Component Variables
    private Text C_TEXT; //The counter's text display on screen

    [SerializeField]
    private Text HIGHSCORE_TEXT; //The highscore counter text display.

    public static int currentScore = 0; //The counter's current score, used for reference.

    public static int currentHighScore; //The current high score of the game.


    void InitializeComponents() //Establishes the values for component-based constants.
    {
        C_TEXT = gameObject.GetComponent<Text>();
        currentHighScore = PlayerPrefs.GetInt("TOPSCORE",0);
    }

    void Awake()
    {
        InitializeComponents();   
    }

    // Use this for initialization
    void Start () {
        HIGHSCORE_TEXT.text = currentHighScore.ToString();
	}
	
	// Update is called once per frame
	void Update () {
        currentScore = Mathf.Clamp(currentScore,0,int.MaxValue);

        C_TEXT.text = currentScore.ToString("000000000");
        if (currentScore > currentHighScore)
        {
            currentHighScore = currentScore;
        }
        HIGHSCORE_TEXT.text = currentHighScore.ToString("000000000");
    }


    public static void WriteHighScore()
    {
        PlayerPrefs.SetInt("TOPSCORE", currentHighScore);
    }

}
