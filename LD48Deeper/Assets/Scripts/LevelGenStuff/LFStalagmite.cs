using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class LFStalagmite : LevelFeature
{
	public bool Attached = true;
	public Vector3 HangPosition;
	public Transform PlayerTransform;
	public Rigidbody RB;
	public float MaxDist = 10;

	public float DetachDelay = .2f;
	private void Awake() {
		PlayerTransform = FindObjectOfType<PlayerController>().transform;
	}
	// Start is called before the first frame update
	void OnEnable()
    {
		Attached = true;
		RB.isKinematic = true;
	}

	protected override void OnSpawn() {
		HangPosition = transform.position;
	}

	[NaughtyAttributes.Button("TriggerFall")]
	private void TriggerFall() {
		Attached = false;
		StartCoroutine(Detach());
	}


	public void Update() {
		if (Attached) {
			float xDist = Mathf.Abs(PlayerTransform.position.x - transform.position.x);
			float yDist = transform.position.y - PlayerTransform.position.y;
			if (yDist < MaxDist && yDist > 0 && xDist < .5f) {
				TriggerFall();
			}
		}
	}

	public IEnumerator Detach() {
		transform.DOShakePosition(DetachDelay, .1f, 100);
		yield return new WaitForSeconds(DetachDelay);
		RB.isKinematic = false;
	}
}
