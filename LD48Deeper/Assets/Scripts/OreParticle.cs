﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreParticle : MonoBehaviour
{
	[SerializeField]
	private Vector2 _curVel;
	private Vector2 _startVel;
	public float TowardsPlayerSpeed = 30;
	public float StartExplosiveSpeed = 20;
	public PlayerController Target;
	public float DeathDist = .3f;

	public float VelocityLerpTime = .75f;

	public AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
		_startVel = Random.insideUnitCircle.normalized * StartExplosiveSpeed;
		StartCoroutine(OrePixelCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	public Renderer Rend;
	public IEnumerator OrePixelCoroutine()
	{
		float t = 0;

		while(Vector2.Distance(transform.position, Target.transform.position) > DeathDist) {
			Vector2 towardsPlayer = (Target.transform.position - transform.position).normalized * TowardsPlayerSpeed;
			_curVel = Vector2.Lerp(_startVel, towardsPlayer, t / VelocityLerpTime);
			if(Vector2.Distance(Target.transform.position, transform.position) <= (_curVel.magnitude * Time.deltaTime)) {
				break;
			}
			transform.Translate(_curVel * Time.deltaTime);
			t += Time.deltaTime;
			yield return null;
		}

		Target.CollectOre(1);
		source.Play();
		Rend.enabled = false;
		while (source.isPlaying) {
			yield return null;
		}
		Destroy(this.gameObject);
	}


}
