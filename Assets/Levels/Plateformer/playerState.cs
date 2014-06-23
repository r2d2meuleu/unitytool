﻿using UnityEngine;
using System.Collections;

public class PlayerState{

	public float movementSpeed;
	public float jumpPower;
	public int maxJumps;
	public bool isOnGround;
	public int numJumps;
	public Vector2 velocity;
	public Vector2 platformVelocity;
	public Vector2 gravity;

	public PlayerState(){
		movementSpeed = 0.15f;
		maxJumps = 2;
		isOnGround = false;
		numJumps = 0;
		velocity = new Vector2(0,0);
		platformVelocity = new Vector2(0,0);
		gravity = new Vector2(0, -0.020f);
		jumpPower = 0.5f;
	}

	public void reset(){
		isOnGround = false;
		numJumps = 0;
		velocity = new Vector2(0,0);
		platformVelocity = new Vector2(0,0);
	}

	public PlayerState clone(){
		PlayerState state = new PlayerState();
		state.isOnGround = isOnGround;
		state.numJumps = numJumps;
		state.velocity = new Vector2(velocity.x, velocity.y);
		state.platformVelocity = new Vector2(platformVelocity.x, platformVelocity.y);
		                                    
		return state;
	}

	public override string ToString ()
	{
		return "isOnGround:" + isOnGround + " -- numJumps:" + numJumps + " -- velocity:" + velocity + " -- Platvelocity:" + platformVelocity;
	}
}