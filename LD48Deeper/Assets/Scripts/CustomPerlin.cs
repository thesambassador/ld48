using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPerlin 
{
	public float MainOffset;
	public float Scale;
	public bool Invert = false;
	public CustomPerlin(float offset, float multiplier, bool invert = false) {
		MainOffset = offset;
		Scale = multiplier;
		Invert = invert;
	}

	public float GetPerlin(float x, float y) {
		float xFactor = (x + MainOffset) * Scale;
		float yFactor = (y + MainOffset) * Scale;
		float result = Mathf.PerlinNoise(xFactor, yFactor);
		if (Invert) {
			return 1 - result;
		}
		return result;
	}

	public Texture2D OutputToTexture(int width, int height, int startX, int startY) {
		Texture2D result = new Texture2D(width, height);
		result.filterMode = FilterMode.Point;
		Color[] pix = new Color[width * height];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				float sample = GetPerlin(x+startX, y+startY);
				pix[(int)y * width + (int)x] = new Color(sample, sample, sample);
			}
		}

		result.SetPixels(pix);
		result.Apply();
		return result;
	}
}
