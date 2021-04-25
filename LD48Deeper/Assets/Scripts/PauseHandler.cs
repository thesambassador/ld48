using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseHandler : MonoBehaviour
{
	public bool Paused = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void TogglePause() {
		Paused = !Paused;
		if (Paused) Time.timeScale = 0;
		else Time.timeScale = 1;
	}
}
