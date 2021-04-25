using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AmmoEvent : UnityEvent<int> { }

public class AmmoController : MonoBehaviour
{
	[SerializeField]
	private int _currentAmmo = 30;
	public int CurrentAmmo {
		get { return _currentAmmo; }
		set {
			if(_currentAmmo != value) {
				_currentAmmo = value;
				EventAmmoChanged?.Invoke(_currentAmmo);
			}
		}
	}
	public int MaxAmmo = 100;
	public AmmoEvent EventAmmoChanged = new AmmoEvent();
	
	public bool TrySpendAmmo(int amount) {
		if(_currentAmmo < amount) {
			return false;
		}
		else {
			CurrentAmmo -= amount;
			return true;
		}
	}

	public void AddAmmo(int amount) {
		CurrentAmmo = Mathf.Clamp(CurrentAmmo + amount, 0, MaxAmmo);
	}

}
