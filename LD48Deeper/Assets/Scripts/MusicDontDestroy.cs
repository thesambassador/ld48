using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class MusicDontDestroy : MonoBehaviour
{
	public static MusicDontDestroy Instance;
	public bool Music = true;
	public AudioSource Source;

	private float _savedVol;
    void Awake()
    {
        if(Instance != null) {
			Destroy(this.gameObject);
		}
		else {
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
			_savedVol = Source.volume;
		}
    }

	private void Update() {
		if (Keyboard.current.mKey.wasPressedThisFrame) {
			Music = !Music;
			if (Music) {
				Source.volume = _savedVol;
			}
			else {
				_savedVol = Source.volume;
				Source.volume = 0;
			}
		}
		
	}

}
