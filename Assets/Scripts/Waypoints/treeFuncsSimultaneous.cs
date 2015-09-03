#define includeTreeFuncsSimultaneous
#if includeTreeFuncsSimultaneous
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
public partial class Visibility1 : MonoBehaviour 
{
	Hashtable m_hCompleteNodeTable = new Hashtable();
	private int m_lastPathIndex = 10;
	private void DumpInfoFile(string dirName,float totalTime)
	{
		string sourceFileName = dirName+"\\Info"+".txt";
		StreamWriter sw = new StreamWriter(sourceFileName);
		sw.WriteLine("Scene Name = "+currSceneName+"");
		sw.WriteLine("Last Path Index = "+lastPathIndex()+"");
		sw.WriteLine("Discrete rows & cols = "+discretePtsX+" X "+discretePtsZ+"");
		sw.WriteLine("Speed of the Player = "+speedPlayer+"");
		sw.WriteLine("Distance covered by the player = "+m_stepDistance+"");
		sw.WriteLine("Max speed of the Enemy = "+speedEnemy+"");
		sw.WriteLine("Max distance covered by the Enemy = "+standardMaxMovement+"");
		sw.WriteLine("Time taken to calculate tree structure = "+totalTime+" mins"+"");
		sw.WriteLine("Current Date = " + System.DateTime.Now.ToLongDateString()+". Time = "+ System.DateTime.Now.ToLongTimeString() + "");
		sw.Close ();
	}
	private void DumpEdgesForLevel(Hashtable h_mapPtToNode,int levelOfAccess,string dirName)
	{
		string sourceFileName = dirName+"\\Edges"+levelOfAccess+".txt";
		StreamWriter sw = new StreamWriter(sourceFileName);
		//sw.WriteLine("(Vector3;level)|(Vector3;level)"+"");
		foreach(Vector3 vect in h_mapPtToNode.Keys)
		{
			NodeShadow nodeNow = (NodeShadow)h_mapPtToNode[vect];
			foreach(NodeShadow nodeNowParent in nodeNow.getParent())
			{
				sw.Write("("+nodeNow.getPos().x+","+nodeNow.getPos().y+","+nodeNow.getPos().z+";"+nodeNow.getSafetyLevel()+")|("+nodeNowParent.getPos().x+","+nodeNow.getPos().y+","+nodeNow.getPos().z+";"+nodeNowParent.getSafetyLevel()+")");
				sw.WriteLine("");
			}
		}
		sw.Close ();
	}
	private void executeTrueCase4()
	{
		setGlobalVars1();
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Hashtable relationMap = createRelationships ();
		float startTime = Time.realtimeSinceStartup;
		string dirName = createSaveDataDir(Application.dataPath);
		int levelOfAccess = 1;
		
		
		
		
		while(true)
		{
			Hashtable h_mapChild_Parent = new Hashtable();
			if(levelOfAccess>pathPoints.Count-1)
				break;
			int j1=0;
			for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
			{
				int k1=0;
				for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
				{
					//Debug.Log(j1+" , "+k1);
					Vector3 pt = new Vector3(j,1,k);
					
					//sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [levelOfAccess]];
					if(!pointInShadow(pt,levelOfAccess))
					//if(shadowArray[j1,k1]!=0)
					{
						k1++;
						continue;
					}
					
					Vector2 keyTemp = new Vector2((float)j1,(float)k1);
					//pt = (Vector3)h_mapIndxToPt[keyTemp];
					fillMapWithChildren4(pt,keyTemp,levelOfAccess,h_mapChild_Parent,relationMap);
					k1++;
					
				}
				
				j1++;
			}
			DumpEdgesForLevel3(h_mapChild_Parent,levelOfAccess,dirName);
			levelOfAccess++;
		}
		float totalTime = (Time.realtimeSinceStartup - startTime)/60;
		Debug.Log("executeTrueCase Finished. Time taken is = "+totalTime+" mins");
		DumpInfoFile (dirName,totalTime);
	}
	private Hashtable createRelationships()
	{
		Hashtable relationMap = new Hashtable ();
		int j1=0;
		for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
		{
			int k1=0;
			for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
			{
				//Debug.Log(j1+" , "+k1);
				Vector3 pt = new Vector3(j,1,k);
				relationMap = findNeighbors(pt,j1,k1,relationMap);
				k1++;
			}
			j1++;
		}
		return relationMap;		

	}
	private Hashtable findNeighbors(Vector3 pt,int j1,int k1,Hashtable relationMap)
	{
		int rowJ = j1;
		int colK = k1;
		Vector3 currPos = pt;//((Vector3)h_mapIndxToPt[keyTemp]);
		List<Vector3> listOfAvailablePos = new List<Vector3> ();
		
		listOfAvailablePos.Add(pt);

		
		bool runAgain = true;
		Vector3 vectPos = new Vector3 ();
		while(runAgain)
		{
			runAgain = false;
			rowJ--;
			colK--;
			int rowLen = (j1 - rowJ)*2 +1;
			//if(rowJ<0 || colK<0 || rowJ+rowLen>discretePtsX || colK+rowLen>discretePtsZ)
			//	break;
			for(int i1=rowJ;i1<rowJ+rowLen;i1++)
			{
				Vector2 vect2Pos = new Vector2(i1,colK);
				if(h_mapIndxToPt.ContainsKey(vect2Pos))
				{
				
					vectPos = (Vector3)h_mapIndxToPt[vect2Pos];

					if(Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
					{
						runAgain = true;
						listOfAvailablePos.Add(vectPos);

					}
				}
				vect2Pos = new Vector2(i1,colK+rowLen-1);
				if(h_mapIndxToPt.ContainsKey(vect2Pos))
				{
					vectPos = (Vector3)h_mapIndxToPt[vect2Pos];

					if(Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
					{
						runAgain = true;
						listOfAvailablePos.Add(vectPos);

					}
				}
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector2 vect2Pos = new Vector2(rowJ,i2);
				if(h_mapIndxToPt.ContainsKey(vect2Pos))
				{
					vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
					if(Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
					{
						runAgain = true;
						listOfAvailablePos.Add(vectPos);

					}
				}
				vect2Pos = new Vector2(rowJ+rowLen-1,i2);
				if(h_mapIndxToPt.ContainsKey(vect2Pos))
				{
					vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
					if(Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
					{
						runAgain = true;
						listOfAvailablePos.Add(vectPos);

					}
				}
			}
		}
		relationMap.Add(pt,listOfAvailablePos);
		return relationMap;
	}
	private void fillMapWithChildren4(Vector3 pt,Vector2 keyTemp,int levelOfAccess,Hashtable h_mapChild_Parent,Hashtable relationMap)
	{
		int rowJ = (int)keyTemp.x;
		int colK = (int)keyTemp.y;
		//standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Vector3 currPos = pt;//((Vector3)h_mapIndxToPt[keyTemp]);
		List<Vector3> listOfAvailablePos = new List<Vector3> ();
		
		List<Vector3> ptList = (List<Vector3>)relationMap [pt];
		foreach(Vector3 pt1 in ptList)
		{
			if(pointInShadow(pt1,levelOfAccess-1))
			{
				listOfAvailablePos.Add(pt1);
			}
		}
		h_mapChild_Parent.Add(pt,listOfAvailablePos);
		

	}
	private void executeTrueCase3()
	{
		setGlobalVars1();
		createDiscreteMap ();
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Debug.Log ("Initialize standardMaxMovement = " + standardMaxMovement);
		float startTime = Time.realtimeSinceStartup;
		string dirName = createSaveDataDir(Application.dataPath);
		int levelOfAccess = 1;




		while(true)
		{
			Hashtable h_mapChild_Parent = new Hashtable();
			if(levelOfAccess>pathPoints.Count-1)
				break;
			int j1=0;
			for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
			{
				int k1=0;
				for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
				{
					//Debug.Log(j1+" , "+k1);
					Vector3 pt = new Vector3(j,1,k);

					sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [levelOfAccess]];
					//if(!pointInShadow(pt,levelOfAccess))
					if(shadowArray[j1,k1]!=0)
					{
						k1++;
						continue;
					}

					Vector2 keyTemp = new Vector2((float)j1,(float)k1);
					//pt = (Vector3)h_mapIndxToPt[keyTemp];
					fillMapWithChildren(pt,keyTemp,levelOfAccess,h_mapChild_Parent);
					k1++;
					
				}
				
				j1++;
			}
			DumpEdgesForLevel3(h_mapChild_Parent,levelOfAccess,dirName);
			levelOfAccess++;
		}
		float totalTime = (Time.realtimeSinceStartup - startTime)/60;
		Debug.Log("executeTrueCase Finished. Time taken is = "+totalTime+" mins");
		DumpInfoFile (dirName,totalTime);
	}
	private void DumpEdgesForLevel3(Hashtable h_mapChild_Parent,int levelOfAccess,string dirName)
	{
		string sourceFileName = dirName+"\\Edges"+levelOfAccess+".txt";
		StreamWriter sw = new StreamWriter(sourceFileName);


		foreach(Vector3 key in h_mapChild_Parent.Keys)
		{
			List<Vector3> ptList = (List<Vector3>)h_mapChild_Parent[key];
			foreach(Vector3 pt in ptList)
			{
				sw.Write("("+key.x+","+key.y+","+key.z+";"+levelOfAccess+")|("+pt.x+","+pt.y+","+pt.z+";"+(levelOfAccess-1)+")");
				sw.WriteLine("");
			}
		}

		sw.Close ();
	}
	private void fillMapWithChildren(Vector3 pt,Vector2 keyTemp,int levelOfAccess,Hashtable h_mapChild_Parent)
	{
		int rowJ = (int)keyTemp.x;
		int colK = (int)keyTemp.y;
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Vector3 currPos = pt;//((Vector3)h_mapIndxToPt[keyTemp]);
		List<Vector3> listOfAvailablePos = new List<Vector3> ();

		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [levelOfAccess-1]];
		if(shadowArray[rowJ,colK]==0)
		{
			listOfAvailablePos.Add(pt);
		}
		
		bool runAgain = true;

		while(runAgain)
		{
			runAgain = false;
			rowJ--;
			colK--;
			int rowLen = ((int)keyTemp.x - rowJ)*2 +1;
			//if(rowJ<0 || colK<0 || rowJ+rowLen>discretePtsX || colK+rowLen>discretePtsZ)
			//	break;
			for(int i1=rowJ;i1<rowJ+rowLen;i1++)
			{
				Vector2 vect2Pos = new Vector2(i1,colK);
				Vector3 vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
				//if(pointInShadow(vectPos,levelOfAccess-1) && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				if(shadowArray[i1,colK]==0 && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				{
					runAgain = true;
					listOfAvailablePos.Add(vectPos);
					//h_mapChild_Parent.Add(pt,vectPos);
				}
				vect2Pos = new Vector2(i1,colK+rowLen-1);
				vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
				//if(pointInShadow(vectPos,levelOfAccess-1) && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				if(shadowArray[i1,colK+rowLen-1]==0 && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				{
					runAgain = true;
					listOfAvailablePos.Add(vectPos);
					//h_mapChild_Parent.Add(pt,vectPos);
				}
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector2 vect2Pos = new Vector2(rowJ,i2);
				Vector3 vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
				if(shadowArray[rowJ,i2]==0 && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				{
					runAgain = true;
					listOfAvailablePos.Add(vectPos);
					//h_mapChild_Parent.Add(pt,vectPos);
				}
				vect2Pos = new Vector2(rowJ+rowLen-1,i2);
				vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
				if(shadowArray[rowJ+rowLen-1,i2]==0 && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				{
					runAgain = true;
					listOfAvailablePos.Add(vectPos);
					//h_mapChild_Parent.Add(pt,vectPos);
				}
			}
		}
		h_mapChild_Parent.Add(pt,listOfAvailablePos);
	}
	private void executeTrueCase2()
	{
		setGlobalVars1();
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Debug.Log ("Initialize standardMaxMovement = " + standardMaxMovement);
		float startTime = Time.realtimeSinceStartup;
		string dirName = createSaveDataDir(Application.dataPath);


		//if(m_ContinueExecuteTrueCase)
		//	continueExecuteTrueCase (dirName);
		//else
			executeTrueCaseFor2(dirName);
		
		float totalTime = (Time.realtimeSinceStartup - startTime)/60;
		Debug.Log("executeTrueCase Finished. Time taken is = "+totalTime+" mins");
		DumpInfoFile (dirName,totalTime);

	}
	private void executeTrueCaseFor2(string dirName)
	{
		Hashtable h_mapPtToNode = new Hashtable();
		int levelOfAccess = 0;
		List<NodeShadow> nodeSafeLevelNow = new List<NodeShadow> ();

		/*Vector3 pt = new Vector3 (-3.8f, 1.0f, -3.5f);
		NodeShadow headNode = new NodeShadow (pt);
		headNode.setSafetyLevel (levelOfAccess);
		nodeSafeLevelNow.Add (headNode);*/
		int j1=0;
		for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
		{
			int k1=0;
			for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
			{
				//Debug.Log(j1+" , "+k1);
				Vector3 pt = new Vector3(j,1,k);
				if(bMultiplePaths)
				{
					if(!pointInShadowMultiplePaths(pt,0))
					{
						k1++;
						continue;
					}
				}
				else 
				{
					if(!pointInShadow(pt,0))
					{
						k1++;
						continue;
					}
				}
				Vector2 keyTemp = new Vector2((float)j1,(float)k1);
				pt = (Vector3)h_mapIndxToPt[keyTemp];
				NodeShadow headNode = new NodeShadow (pt);
				headNode.setSafetyLevel (levelOfAccess);
				nodeSafeLevelNow.Add (headNode);

				k1++;

			}

			j1++;
		}
		int numOfLevels = lastPathIndex ();//pathPoints.Count-1;//m_lastPathIndex;
		while(levelOfAccess<numOfLevels)//TODO:think other exit cases
		{
			levelOfAccess++;
			
			foreach(NodeShadow node in nodeSafeLevelNow)
			{
				Vector2 indexOfPtTemp = (Vector2)h_mapPtToIndx[node.getPos()];
				reachableChildren2 (node,indexOfPtTemp,levelOfAccess,h_mapPtToNode);
			}
			nodeSafeLevelNow = new List<NodeShadow> ();
			foreach(Vector3 vect in h_mapPtToNode.Keys)
			{
				nodeSafeLevelNow.Add((NodeShadow)h_mapPtToNode[vect]);
			}
			DumpEdgesForLevel(h_mapPtToNode,levelOfAccess,dirName);
			h_mapPtToNode.Clear();

		}
	}
	private bool addPossibleChild2(Vector2 tempVect2,NodeShadow node,int pathPointIndx,Hashtable h_mapPtToNode)
	{
		//Debug.Log ("Possible Child 1 ="+(Vector3)h_mapIndxToPt [tempVect2]);
		if(h_mapIndxToPt.ContainsKey(tempVect2))
		{
			Vector3 tempVect3 = (Vector3)h_mapIndxToPt[tempVect2];
			//Vector4 tempVect4 = new Vector4(tempVect3.x,tempVect3.y,tempVect3.z,pathPointIndx);
			//Debug.Log("standardMaxMovement = "+standardMaxMovement);
			//Debug.Log("Possible Child 2 = "+tempVect3);
			if(bMultiplePaths)
			{
				if(pointInShadowMultiplePaths(tempVect3,pathPointIndx) && Vector3.Distance(node.getPos(),tempVect3)<=standardMaxMovement  && CheckStraightLineVisibility(node.getPos(),tempVect3))
				{
					NodeShadow nodeChild;
					if(h_mapPtToNode.ContainsKey(tempVect3))
					{
						nodeChild = (NodeShadow)h_mapPtToNode[tempVect3];
					}
					else
					{
						nodeChild = new NodeShadow(tempVect3);
						nodeChild.setSafetyLevel(pathPointIndx);
						h_mapPtToNode.Add(tempVect3,nodeChild);
					}
					node.addChild(nodeChild);
					//Debug.Log(tempVect3+" added as child of "+node.getPos()+" Dist b/w them is "+Vector3.Distance(node.getPos(),tempVect3));
					return true;
				}
				else
				{
					//Debug.Log(tempVect3+" cannot be added as child of "+node.getPos()+" Dist b/w them is "+Vector3.Distance(node.getPos(),tempVect3));
				}
			}
			else
			{
				if(pointInShadow(tempVect3,pathPointIndx) && Vector3.Distance(node.getPos(),tempVect3)<=standardMaxMovement && CheckStraightLineVisibility(node.getPos(),tempVect3))
				{
					NodeShadow nodeChild;
					if(h_mapPtToNode.ContainsKey(tempVect3))
					{
						nodeChild = (NodeShadow)h_mapPtToNode[tempVect3];
					}
					else
					{
						nodeChild = new NodeShadow(tempVect3);
						nodeChild.setSafetyLevel(pathPointIndx);
						h_mapPtToNode.Add(tempVect3,nodeChild);
					}
					node.addChild(nodeChild);
					//Debug.Log(tempVect3+" added as child of "+node.getPos()+" Dist b/w them is "+Vector3.Distance(node.getPos(),tempVect3));
					return true;
				}
				else
				{
					//Debug.Log(tempVect3+" cannot be added as child of "+node.getPos()+" Dist b/w them is "+Vector3.Distance(node.getPos(),tempVect3));
				}
			}
		}
		return false;
	}
	private bool CheckStraightLineVisibility(Vector3 pPoint,Vector3 vect)
	{
		Line longRayLine = new Line(pPoint,vect);
		List<Geometry> allGeometries = new List<Geometry>();
		allGeometries.Add (mapBG);
		allGeometries.AddRange(globalPolygon);
		//Find intersection points for longRayLine
		List<Vector3> intersectionPoints = new List<Vector3>();
		//Intersection with holes
		foreach (Geometry g in allGeometries) 
		{
			foreach(Line l in g.edges)
			{
				if(l.LineIntersectMuntacEndPt(longRayLine)!=0)
				{
					return false;
				}
			}
		}
		return true;
	}
	private void reachableChildren2(NodeShadow node,Vector2 indexOfPt,int pathPointIndx,Hashtable h_mapPtToNode)
	{
		int rowJ = (int)indexOfPt.x;
		int colK = (int)indexOfPt.y;
		addPossibleChild2(indexOfPt,node,pathPointIndx,h_mapPtToNode);
		while(true)
		{
			bool bStillReachable=false;
			bool bRunAgain=false;
			rowJ--;
			colK--;
			int rowLen = ((int)indexOfPt.x - rowJ)*2 +1;
			//////////////////////////////////////////////////////////////////////&&&&&&&&&&&&&&&&&
			Vector2 testPt2D = new Vector2(rowJ+rowLen/2,colK);
			Vector3 testPt3D = new Vector3(0,0,0);
			bool bPtAssigned = false;
			if(h_mapIndxToPt.ContainsKey(testPt2D))
			{
				testPt3D = (Vector3)h_mapIndxToPt[testPt2D];
				bPtAssigned = true;
			}
			else
			{
				testPt2D = new Vector2(rowJ+rowLen/2,colK+rowLen-1);
				if(h_mapIndxToPt.ContainsKey(testPt2D))
				{
					testPt3D = (Vector3)h_mapIndxToPt[testPt2D];
					bPtAssigned = true;
				}
				else
				{
					testPt2D = new Vector2(rowJ+rowLen-1,colK+rowLen/2);
					if(h_mapIndxToPt.ContainsKey(testPt2D))
					{
						testPt3D = (Vector3)h_mapIndxToPt[testPt2D];
						bPtAssigned = true;
					}
					else
					{
						testPt2D = new Vector2(rowJ,colK+rowLen/2);
						if(h_mapIndxToPt.ContainsKey(testPt2D))
						{
							testPt3D = (Vector3)h_mapIndxToPt[testPt2D];
							bPtAssigned = true;
						}
					}
				}
			}
			if(!bPtAssigned)
			{
				Debug.LogError("All Possible points exhausted. No outcome. Breaking from loop");
				break;
			}
			////////////////////////////////////////////////////////////////////////&&&&&&&&&&&&&&&&;
			float testDist = Vector3.Distance(node.getPos(),testPt3D);
			//Debug.Log("testDist = "+testDist);
			if(testDist > standardMaxMovement)// || rowJ<0 || colK<0 || rowJ+rowLen>discretePtsX || colK+rowLen>discretePtsZ)
				break;
			//Debug.Log("rowJ = "+rowJ);
			//Debug.Log("colK = "+colK);
			//Debug.Log("rowLen = "+rowLen);
			for(int i1=rowJ;i1<rowJ+rowLen;i1++)
			{
				Vector2 tempVect2 = new Vector2(i1,colK);
				bStillReachable = addPossibleChild2(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
				tempVect2 = new Vector2(i1,colK+rowLen-1);
				bStillReachable = addPossibleChild2(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector2 tempVect2 = new Vector2(rowJ,i2);
				bStillReachable = addPossibleChild2(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
				tempVect2 = new Vector2(rowJ+rowLen-1,i2);
				bStillReachable = addPossibleChild2(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
			}
			
			
		}
		/*Debug.Log(node.getPos()+" has following children");
		string childrEn="";
		foreach(NodeShadow ch in node.getChildren())
		{
			childrEn+=ch.getPos()+" , ";
		}
		Debug.Log(childrEn);
		*/
		/*Vector3 pt = (Vector3)h_mapIndxToPt[indexOfPt];
		Vector4 pt4 = new Vector4(pt.x,pt.y,pt.z,pathPointIndx-1);
		if(!m_hCompleteNodeTable.ContainsKey(pt4))
		{
			m_hCompleteNodeTable.Add (pt4,node);
		}
		List<NodeShadow> newChildren = new List<NodeShadow> ();
		foreach(NodeShadow childTemp in node.getChildren())
		{
			pt = childTemp.getPos();
			pt4 = new Vector4(pt.x,pt.y,pt.z,pathPointIndx);
			if(!m_hCompleteNodeTable.ContainsKey(pt4))
			{
				newChildren.Add(childTemp);
			}
		}
		return newChildren;
		*/
	}

	private int readLastNodeOutput(Hashtable h_mapPtToNode)
	{
		
		List<char> sep = new List<char>();
		sep.Add(',');
		sep.Add(' ');
		sep.Add(';');
		sep.Add('(');
		sep.Add(')');
		sep.Add('|');
		int levelOfAccess = -1;
		string sourceFileName = EditorUtility.OpenFilePanel("Please select last data node file from previous run", Application.dataPath,"");


		StreamReader sr = new StreamReader(sourceFileName);
		string str;// = sr.ReadLine();
		while(!sr.EndOfStream)
		{
			str = sr.ReadLine();
			
			string[] line1 = str.Split(sep.ToArray());
			//Debug.Log(str);
			List<string> line = new List<string>();
			for(int i=0;i<line1.Length;i++)
			{
				if(line1[i]=="")
					continue;
				line.Add(line1[i]);
				//Debug.Log(line1[i]);
			}

			//Vector4 keyObj = new Vector4(float.Parse(line[0]),float.Parse(line[1]),float.Parse(line[2]),float.Parse(line[3]));
			Vector3 keyObj = new Vector4(float.Parse(line[0]),float.Parse(line[1]),float.Parse(line[2]));
			//Vector4 parentKeyObj = new Vector4(float.Parse(line[4]),float.Parse(line[5]),float.Parse(line[6]),float.Parse(line[7]));

			NodeShadow node = null;
			if(!h_mapPtToNode.ContainsKey(keyObj))
			{
				
				node = new NodeShadow(new Vector3(keyObj.x,keyObj.y,keyObj.z));
				node.setSafetyLevel((int)float.Parse(line[3]));
				levelOfAccess = node.getSafetyLevel();
				h_mapPtToNode.Add(keyObj,node);
			}
		}
		return levelOfAccess;
	}
	private void continueExecuteTrueCase(string dirName)
	{
		float startTime = Time.realtimeSinceStartup;
		Hashtable h_mapPtToNode = new Hashtable ();
		int levelOfAccess = readLastNodeOutput(h_mapPtToNode);
		int numOfLevels = pathPoints.Count-1;
		List<NodeShadow> nodeSafeLevelNow = new List<NodeShadow> ();
		foreach(Vector3 vect in h_mapPtToNode.Keys)
		{
			nodeSafeLevelNow.Add((NodeShadow)h_mapPtToNode[vect]);
			if(h_mapPtToIndx.ContainsKey(vect))
				Debug.Log(vect+"  found in h_mapPtToIndx");
		}


		//Debug.Log ("Num of nodes continued from = " + nodeSafeLevelNow.Count);
		//Debug.Log (h_mapPtToIndx.Keys.Count);
		Vector3 v = new Vector3 (-0.7f, 1.0f, -4.0f);
		foreach(Vector3 vect in h_mapPtToIndx.Keys)
		{

			if(vect==v)
				Debug.Log(vect);
			//if(h_mapPtToNode.ContainsKey(vect))
			//	Debug.Log(vect+"  found in h_mapPtToNode");
		}


		h_mapPtToNode.Clear ();
		//h_mapPtToNode = new Hashtable();
		while(levelOfAccess<numOfLevels)//TODO:think other exit cases
		{
			levelOfAccess++;
			
			foreach(NodeShadow node in nodeSafeLevelNow)
			{
				//
				//Vector3 v = new Vector3 (-0.7f, 1.0f, -4.0f);
				//if(h_mapIndxToPt.ContainsValue(node.getPos()))
					//Debug.Log("Here"+node.getPos());

				Vector2 indexOfPtTemp = (Vector2)h_mapPtToIndx[node.getPos()];
				reachableChildren2 (node,indexOfPtTemp,levelOfAccess,h_mapPtToNode);
			}
			nodeSafeLevelNow = new List<NodeShadow> ();
			foreach(Vector3 vect in h_mapPtToNode.Keys)
			{
				nodeSafeLevelNow.Add((NodeShadow)h_mapPtToNode[vect]);
			}
			DumpEdgesForLevel(h_mapPtToNode,levelOfAccess,dirName);
			h_mapPtToNode.Clear();
			
		}
	}

	private void displayPredictedPaths3()
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////

		/// //////////////////////////////////////////////////////////////////////////////////////////////
		float startTime = Time.realtimeSinceStartup;
		int numOfLevels = lastPathIndex();//m_lastPathIndex;
		
		string sourceDirName = EditorUtility.OpenFolderPanel("Please select results dir", Application.dataPath,"");
		string resultFileName = sourceDirName+"\\Result.txt";
		StreamReader sr = new StreamReader(resultFileName);
		List<char> sep = new List<char>();
		sep.Add(',');
		sep.Add(' ');
		sep.Add(';');
		sep.Add('(');
		sep.Add(')');
		sep.Add('|');
		int numLevelsReached = -1;
		string str;

		while(!sr.EndOfStream)
		{
			str = sr.ReadLine();
			sr.ReadLine();
			string[] line1 = str.Split(sep.ToArray());
			List<string> line = new List<string>();
			for(int i=0;i<line1.Length;i++)
			{
				if(line1[i]=="")
					continue;
				line.Add(line1[i]);
			}

			Vector3 keyObj = new Vector3(float.Parse(line[0]),float.Parse(line[1]),float.Parse(line[2]));
			numLevelsReached = int.Parse(line[3]);
			float greenNum = (float)numLevelsReached/(float)numOfLevels;
			float G = (255 * numLevelsReached) / numOfLevels;
			float R = (255 * (numOfLevels - numLevelsReached)) / numOfLevels ;
			float B = 0;
			showPosOfPoint(keyObj,new Color(0.0f,greenNum,0.0f));
			//showPosOfPoint(keyObj,new Color(R,G,B));
		}
		sr.Close ();
	}
	private void testFunc()
	{
		string sourceDirName = EditorUtility.OpenFolderPanel("Please select results dir", Application.dataPath,"");



		string infoFileName = sourceDirName+"\\Info.txt";
		StreamReader sr = new StreamReader (infoFileName);
		string line = sr.ReadLine ();
		while(true)
		{
			if(line.Contains("Distance covered by the player"))
			{
				int indx = line.IndexOf(" = ");
				string numberStr = line.Substring(indx+3);
				
				m_stepDistance = float.Parse(numberStr);
				Debug.Log(m_stepDistance);
				Debug.Log("numberStr = "+numberStr);
				
				sr.Close ();
				break;
			}
			line = sr.ReadLine ();
		}
	}
	private void calculatePredictedPaths()
	{
		float startTime = Time.realtimeSinceStartup;
		List<NodeShadow> headNodes = readNodeStructureFor2 ();
		//Debug.Log ("Num of headNodes = "+headNodes.Count);
		//return;
		int numOfLevels = lastPathIndex ();//pathPoints.Count-1;//m_lastPathIndex;

		string sourceDirName = EditorUtility.OpenFolderPanel("Please select results dir", Application.dataPath,"");
		string resultFileName = sourceDirName+"\\Result.txt";
		StreamWriter sw = new StreamWriter (resultFileName);
		//////////////////Reading Info file for step distance
		string infoFileName = sourceDirName+"\\Info.txt";
		StreamReader sr = new StreamReader (infoFileName);
		string line = sr.ReadLine ();
		while(true)
		{
			if(line.Contains("Distance covered by the player"))
			{
				int indx = line.IndexOf(" = ");
				string numberStr = line.Substring(indx+3);

				m_stepDistance = float.Parse(numberStr);

				sr.Close ();
				break;
			}
			line = sr.ReadLine ();
		}
		///////////////////
		foreach(NodeShadow headNode in headNodes)
		{
			int numLevelsReached = findFurthestPathPointReached(headNode);
			sw.Write("("+headNode.getPos().x+","+headNode.getPos().y+","+headNode.getPos().z+")"+";"+numLevelsReached);
			sw.WriteLine("");
			/*float greenNum = numLevelsReached/numOfLevels;
			float redNum = 1-greenNum;
			showPosOfPoint(headNode.getPos(),new Color(redNum,greenNum,0));
			*/
		}
		sw.Close ();
	}
	List<NodeShadow> getNodesWithIndex(int endIndxTemp)
	{
		List<NodeShadow> nodeList = new List<NodeShadow> ();
		foreach(Vector4 vect in m_hCompleteNodeTable.Keys)
		{
			if(vect.w==endIndxTemp && ((NodeShadow)(m_hCompleteNodeTable[vect])).getCanReachLimit()<0)
			{
				nodeList.Add((NodeShadow)m_hCompleteNodeTable[vect]);
			}
		}
		return nodeList;
	}
	void setCanReachLimit(List<NodeShadow> nodeList,int endIndxTemp)
	{
		foreach(NodeShadow node in nodeList)
		{
			node.setCanReachLimit(endIndxTemp);
		}
	}
	List<NodeShadow> getParentList(List<NodeShadow> nodeList,float radiusMovement)
	{
		List<NodeShadow> nodeParentList = new List<NodeShadow> ();
		foreach (NodeShadow node in nodeList) 
		{
			List<NodeShadow> parentList = node.getParent();
			foreach (NodeShadow parentNode in parentList) 
			{
				if(!nodeParentList.Contains(parentNode) && parentNode.getCanReachLimit()<0 && Vector3.Distance(parentNode.getPos(),node.getPos())<=radiusMovement)
					nodeParentList.Add(parentNode);
			}
		}
		return nodeParentList;
	}
	private void calculatePredictedPathsNew()
	{
		float startTime = Time.realtimeSinceStartup;
		List<NodeShadow> headNodes = readNodeStructureFor2 ();
		//m_EndIndx
		int endIndxTemp = m_EndIndx;

		int numOfLevels = lastPathIndex ();//pathPoints.Count-1;//m_lastPathIndex;
		
		string sourceDirName = EditorUtility.OpenFolderPanel("Please select results dir", Application.dataPath,"");
		string resultFileName = sourceDirName+"\\Result.txt";
		StreamWriter sw = new StreamWriter (resultFileName);
		//////////////////Reading Info file for step distance
		string infoFileName = sourceDirName+"\\Info.txt";
		StreamReader sr = new StreamReader (infoFileName);
		string line = sr.ReadLine ();
		while(true)
		{
			if(line.Contains("Distance covered by the player"))
			{
				int indx = line.IndexOf(" = ");
				string numberStr = line.Substring(indx+3);
				
				m_stepDistance = float.Parse(numberStr);
				
				sr.Close ();
				break;
			}
			line = sr.ReadLine ();
		}
		float timePlayer = m_stepDistance/speedPlayer;
		float radiusMovement = speedEnemy*timePlayer;
		while(endIndxTemp>0)
		{
			List<NodeShadow> nodeList = getNodesWithIndex(endIndxTemp);
			setCanReachLimit(nodeList,endIndxTemp);
			List<NodeShadow> nodeParentList = getParentList(nodeList,radiusMovement);
			while(nodeParentList.Count!=0)
			{
				setCanReachLimit(nodeParentList,endIndxTemp);
				nodeParentList = getParentList(nodeParentList,radiusMovement);
			}
			endIndxTemp--;
		}
		///////////////////
		foreach(NodeShadow headNode in headNodes)
		{
			int numLevelsReached = headNode.getCanReachLimit();//findFurthestPathPointReached(headNode);
			if(numLevelsReached<0)
				numLevelsReached=0;
			sw.Write("("+headNode.getPos().x+","+headNode.getPos().y+","+headNode.getPos().z+")"+";"+numLevelsReached);
			sw.WriteLine("");
			/*float greenNum = numLevelsReached/numOfLevels;
			float redNum = 1-greenNum;
			showPosOfPoint(headNode.getPos(),new Color(redNum,greenNum,0));
			*/
		}
		sw.Close ();
	}

	int m_EndIndx = -1;
	private List<NodeShadow> readNodeStructureFor2()
	{
		List<NodeShadow> headNodes = new List<NodeShadow> ();
		setGlobalVars1 ();
		string sourceDirName = EditorUtility.OpenFolderPanel("Please select data node dir", Application.dataPath,"");

		List<char> sep = new List<char>();
		sep.Add(',');
		sep.Add(' ');
		sep.Add(';');
		sep.Add('(');
		sep.Add(')');
		sep.Add('|');
		int levelOfAccess = 1;
		string sourceFileName = sourceDirName+"\\Edges"+levelOfAccess+".txt";
		FileInfo fInfo = new FileInfo(sourceFileName);
		if(!fInfo.Exists)
			return headNodes;
		StreamReader sr = new StreamReader(sourceFileName);
		string str;// = sr.ReadLine();
		while(true)
		{
			//Debug.Log("Reading "+sourceFileName);
			////////////////////////////////////////////////////////////////////////////////////////
			while(!sr.EndOfStream /*&& jk>0*/)
			{
				str = sr.ReadLine();

				string[] line1 = str.Split(sep.ToArray());
				//Debug.Log(str);
				List<string> line = new List<string>();
				for(int i=0;i<line1.Length;i++)
				{
					if(line1[i]=="")
						continue;
					line.Add(line1[i]);
					//Debug.Log(line1[i]);
				}
				
				Vector4 parentKeyObj = new Vector4();
				Vector4 keyObj = new Vector4(float.Parse(line[0]),float.Parse(line[1]),float.Parse(line[2]),float.Parse(line[3]));

				/*if(keyObj.w==0.0f)//A head Node
				{
					NodeShadow headNode = new NodeShadow(new Vector3(keyObj.x,keyObj.y,keyObj.z));
					headNode.setSafetyLevel((int)keyObj.w);
					if(!m_hCompleteNodeTable.ContainsKey(keyObj))
					{
						m_hCompleteNodeTable.Add(keyObj,headNode);
					}
					//headNodes.Add(headNode);
					continue;
				}*/
				//else
				//{
				parentKeyObj = new Vector4(float.Parse(line[4]),float.Parse(line[5]),float.Parse(line[6]),float.Parse(line[7]));
				//}
				NodeShadow node = null;
				if(!m_hCompleteNodeTable.ContainsKey(keyObj))
				{
					
					node = new NodeShadow(new Vector3(keyObj.x,keyObj.y,keyObj.z));
					node.setSafetyLevel((int)keyObj.w);
					m_hCompleteNodeTable.Add(keyObj,node);
					if(m_EndIndx<node.getSafetyLevel())
					{
						m_EndIndx = node.getSafetyLevel();
					}
				}
				else
				{
					node = (NodeShadow)m_hCompleteNodeTable[keyObj];

				}

				NodeShadow parentNode = null;
				if(!m_hCompleteNodeTable.ContainsKey(parentKeyObj))
				{
					parentNode = new NodeShadow(new Vector3(parentKeyObj.x,parentKeyObj.y,parentKeyObj.z));
					parentNode.setSafetyLevel((int)parentKeyObj.w);
					m_hCompleteNodeTable.Add(parentKeyObj,parentNode);
				}
				else
				{
					parentNode = (NodeShadow)m_hCompleteNodeTable[parentKeyObj];
				}
				
				parentNode.addChild(node);
				if(levelOfAccess==1)
				{
					headNodes.Add(parentNode);
				}
				
				
			}
			//Debug.Log ("Number of nodes are  = " + m_hCompleteNodeTable.Keys.Count);
			sr.Close ();
			///////////////////////////////////////////////////////////////////////////////////////;
			levelOfAccess++;
			sourceFileName = sourceDirName+"\\Edges"+levelOfAccess+".txt";
			fInfo = new FileInfo(sourceFileName);
			if(!fInfo.Exists)
				break;
			sr = new StreamReader(sourceFileName);
			/////////Check for empty file
			str = sr.ReadLine();
			if(str=="")
			{
				break;
			}
			sr.Close ();
			sr = new StreamReader(sourceFileName);
			/////////Check for empty file;
		}
		return headNodes;
	}


	private int findFurthestPathPointReached (NodeShadow headNode)
	{
		int lastIndex = lastPathIndex();//m_lastPathIndex;
		int maxIndex = 0;

		List<NodeShadow> traversedNodes = new List<NodeShadow> ();
		List<NodeShadow> stack = new List<NodeShadow> ();
		stack.Add (headNode);
		int topIndex = -1;
		float timePlayer = m_stepDistance/speedPlayer;
		float radiusMovement = speedEnemy*timePlayer;

		while(stack.Count>0)
		{
			//pop the top
			topIndex = stack.Count-1;
			NodeShadow nodeTop = stack[topIndex];
			stack.RemoveAt(topIndex);
			if(traversedNodes.Contains(nodeTop))
			{
				continue;
			}
			else
			{
				traversedNodes.Add(nodeTop);
			}
			if(nodeTop.getSafetyLevel()>maxIndex)
			{
				maxIndex = nodeTop.getSafetyLevel();
			}
			//stack.AddRange(nodeTop.getChildren());
			//Add children depending on speed
			foreach(NodeShadow child in nodeTop.getChildren())
			{
				if(Vector3.Distance(child.getPos(),nodeTop.getPos())<=radiusMovement)
					stack.Add(child);
			}
			/////////////////////////////////
			if(maxIndex==lastIndex)
				break;


			
		}
		return maxIndex;
	}
}
	#endif