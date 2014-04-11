using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Mapper : MonoBehaviour
{
	// Stores the positions for later verification and access of data
	private float tileSizeX, tileSizeZ;
	private int cellsX, cellsZ;
	private float minX, minZ;

	// This computes the map that contains only the obstacles
	public Cell[][] ComputeObstacles ()
	{
		Cell[][] baseMap = new Cell[cellsX][];
		
		int layer = LayerMask.NameToLayer ("Obstacles");
		Vector3 pos = new Vector3 ();
		
		for (int x = 0; x < cellsX; x++) {
			baseMap [x] = new Cell[cellsZ];
			for (int y = 0; y < cellsZ; y++) {
				Cell c = new Cell ();
				pos.Set ((x + tileSizeX / 2 - cellsX / 2) * tileSizeX, 0f, (y + tileSizeZ / 2 - cellsZ / 2) * tileSizeZ);
				c.blocked = Physics.CheckSphere (pos, (tileSizeX + tileSizeZ) / 4, 1 << layer);
				baseMap [x] [y] = c;
				baseMap [x] [y].i = x;
				baseMap [x] [y].j = y;
			}
		}

		return baseMap;
	}
	
	public void ComputeTileSize (Vector3 floorMin, Vector3 floorMax, int cellsX, int cellsZ)
	{
		// Initial computation
		float dx = Mathf.Abs (floorMin.x - floorMax.x);
		float dz = Mathf.Abs (floorMin.z - floorMax.z);
		tileSizeX = dx / cellsX;
		tileSizeZ = dz / cellsZ;
		
		this.minX = floorMin.x;
		this.minZ = floorMin.z;
		
		SpaceState.Instance.tileSize = new Vector2 (tileSizeX, tileSizeZ);
		SpaceState.Instance.floorMin = floorMin;
	}
	
	// Precompute a timestamps number of maps in the future by simulating the enemies movement across the map
	public Cell[][][] PrecomputeMaps (Vector3 floorMin, Vector3 floorMax, int cellsX, int cellsZ, int timestamps, float stepSize, int ticksBehind = 0, Cell[][] baseMap = null)
	{
		// Initial computation
		this.cellsX = cellsX;
		this.cellsZ = cellsZ;
		ComputeTileSize (floorMin, floorMax, cellsX, cellsZ);
		
		// Compute the fixed obstacle map
		if (baseMap == null)
			baseMap = ComputeObstacles ();
		
		// Prepare the dataholders (used afterwards by the callers)
		GameObject[] en = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
		Enemy[] enemies = new Enemy[en.Length];
		for (int i = 0; i < en.Length; i++) {
			enemies [i] = en [i].GetComponent<Enemy> ();
			enemies [i].positions = new Vector3[timestamps];
			enemies [i].forwards = new Vector3[timestamps];
			enemies [i].rotations = new Quaternion[timestamps];
			enemies [i].cells = new Vector2[timestamps][];
		}
		Cell[][][] fullMap = new Cell[timestamps][][];
		
		List<List<Vector2>> cells = new List<List<Vector2>> ();
		// Prepare the cells by enemy
		for (int i = 0; i < enemies.Length; i++) {
			cells.Add (new List<Vector2> ());
		}
		
		
		// Foreach period time, we advance a stepsize into the future and compute the map for it
		for (int counter = 0; counter < timestamps; counter++) {
			// Simulate and store the values for future use
			foreach (Enemy e in enemies) {
				e.Simulate (stepSize);
				e.positions [counter] = e.GetSimulationPosition ();
				e.forwards [counter] = e.GetSimulatedForward ();
				e.rotations [counter] = e.GetSimulatedRotation ();
			}
				
			fullMap [counter] = ComputeMap (baseMap, enemies, cells);
			
			// Store the seen cells in the enemy class
			List<Vector2>[] arr = cells.ToArray ();
			for (int i = 0; i < enemies.Length; i++) {
				enemies [i].cells [counter] = arr [i].ToArray ();
				arr [i].Clear ();
			}
		}
	
		// From the last time to the first, pick a cell and look back in time to see if it was seen previously
		if (ticksBehind > 0)
			for (int counter = timestamps-1; counter >= 0; counter--) 
				for (int ticks = 1; ticks <= ticksBehind && counter - ticks > 0; ticks++) 
					foreach (Enemy e in enemies)
						foreach (Vector2 v in e.cells[counter - ticks])
							if (fullMap [counter - ticks] [(int)v.x] [(int)v.y].seen) {
								fullMap [counter] [(int)v.x] [(int)v.y].seen = true;
							}
		
		SpaceState.Instance.enemies = enemies;
		SpaceState.Instance.fullMap = fullMap;
		
		return fullMap;
	}
	
	
	// Precompute a timestamps number of maps in the future by simulating the enemies movement across the map
	public Cell[][][] PrecomputeMapsOverRhythm (Vector3 floorMin, Vector3 floorMax, int cellsX, int cellsZ, int timestamps, float stepSize, int ticksBehind = 0, Cell[][] baseMap = null)
	{
		// Initial computation
		this.cellsX = cellsX;
		this.cellsZ = cellsZ;
		ComputeTileSize (floorMin, floorMax, cellsX, cellsZ);
		
		// Compute the fixed obstacle map
		if (baseMap == null)
			baseMap = ComputeObstacles ();
		
		// Prepare the dataholders (used afterwards by the callers)
		GameObject[] en = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
		Enemy[] enemies = new Enemy[en.Length];
		for (int i = 0; i < en.Length; i++) {
			enemies [i] = en [i].GetComponent<Enemy> ();
			enemies [i].positions = new Vector3[timestamps];
			enemies [i].forwards = new Vector3[timestamps];
			enemies [i].rotations = new Quaternion[timestamps];
			enemies [i].cells = new Vector2[timestamps][];
		}
		Cell[][][] fullMap = new Cell[timestamps][][];
		
		List<List<Vector2>> cells = new List<List<Vector2>> ();
		// Prepare the cells by enemy
		for (int i = 0; i < enemies.Length; i++) {
			cells.Add (new List<Vector2> ());
		}
		
		
		// Foreach period time, we advance a stepsize into the future and compute the map for it
		for (int counter = 0; counter < timestamps; counter++) {
			// Simulate and store the values for future use
			int elapseIndex = (counter + 1) / FSMController.timeInterval;
			
			foreach (Enemy e in enemies) {
				if ((counter + 1) % FSMController.timeInterval == 0) {
					e.SimulateOverRhythm (stepSize, elapseIndex);
				} else {
					e.Simulate (stepSize);
				}
				e.positions [counter] = e.GetSimulationPosition ();
				e.forwards [counter] = e.GetSimulatedForward ();
				e.rotations [counter] = e.GetSimulatedRotation ();
			}
				
			fullMap [counter] = ComputeMap (baseMap, enemies, cells);
			
			// Store the seen cells in the enemy class
			List<Vector2>[] arr = cells.ToArray ();
			for (int i = 0; i < enemies.Length; i++) {
				enemies [i].cells [counter] = arr [i].ToArray ();
				arr [i].Clear ();
			}
		}
	
		// From the last time to the first, pick a cell and look back in time to see if it was seen previously
		if (ticksBehind > 0)
			for (int counter = timestamps-1; counter >= 0; counter--) 
				for (int ticks = 1; ticks <= ticksBehind && counter - ticks > 0; ticks++) 
					foreach (Enemy e in enemies)
						foreach (Vector2 v in e.cells[counter - ticks])
							if (fullMap [counter - ticks] [(int)v.x] [(int)v.y].seen) {
								fullMap [counter] [(int)v.x] [(int)v.y].seen = true;
							}
		
		SpaceState.Instance.enemies = enemies;
		SpaceState.Instance.fullMap = fullMap;
		
		return fullMap;
	}
	
	public Cell[][] ComputeMap (Cell[][] baseMap, Enemy[] enemies, List<List<Vector2>> cellsByEnemy)
	{
		Cell[][] im = new Cell[cellsX][];
		
		for (int x = 0; x < cellsX; x++) {
			im [x] = new Cell[cellsZ];
			for (int y = 0; y < cellsZ; y++) {
				im [x] [y] = baseMap [x] [y].Copy ();
			}
		}
		
		for (int i = 0; i < enemies.Length; i++) {
			Enemy enemy = enemies [i];
			// For every enemy, get their direction and current world position and scale into IM scale
			Vector2 dir = new Vector2 (enemy.GetSimulatedForward ().x, enemy.GetSimulatedForward ().z);
			
			// Convert enemy position into Grid Coordinates
			Vector2 pos = new Vector2 ((enemy.GetSimulationPosition ().x - minX) / tileSizeX, (enemy.GetSimulationPosition ().z - minZ) / tileSizeZ);
			Vector2 p = new Vector2 ();
			
			for (int x = -1; x <= 1; x++)
				for (int y = -1; y <= 1; y++)
					// Check map boundaries
					if (Mathf.FloorToInt (pos.x + x) >= 0 && Mathf.FloorToInt (pos.x + x) < cellsX && Mathf.FloorToInt (pos.y + y) >= 0 && Mathf.FloorToInt (pos.y + y) < cellsZ)
						// (everything  here is in world coord, so we must transform back from grid coord to world coord)
						// If the distance from the position of the guy to the middle of the 4 cells around him is leseer than the radius, we paint those cells
					if (Vector2.Distance (new Vector2 (enemy.GetSimulationPosition ().x, enemy.GetSimulationPosition ().z), new Vector2 ((Mathf.Floor (pos.x + x) + tileSizeX) * tileSizeX + minX, (Mathf.Floor (pos.y + y) + tileSizeZ) * tileSizeZ + minZ)) < enemy.radius) {
						im [Mathf.FloorToInt (pos.x + x)] [Mathf.FloorToInt (pos.y + y)].seen = true;
						cellsByEnemy [i].Add (new Vector2 (pos.x + x, pos.y + y));
					}
			
			// if tileSizeX != tileSizeZ we can be in big trouble!
			float dist = enemy.fovDistance / ((tileSizeX + tileSizeZ) / 2);

			for (int x = 0; x < cellsX; x++) {
				for (int y = 0; y < cellsZ; y++) {
					
					// Skip cells that are staticly blocked or seen by other enemies
					if (im [x] [y].blocked || im [x] [y].seen || im [x] [y].safe)
						continue;
					
					bool seen = false;
					
					for (int px = 0; px <= 1; px++) {
						for (int py = 0; py <= 1; py++) {
							
							// Destination of the ray
							p.Set (x + px, y + py);
							
							// Direction of the ray
							Vector2 res = (p - pos).normalized;
							
							// Is the target within our FoV?
							if (Vector2.Distance (p, pos) < dist && Vector2.Angle (res, dir) < enemy.fovAngle) {
								// Perform the DDA line algorithm
								// Based on http://lodev.org/cgtutor/raycasting.html
								
								//which box of the map we're in
								int mapX = Mathf.FloorToInt (pos.x);
								int mapY = Mathf.FloorToInt (pos.y);
       
								//length of ray from current position to next x or y-side
								float sideDistX;
								float sideDistY;
       
								//length of ray from one x or y-side to next x or y-side
								float deltaDistX = Mathf.Sqrt (1 + (res.y * res.y) / (res.x * res.x));
								float deltaDistY = Mathf.Sqrt (1 + (res.x * res.x) / (res.y * res.y));
       
								//what direction to step in x or y-direction (either +1 or -1)
								int stepX;
								int stepY;
								
								//calculate step and initial sideDist
								if (res.x < 0) {
									stepX = -1;
									sideDistX = (pos.x - mapX) * deltaDistX;
								} else {
									stepX = 1;
									sideDistX = (mapX + tileSizeX - pos.x) * deltaDistX;
								}
								if (res.y < 0) {
									stepY = -1;
									sideDistY = (pos.y - mapY) * deltaDistY;
								} else {
									stepY = 1;
									sideDistY = (mapY + tileSizeZ - pos.y) * deltaDistY;
								}
								
								bool done = im [x] [y].blocked || im [x] [y].seen;
								//perform DDA
								while (!done) {
									//jump to next map square, OR in x-direction, OR in y-direction
									if (sideDistX < sideDistY) {
										sideDistX += deltaDistX;
										mapX += stepX;
									} else {
										sideDistY += deltaDistY;
										mapY += stepY;
									}

									if (Vector2.Distance (pos, new Vector2 (mapX, mapY)) > Vector2.Distance (p, pos)) {
										seen = true;
										done = true;
									}
									// Check map boundaries
									if (mapX < 0 || mapY < 0 || mapX >= cellsX || mapY >= cellsZ) {
										seen = true;
										done = true;
									} else {
										//Check if ray has hit a wall
										if (im [mapX] [mapY].blocked) {
											done = true;
										}
										// End the algorithm
										if (x == mapX && y == mapY) {
											seen = true;
											done = true;
										}
									}
								}
							}
						}
					}
					if (seen)
						cellsByEnemy [i].Add (new Vector2 (x, y));
					
					im [x] [y].seen = seen;
				}
			}
		}
		return im;
	}
}
