using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour {

	/*private static Dictionary<int, UnityEngine.Object[]> a = new Dictionary<int, UnityEngine.Object[]>(){
		{(int)ShipType.Default, Resources.LoadAll("Ships/DefaultShip")},
	};*/
	public static Ship OwnedShip;

	public int ship_id;
	public GameObject instance;
	public ShipNetworkController networkController;

	public string type = "DefaultShip";

	public Vector3 position;
	public Quaternion rotation;
	public Vector3 velocity = new Vector3();
	public Vector3 rotvelocity = new Vector3();

	public Ship(int _ship_id, string _type, Vector3 pos, Quaternion rot, bool local, Vector3? vel, Vector3? rotvel){
		if (local && OwnedShip!=null){Debug.LogWarning("Tried to create a local ship while an owned one is still available, prev ship is removed"); OwnedShip.Remove();}


		ship_id = _ship_id;
		position = pos;
		rotation = rot;
		if (vel!=null){velocity=(Vector3)vel;}
		if (rotvel!=null){rotvelocity=(Vector3)rotvel;}

		type = _type;

		if (local){
			instance = Instantiate(Resources.Load($"Ships/{type}/LocalShip", typeof(GameObject)) as GameObject, pos, rot);
		}else{
			instance = Instantiate(Resources.Load($"Ships/{type}/OthersShip", typeof(GameObject)) as GameObject, pos, rot);
		}

		networkController = instance.GetComponent<ShipNetworkController>();
		networkController.ship = this;
		
		
		AddShipsDict(this);
	}

	public void ShipSendPhysicsInfo(){
		using (Packet _packet = new Packet((int)ClientPackets.ShipPhysicsUpdate)){
			_packet.Write(OwnedShip.position);
			_packet.Write(OwnedShip.rotation);
			_packet.Write(OwnedShip.velocity);
			_packet.Write(OwnedShip.rotvelocity);

			ClientSend.SendUDPData(_packet);
		}
	}

	public void Remove(){
		Destroy(instance);
		RemoveShipsDict(ship_id);
	}


	//Statics

	public static Dictionary<int,Ship> Ships = new Dictionary<int, Ship>(); //ship_id = ship

	public static Ship GetShipFromId(int ship_id){
		return Ships[ship_id];
	}
	
	private static void AddShipsDict(Ship ship){
		Ships[ship.ship_id] = ship;
	}
	private static void RemoveShipsDict(int ship_id){
		Ships[ship_id] = null;
	}
	private static void RemoveShipsDict(Ship ship){
		Ships[ship.ship_id] = null;
	}

	private static Vector3 nilv = Vector3.zero;

	public static Ship TryGetOrCreate(int ship_id, string type, Vector3 pos, Quaternion rot, Vector3? vel, Vector3? rotvel){
		Ship ship;
		if(Ships.TryGetValue(ship_id, out ship)==false){
			if (vel==null){vel=nilv;}
			if (rotvel==null){rotvel=nilv;}
			ship = new Ship(ship_id, type, pos, rot, false, vel, rotvel);
		}
		return ship;
	}
	
	
	public static void OwnedShipSendPhysicsInfo(){
		if (OwnedShip==null){return;} 
		OwnedShip.ShipSendPhysicsInfo();
	}
}
