using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class Timer : MonoBehaviour
{
	public TMP_Text FullTimeText;
	public TMP_Text Current250Text;
	public TMP_Text TimesText;

	public DepthDisplay DepthTracker;

	public float GameTime = 0;
	public float CurrentBlockTime = 0;
	public int DepthCheckpointDistance = 250;
	public int CurrentDepthCheckpoint = 250;

	public List<float> DepthTimes;

    // Start is called before the first frame update
    void Start()
    {
		DepthTimes = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
		GameTime += Time.deltaTime;
		CurrentBlockTime += Time.deltaTime;

		if(DepthTracker.CurDepth > CurrentDepthCheckpoint) {
			
			AddDepthTime(CurrentBlockTime, CurrentDepthCheckpoint);
			CurrentDepthCheckpoint += DepthCheckpointDistance;
			CurrentBlockTime = 0;
		}

		FullTimeText.text = string.Format("{0:0.00}", GameTime);
		Current250Text.text = string.Format("{0:0.00}", CurrentBlockTime);


	}

	void AddDepthTime(float newTime, int checkpoint) {
		DepthTimes.Add(CurrentBlockTime);
		TimesText.text += string.Format("\n{0} : {1:0.00}", checkpoint, newTime);
	}
}