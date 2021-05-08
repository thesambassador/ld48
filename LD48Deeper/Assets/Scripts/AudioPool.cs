using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : MonoBehaviour
{
	public AudioClip PoolClip;
	public int PreloadedInitialSize = 5;

	private Queue<AudioSource> _availableSources;

    // Start is called before the first frame update
    void Awake()
    {
		_availableSources = new Queue<AudioSource>();
		for (int i = 0; i < PreloadedInitialSize; i++) {
			ExtendPool();
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void ExtendPool() {
		AudioSource newSource = this.gameObject.AddComponent<AudioSource>();
		newSource.clip = PoolClip;
		_availableSources.Enqueue(newSource);
	}

	public void Play(float pitch = 1) {

		if(_availableSources.Count == 0) {
			ExtendPool();
		}

		AudioSource source = _availableSources.Dequeue();
		source.pitch = pitch;
		StartCoroutine(PlayClipCoroutine(source));

	}

	private IEnumerator PlayClipCoroutine(AudioSource selectedSource) {
		selectedSource.Play();
		while (selectedSource.isPlaying) {
			yield return null;
		}
		_availableSources.Enqueue(selectedSource);
	}
}
