using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class RandomizePitch : MonoBehaviour
{
	[MinMaxSlider(.5f, 1.5f)]
	public Vector2 PitchRange;
	public AudioSource Audio;

    // Start is called before the first frame update
    void Awake()
    {
		Audio.pitch = Random.Range(PitchRange.x, PitchRange.y);
		Audio.Play();
    }

}
