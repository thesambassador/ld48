using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.SceneManagement;

using DG.Tweening;

public class PlayerController : MonoBehaviour
{
	private Rigidbody2D _rb;
	private AmmoController _ammo;

	private Vector2 _movementInput;
	private bool _firingWeapon = false;

	public float ControlledSpeed = 5;
	public float MaxForce = 10;
	public AnimationCurve AccelVelocityCurve;

	public Bullet BootBulletPrefab;
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
		_rb = GetComponent<Rigidbody2D>();
		_ammo = GetComponent<AmmoController>();

		
	}

    // Update is called once per frame
    void Update()
    {
		_timeSinceLastFired += Time.deltaTime;
	}

	private void FixedUpdate() {

		float targetVel = _movementInput.x * ControlledSpeed;
		float velDiff = targetVel - _rb.velocity.x;
		float normalizedVelDiff = Mathf.Abs(velDiff) / ControlledSpeed;

		
		float accel = AccelVelocityCurve.Evaluate(normalizedVelDiff / ControlledSpeed) * MaxForce * Mathf.Sign(velDiff);
		_rb.AddForce(Vector2.right * accel);

	}

	public void OnMove(InputValue val) {
		_movementInput = val.Get<Vector2>();
	}

	public void OnFireWeapon(InputValue val) {
		
		float pressedVal = val.Get<float>();
		if (pressedVal > 0) {
			_firingWeapon = true;
		}
		else {
			_firingWeapon = false;
		}
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
			if (_ammo.TrySpendAmmo(BootBulletPrefab.AmmoCost)) {
				if (_rb.velocity.y < 0) {
					Vector3 newVel = _rb.velocity;
					newVel.y *= .5f;
					_rb.velocity = newVel; //half downward velocity
				}
				_rb.AddForce(Vector2.up * GetBootImpulse(), ForceMode2D.Impulse);
				_timeSinceLastFired = 0;
				Bullet newBullet = Instantiate(BootBulletPrefab, BootBulletSpawnPoint.position, Quaternion.identity);
				newBullet.SetVelocityDirection(Vector2.down);

				Vector3 punch = Vector3.down * .2f + Vector3.left * .1f;
				SpriteTransform.DOComplete();
				SpriteTransform.DOPunchScale(punch, .2f);

			}
		}
	}
	public float BootFullChargeTime = .1f;
	public float BootMinImpulse = 2;
	public float GetBootImpulse() {
		float result = Mathf.Lerp(BootMinImpulse, BootImpulse, _timeSinceLastFired / BootFullChargeTime);
		print("boots: " + result);
		return result;
	}

	public void OnExit() {
		Application.Quit();
	}

	public float CollectOre(int amount) {
		_ammo.AddAmmo(amount * OreMultiplier);
		return (float)_ammo.CurrentAmmo / _ammo.MaxAmmo;
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

		_rb.velocity = Vector3.zero;
		_rb.isKinematic = true;
		SpriteTransform.gameObject.SetActive(false);
	}


	bool dead = false;
	public void OnCollisionEnter2D(Collision2D collision) {
		deathCollision = collision;
		print("vel: " + _rb.velocity);
		print("collision: " + collision.contacts[0].normal);
		if(collision.contacts[0].normal.y >= .99f) {//if we hit the top of a tile
			print("player vel: " + _rb.velocity.y);
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
