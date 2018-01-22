using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://www.habrador.com/tutorials/unity-boat-tutorial/4-waves-endless-ocean/

public class Sea : MonoBehaviour {


	//One water square
	public GameObject waterSqrObj;

	//Water square data
	public float squareWidth = 1000f;
	public int innerSquareResolution = 20;
	public int outerSquareResolution = 5;
	MeshFilter mf ;
	MeshCollider mc;

	public float waterUpdateTime = 0.3f, waveSpeed = 1, waveScale = 1, waveCount = 10, 
	noiseStrength = 0.1f, noiseWalk = 0.1f;
	public float waterUpdateTimer = 0, t = 0, t2 = 0;
	Vector3 correctionVector, prevPos;
	MeshFilter[] additionalSquares;
	public float seaStrength = 1;

	void Awake () {
		mf = gameObject.AddComponent<MeshFilter>();
		if (t > 1) t = Mathf.Repeat(0,1);
		else {if (t<0) t*=-1; t = Mathf.Repeat(0,1);}
		if (t2 > 1) t2 = Mathf.Repeat(0,1);
		else {if (t2 <0) t2*=-1; t2 = Mathf.Repeat(0,1);}

		Mesh waterMesh= GenerateMesh(innerSquareResolution+1, squareWidth / innerSquareResolution );
		waterMesh.name = "seaGrid_main";
		if (mf.mesh != null) mf.mesh.Clear();
		mf.mesh = waterMesh;
		mc = gameObject.GetComponent<MeshCollider>();
		mc.sharedMesh = waterMesh;
		correctionVector = new Vector3(-0.5f,0,-0.5f) * squareWidth;
		prevPos = transform.position;

		additionalSquares = new MeshFilter[8];
		Material myWaterMaterial = gameObject.GetComponent<MeshRenderer>().material;
		for (int k =0; k < 8; k++) {
			GameObject g = new GameObject("additionalSquare");
			additionalSquares[k] = g.AddComponent<MeshFilter>();
			additionalSquares[k].mesh = GenerateMesh(outerSquareResolution+1,squareWidth / outerSquareResolution);
			MeshRenderer mr =  g.AddComponent<MeshRenderer>();
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.material = myWaterMaterial;
			g.transform.parent = transform;
			switch (k) {
			case 0: g.transform.localPosition = new Vector3(-1, 0, 1) * squareWidth; break;
			case 1: g.transform.localPosition = new Vector3(0, 0, 1) * squareWidth; break;
			case 2: g.transform.localPosition = new Vector3(1, 0, 1) * squareWidth; break;
			case 3: g.transform.localPosition = new Vector3(-1, 0, 0) * squareWidth; break;
			case 4: g.transform.localPosition = new Vector3(1, 0, 0) * squareWidth; break;
			case 5: g.transform.localPosition = new Vector3(-1, 0, -1) * squareWidth; break;
			case 6: g.transform.localPosition = new Vector3(0, 0, -1) * squareWidth; break;
			case 7: g.transform.localPosition = new Vector3(1, 0, -1) * squareWidth; break;
			}
			g.transform.Translate(Vector3.down * 0.1f);
			g.SetActive(false);
		}

		GenerateWave();
	}

	void Update() {
		GameMaster.seaStrength = seaStrength;
		if (GameMaster.isPaused()) return;
		t += Time.deltaTime * waveSpeed;
		if (t > 1) t= Mathf.Repeat(0,1);
		t2 += Time.deltaTime * waveSpeed;
		if (t2 > 1) t2= Mathf.Repeat(0,1);
		waterUpdateTimer -= Time.deltaTime;
		if (waterUpdateTimer <=0) GenerateWave();

	}


	void GenerateWave() {
		Vector3[] nv = mf.mesh.vertices;
		Vector3 deltaPos = prevPos - transform.position;
		prevPos = transform.position;
		Vector3 movementCorrection = deltaPos / squareWidth;
		float p = 0;
		for (int i = 0; i <innerSquareResolution; i++) {
			for (int j = 0; j < innerSquareResolution; j++)
			{
				p = j; p /= innerSquareResolution; 
				nv[ i * innerSquareResolution + j].y = Mathf.Sin((t+p) * waveCount*Mathf.PI * 2) * waveScale;
				//nv[ i * innerSquareResolution + j].y += Mathf.PerlinNoise(nv[ i * innerSquareResolution + j].x +movementCorrection.z + noiseWalk, nv[ i * innerSquareResolution + j].y + Mathf.Sin((t+movementCorrection.x) * 0.2f * Mathf.PI) + movementCorrection.z) * noiseStrength;
			}
		}
		mf.mesh.vertices = nv;
		mf.mesh.RecalculateNormals();
		mc.sharedMesh = mf.mesh;

		waterUpdateTimer = waterUpdateTime;
	}

	public Vector3 SinXWave(Vector3 position, float t) 
	{
		//Using only x or z will produce straight waves
		//Using only y will produce an up/down movement
		//x + y + z rolling waves
		//x * z produces a moving sea without rolling waves
		t = Mathf.Repeat(0,1);
		float waveType = position.z;

		position.y = Mathf.Sin((t * waveSpeed * waveCount + waveType)* Mathf.PI * 2) * waveScale;

		//Add noise to make it more realistic
		position.y += Mathf.PerlinNoise(position.x + noiseWalk, position.y + Mathf.Sin(t * 0.2f * Mathf.PI)) * noiseStrength;
		return position;
	}

	public Mesh GenerateMesh(int width, float spacing)
	{
		List<Vector3[]> verts = new List<Vector3[]>();
		List<int> tris = new List<int>();
		List<Vector2> uvs = new List<Vector2>();

		for (int z = 0; z < width; z++)
		{

			verts.Add(new Vector3[width]);

			for (int x = 0; x < width; x++)
			{
				Vector3 current_point = new Vector3();

				//Get the corrdinates of the vertices
				current_point.x = x * spacing;
				current_point.z = z * spacing;
				current_point.y = 0;

				verts[z][x] = current_point;
				uvs.Add(new Vector2(x,z));

				//Don't generate a triangle the first coordinate on each row
				//Because that's just one point
				if (x <= 0 || z <= 0)
				{
					continue;
				}

				//Each square consists of 2 triangles

				//The triangle south-west of the vertice
				tris.Add(x 		+ z * width);
				tris.Add(x 		+ (z-1) * width);
				tris.Add((x-1) 	+ (z-1) * width);

				//The triangle west-south of the vertice
				tris.Add(x 		+ z * width);
				tris.Add((x-1) 	+ (z-1) * width);
				tris.Add((x-1)	+ z * width);
			}
		}

		//Unfold the 2d array of verticies into a 1d array.
		Vector3[] unfolded_verts = new Vector3[width * width];

		int i = 0;
		foreach (Vector3[] v in verts)
		{
			//Copies all the elements of the current 1D-array to the specified 1D-array
			v.CopyTo(unfolded_verts, i * width);

			i++;
		}

		//Generate the mesh object
		Mesh newMesh = new Mesh();
		newMesh.MarkDynamic();
		newMesh.vertices = unfolded_verts;
		newMesh.uv = uvs.ToArray();
		newMesh.triangles = tris.ToArray();

		//Ensure the bounding volume is correct
		newMesh.RecalculateBounds();
		//Update the normals to reflect the change
		newMesh.RecalculateNormals();
		return newMesh;
	}

}
