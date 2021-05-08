using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using TMPro;
using DG.Tweening;

public class ShowDeathPanel : MonoBehaviour
{
	public TMP_Text ScoreText;
	public TMP_Text HighScoreText;

	public CanvasGroup HighScoreCanvas;

	public bool GameOver = false;

	public float GameOverDelay = .5f;
	private float _gameOverTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

	// Update is called once per frame
	void Update() {
		_gameOverTimer -= Time.deltaTime;
	}

	public void OnFireBoots() {

		if (GameOver && _gameOverTimer <= 0) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	public void Show(Vector3 playerPos) {
		GameOver = true;
		int depth = -(int)playerPos.y;
		if (depth < 0) depth = 0;
		int highScore = GetHighScore();
		if(depth > highScore) {
			highScore = depth;
			PlayerPrefs.SetInt("HighScore", depth);
		}

		ScoreText.text = depth.ToString();
		HighScoreText.text = highScore.ToString();

		HighScoreCanvas.DOFade(1, .3f);
		_gameOverTimer = GameOverDelay;
	}

	public int GetHighScore() {
		return PlayerPrefs.GetInt("HighScore", 0);
	}
}
