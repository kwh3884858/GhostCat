﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleSprite : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{

	}
	private void OnTriggerEnter2D (Collider2D collision)
	{
		if (collision.name == "BornPlace") {
			GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerMove> ().canDoubleJump = true;

			Skylight.DialogPlayer.Load ("Altar");
		}

	}
}
