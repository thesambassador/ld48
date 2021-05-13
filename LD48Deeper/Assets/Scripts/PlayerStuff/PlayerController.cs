using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.SceneManagement;

using DG.Tweening;

public class PlayerController : MonoBehaviour
{
	public Rigidbody2D RB;
	public AmmoController AmmoRef;

	private Vector2 _movementInput;

	public float ControlledSpeed = 5;
	public float MaxForce = 10;
	public AnimationCurve AccelVelocityCurve;

	public BootGun MainGun;
	public Transform BootBulletSpawnPoint;

	public int OreMultiplier = 3;
	public AudioClip OreAudioClip;
	public AudioClip BounceAudioClip;
	public AudioSource PlayerAudioSource;

	public PauseHandler Pause;

	public float BootImpulse = 10;
	private float _timeSinceLastFired = 0;

	public Transform SpriteTransform;
    // Start is called before the first frame update
    void Start()
    {
		RB = GetComponent<Rigidbody2D>();
		AmmoRef = GetComponent<AmmoController>();
	}

    // Update is called once per frame
    void Update()
    {

	}

	private void FixedUpdate() {
		float targetVel = _movementInput.x * ControlledSpeed;
		float velDiff = targetVel - RB.velocity.x;
		float normalizedVelDiff = Mathf.Abs(velDiff) / ControlledSpeed;

		float accel = AccelVelocityCurve.Evaluate(normalizedVelDiff / ControlledSpeed) * MaxForce * Mathf.Sign(velDiff);
		RB.AddForce(Vector2.right * accel);
	}

	public void OnMove(InputValue val) {
		_movementInput = val.Get<Vector2>();
	}

	public void OnVolumeDown() {
		
		AudioListener.volume = Mathf.Clamp(AudioListener.volume - .1f, 0, 1);
	}

	public void OnVolumeUp() {
		AudioListener.volume = Mathf.Clamp(AudioListener.volume + .1f, 0, 1);
	}
	
	public void OnPause() {
		Pause.TogglePause();
	}

	public void OnFireBoots(InputValue val) {
		DeathPanel.OnFireBoots();
		if (!Pause.Paused) {
			if(MainGun != null) {
				MainGun.TryShootGun();
			}
		}
	}
	

	public void OnExit() {
		Application.Quit();
	}

	public float CollectOre(int amount) {
		AmmoRef.AddAmmo(amount * OreMultiplier);
		return (float)AmmoRef.CurrentAmmo / AmmoRef.MaxAmmo;
		//PlayerAudioSource.pitch = Random.Range(.9f, 1.6f);
		//PlayerAudioSource.PlayOneShot(OreAudioClip, 4);
	}

	public AudioClip DeathAudioClip;
	public ShowDeathPanel DeathPanel;

	public void PlayerDeath() {
		AudioSource.PlayClipAtPoint(DeathAudioClip, transform.position);
		DeathPanel.Show(transform.position);
		Camera.main.transform.parent = null;
		GetComponent<Collider2D>().enabled = false;

		RB.velocity = Vector3.zero;
		RB.isKinematic = true;
		SpriteTransform.gameObject.SetActive(false);
	}


	bool dead = false;
	public void OnCollisionEnter2D(Collision2D collision) {
		deathCollision = collision;
		print("vel: " + RB.velocity);
		print("collision: " + collision.contacts[0].normal);
		if(collision.contacts[0].normal.y >= .99f) {//if we hit the top of a tile
			print("player vel: " + RB.velocity.y);
			print(transform.position.y - collision.contacts[0].normal.y);
			PlayerDeath();
			dead = true;

		}
		else {
			if(!dead)
				deathCollision = collision;
			PlayerAudioSource.pitch = 1;
			PlayerAudioSource.PlayOneShot(BounceAudioClip);
		}
	}

	private Collision2D deathCollision;
	private void OnDrawGizmos() {
		if (deathCollision != null) {
			for (int i = 0; i < deathCollision.contactCount; i++) {
				ContactPoint2D contact = deathCollision.GetContact(i);
				Gizmos.DrawSphere(contact.point, .05f);
				Gizmos.DrawLine(contact.point, contact.point + contact.normal * .2f);
			}
		}
	}
}
