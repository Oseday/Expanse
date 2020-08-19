using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ShipNetworkController : MonoBehaviour
{

	public enum NetworkInterpolationMethod
	{
		Linear=1,
		Hermite,
		CatmullRom,
		CatmullRomContinuous,
		CatmullRomCntsExtrapolated,
	}

	private Rigidbody body;


	public bool Local;

	public NetworkInterpolationMethod InterpolationMethod = NetworkInterpolationMethod.Linear;
	
	[HideInInspector]public Ship ship;

	[HideInInspector]public Vector3 StartPos;
	[HideInInspector]public Vector3 EndPos;
	[HideInInspector]public Vector3 StartVel;
	[HideInInspector]public Vector3 EndVel;

	[HideInInspector]public Quaternion StartRot;
	[HideInInspector]public Quaternion EndRot;
	[HideInInspector]public Vector3 RotVel;


	[HideInInspector]public float Ticker = 0;
	[HideInInspector]public float TickEnd = 0;
	[HideInInspector]public float cTime = 0;

	//[HideInInspector]public string username = "";

	private void Awake() {
		gameObject.TryGetComponent<Rigidbody>(out body);
	}

	public void SetTickerZero(){
		Ticker = 0;
	}

	//rnc.NetUpdate(pos,vel,rot,rotvel,ticker);

	[HideInInspector]public Vector3 velocity;
	[HideInInspector]public Vector3 netacc;

	public void NetUpdate(Vector3 pos, Vector3 vel, Quaternion rot, Vector3 rotvel, float ticker){
		//Debug.Log("netupdate");

		if (InterpolationMethod==NetworkInterpolationMethod.Linear){
			StartPos = gameObject.transform.position; 
			StartVel = pos-StartPos;
			cTime = Time.realtimeSinceStartup;
		}else if(InterpolationMethod==NetworkInterpolationMethod.Hermite){
			StartPos = gameObject.transform.position; 
			StartVel = velocity;
			EndPos = pos;
			EndVel = vel;

			cTime = Time.realtimeSinceStartup;
		}else if(InterpolationMethod==NetworkInterpolationMethod.CatmullRom){
			StartPos = StartVel;
			StartVel = EndPos;
			EndPos = EndVel;
			EndVel = pos + vel/GameNetworkManager.instance.PhysicsServerTickTime;

			cTime = Time.realtimeSinceStartup;
		}else if(InterpolationMethod==NetworkInterpolationMethod.CatmullRomContinuous){
			StartPos = gameObject.transform.position-velocity/GameNetworkManager.instance.PhysicsServerTickTime;
			StartVel = gameObject.transform.position;
			EndPos = pos;
			EndVel = pos + vel/GameNetworkManager.instance.PhysicsServerTickTime;

			cTime = Time.realtimeSinceStartup;
		}else if(InterpolationMethod==NetworkInterpolationMethod.CatmullRomCntsExtrapolated){
			StartPos = gameObject.transform.position-velocity/GameNetworkManager.instance.PhysicsServerTickTime;
			StartVel = gameObject.transform.position;
			EndPos = pos;
			EndVel = pos + vel/GameNetworkManager.instance.PhysicsServerTickTime;

			cTime = Time.realtimeSinceStartup;
		}


		StartRot = gameObject.transform.rotation;
		EndRot = rot;
		RotVel = rotvel;

	}

	private static Vector3 nullv3 = Vector3.zero;

	private Vector3 lastPos;

	private void Update()
	{
		if (Local){
			//ClientHandle.ShipNetworkUpdate(gameObject.transform, body);
			//Ship.OwnedShipSendPhysicsInfo();
			ship.ShipSendPhysicsInfo();
		}else{

			float dt = GameNetworkManager.instance.PhysicsServerTickTime;

			Vector3 nPos = nullv3;

			float t = (Time.realtimeSinceStartup-cTime)*dt;

			if (InterpolationMethod==NetworkInterpolationMethod.Linear){
				nPos = StartPos + StartVel*t;
			}else if(InterpolationMethod==NetworkInterpolationMethod.Hermite){
				nPos = InterpolationMethods.Hermite(StartPos, EndPos, StartVel/dt, EndVel/dt, t);
			}else if(InterpolationMethod==NetworkInterpolationMethod.CatmullRom){
				nPos = InterpolationMethods.CatmullRom(StartPos, StartVel, EndPos, EndVel, Mathf.Min(t,1));
			}else if(InterpolationMethod==NetworkInterpolationMethod.CatmullRomContinuous){
				nPos = InterpolationMethods.CatmullRom(StartPos, StartVel, EndPos, EndVel, Mathf.Min(t,1));
			}else if(InterpolationMethod==NetworkInterpolationMethod.CatmullRomCntsExtrapolated){
				nPos = InterpolationMethods.CatmullRom(StartPos, StartVel, EndPos, EndVel, t);
			}

			velocity = (nPos-lastPos)/Time.unscaledDeltaTime;
			lastPos = nPos;

			gameObject.transform.position = nPos;

			gameObject.transform.rotation = Quaternion.Lerp(StartRot,EndRot,Mathf.Min(t,1));//StartRot * Quaternion.AngleAxis(RotVel.magnitude*Mathf.Min(t,1), RotVel.normalized);

			//implement rotation

		}
	}
}
