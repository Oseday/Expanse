using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[ExecuteAlways]
public class LaserProcessorIndirect : MonoBehaviour
{
	private static LaserProcessorIndirect instance;

	private static int sflt = sizeof(float);

	struct Laser {
		public Vector3 start;
		public Vector3 end;
		public float width;
		public float random;
		public float startTime;
		public Laser(Vector3 _start, Vector3 _end, float _width){
			start = _start;
			end = _end;
			width = _width;
			random = UnityEngine.Random.value;
			startTime = Time.time;
		}
	};

	private int GetLaserSize(){return sflt*3*2 + sflt*3;}

	private Bounds bounds = new Bounds(Vector3.zero,Vector3.one*999999f);

	private ComputeBuffer LasersBuffer;
	private ComputeBuffer argsBuffer;

	private List<Laser> Lasers = new List<Laser>{};

	public Material defaultMaterial;

	public float defaultWidth;

	
	//private List<Laser> Lasers = new List<Laser>{}; //Maybe use lists?

	Mesh defaultMesh;
	
	private uint[] args = new uint[5]{0,0,0,0,0};
	private bool _updateSendArgsBuffer = false;

	private void CreateArgsBuffer() {
		/*args = new uint[5]{
			(uint)defaultMesh.GetIndexCount(0),
			(uint)population,
			(uint)defaultMesh.GetIndexStart(0),
			(uint)defaultMesh.GetBaseVertex(0), 0
		};*/
		args[0] = (uint)defaultMesh.GetIndexCount(0);
		args[1] = (uint)population;
		args[2] = (uint)defaultMesh.GetIndexStart(0);
		args[3] = (uint)defaultMesh.GetBaseVertex(0);

		if (argsBuffer!=null && argsBuffer.IsValid()) {argsBuffer.Dispose();}

		argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
		_updateSendArgsBuffer = true;
	}
	
	private int population;
	private int oldPopulation;
	private void IncreasePopulation() {
		SetPopulation(population+1);
	}
	private void SetPopulation(int _pop) {
		population=_pop;
		/*Laser[] nLasers = new Laser[population]; //Maybe use lists here?
		//Array nLasers = Array.CreateInstance(typeof(Laser), population);
		//nLasers = (Laser[])Lasers.Clone();
		if (Lasers!=null){
			Lasers.CopyTo(nLasers,0);
		}
		Lasers = nLasers;*/

		if (LasersBuffer==null){
			CreateLasersBuffer();
		}
	}

	private void UpdateArgsBuffer() {
		if (args==null){CreateArgsBuffer();}
		args[1] = (uint)population;
		_updateSendArgsBuffer = true;
	}

	private bool _updateSendLasersBuffer = false;
	private void CreateLasersBuffer() {
		if (LasersBuffer!=null && LasersBuffer.IsValid()){ LasersBuffer.Dispose();}
		LasersBuffer = new ComputeBuffer(population, GetLaserSize(), ComputeBufferType.Structured);
		//var a = camera.Depth;
		defaultMaterial.SetBuffer("_Lasers", LasersBuffer);
		_updateSendLasersBuffer = true;
	}

	private void CopyDataToGPU_LasersBuffer() {
		LasersBuffer.SetData(Lasers);
	}
	private void CopyDataToGPU_ArgsBuffer() {
		argsBuffer.SetData(args);
	}

	private void CreateDefaultMesh(){
		defaultMesh = new Mesh();
		defaultMesh.SetVertices(new List<Vector3>{
			new Vector3(0,1),
			new Vector3(1,1),
			new Vector3(0,0),
			new Vector3(1,0),
		});
		defaultMesh.SetUVs(0, new List<Vector2>{
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(0,0),
			new Vector2(1,0),
		});
		defaultMesh.SetTriangles(new List<int>{
			0,1,2,2,1,3
		},0);
	}

	private new Camera camera;
	private void Awake() {
		instance = this;
		camera = gameObject.GetComponent<Camera>();
		camera.depthTextureMode = DepthTextureMode.Depth;

		CreateDefaultMesh();

		AddLaser(UnityEngine.Random.insideUnitSphere*4f, UnityEngine.Random.insideUnitSphere*4f, null);

		CreateArgsBuffer();
		//CreateLasersBuffer();

		for (int i = 0; i < 10; i++)
		{
			AddLaser(UnityEngine.Random.insideUnitSphere*4f, UnityEngine.Random.insideUnitSphere*4f, null);
		}
	}


	private void Update() {

		if (oldPopulation!=population){
			SetPopulation(population);
			oldPopulation=population;
		}

		if (_updateSendArgsBuffer) {
			CopyDataToGPU_ArgsBuffer();
			_updateSendArgsBuffer = false;
		}

		if (_updateSendLasersBuffer) {
			CreateLasersBuffer();
			CopyDataToGPU_LasersBuffer();
			_updateSendLasersBuffer = false;
		}

		if (argsBuffer!=null){
			Graphics.DrawMeshInstancedIndirect(defaultMesh, 0, defaultMaterial, bounds, argsBuffer);
		}
	}

	private void _AddLaser(Vector3 start, Vector3 end, float? width){
		//Lasers.Add(new Laser(start, end, width.HasValue ? width.Value : defaultWidth));
		SetPopulation(population+1);
		Lasers.Add(new Laser(start, end, width.HasValue ? width.Value : defaultWidth));
		/*Lasers[population-1] = new Laser();//start, end, width.HasValue ? width.Value : defaultWidth)
		Lasers[population-1].start = start;
		Lasers[population-1].end = end;
		Lasers[population-1].width = width.HasValue ? width.Value : defaultWidth;
		Lasers[population-1].random = UnityEngine.Random.value;
		Lasers[population-1].startTime = Time.time;*/
		UpdateArgsBuffer();
		_updateSendLasersBuffer = true;
	}

	public static void AddLaser(Vector3 start, Vector3 end, float? width){instance._AddLaser(start, end, width.HasValue ? width.Value : 0.05f);}

	private void OnApplicationQuit() {OnDestroy();}
	private void OnDestroy() {
		if (argsBuffer!=null){
			argsBuffer.Dispose();
		}
		if (LasersBuffer!=null){
			LasersBuffer.Dispose();
		}
	}
}

//using UnityEngine;
//using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(LaserProcessorIndirect))]
public class LaserProcessorEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		//ObjectBuilderScript myScript = (ObjectBuilderScript)target;
		if(GUILayout.Button("Create Random Laser"))
		{
			LaserProcessorIndirect.AddLaser(UnityEngine.Random.insideUnitSphere*4f, UnityEngine.Random.insideUnitSphere*4f, null);
			//myScript.BuildObject();
		}
	}
}

#endif