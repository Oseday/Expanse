using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship {

	/*private static Dictionary<int, UnityEngine.Object[]> a = new Dictionary<int, UnityEngine.Object[]>(){
		{(int)ShipType.Default, Resources.LoadAll("Ships/DefaultShip")},
	};*/

	public int owner_id = -1;

	public static Ship OwnedShip;

	public int ship_id;
	public GameObject instance;
	public ShipNetworkController networkController;

	public string type = "DefaultShip";

	public Vector3 position;
	public Quaternion rotation;
	public Vector3 velocity = new Vector3();
	public Vector3 rotvelocity = new Vector3();

	public Ship(int _ship_id, int? _owner_id, string _type, Vector3 pos, Quaternion rot, bool local, Vector3? vel, Vector3? rotvel){
		if (local && OwnedShip!=null){Debug.LogWarning("Tried to create a local ship while an owned one is still available, prev ship is removed"); OwnedShip.Remove();}


		ship_id = _ship_id;
		if (_owner_id!=null){owner_id=(int)_owner_id;}

		position = pos;
		rotation = rot;
		if (vel!=null){velocity=(Vector3)vel;}
		if (rotvel!=null){rotvelocity=(Vector3)rotvel;}

		type = _type;

		if (local){
			OwnedShip = this;
			instance = GameObject.Instantiate(Resources.Load($"Ships/{type}/LocalShip", typeof(GameObject)) as GameObject, pos, rot);
		}else{
			instance = GameObject.Instantiate(Resources.Load($"Ships/{type}/OthersShip", typeof(GameObject)) as GameObject, pos, rot);
		}

		networkController = instance.GetComponent<ShipNetworkController>();
		networkController.ship = this;
		
		Debug.Log($"spawned new ship, id:{ship_id} , ownerid:{owner_id}");

		AddShipsDict(this);
	}

	public void ShipSendPhysicsInfo(){
		using (Packet _packet = new Packet((int)ClientPackets.ShipPhysicsUpdate)){
			Rigidbody body = this.networkController.body;
			if (body==null){return;}

			_packet.Write(body.position);
			_packet.Write(body.rotation);
			_packet.Write(body.velocity);
			_packet.Write(body.angularVelocity);

			ClientSend.SendUDPData(_packet);
		}
	}

	public void Remove(){
		GameObject.Destroy(instance);
		RemoveShipsDict(ship_id);
	}


	//Statics

	public static Dictionary<int,Ship> Ships = new Dictionary<int, Ship>(); //ship_id = ship

	public static Ship GetShipFromId(int ship_id){
		Ship ship;
		Ships.TryGetValue(ship_id, out ship);
		return ship;
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
			ship = new Ship(ship_id, null, type, pos, rot, false, vel, rotvel);
		}
		return ship;
	}
	
	
	public static void OwnedShipSendPhysicsInfo(){
		if (OwnedShip==null){return;} 
		OwnedShip.ShipSendPhysicsInfo();
	}

	public struct ship_info{
		public int ship_id;
		public int owner_id;
		public short type;
		public Vector3 pos;
		public Quaternion rot;
		public Vector3 vel;
		public Vector3 rotvel;
	}

	public static ship_info ExtractShipInfo(Packet p) {
		ship_info i;
		i.ship_id = p.ReadInt();
		i.owner_id = p.ReadInt();
		i.type = (short)p.ReadInt();
		i.pos = p.ReadVector3();
		i.rot = p.ReadQuaternion();
		i.vel = p.ReadVector3();
		i.rotvel = p.ReadVector3();
		return i;
	}

	public static void SpawnShip(ship_info i){
		bool local = i.owner_id==Client.instance.myId;
		new Ship(i.ship_id, i.owner_id, Constants.ShipTypeDict[i.type], i.pos, i.rot, local, i.vel, i.rotvel);
	}
	public static void SpawnShip(Packet p) {
		SpawnShip(ExtractShipInfo(p));
	}

	public static void DespawnShip(Packet p) {
		int ship_id = p.ReadInt();
		Ship ship;
		if (Ships.TryGetValue(ship_id, out ship)){
			ship.Remove();
		}
	}
	
	public static void AllShipInfo(Packet p) {
		int length = p.ReadInt();

		ship_info info;
		Ship ship;

		for (int i = 0; i < length; i++)
		{
			info = ExtractShipInfo(p);
			if (info.owner_id!=Client.instance.myId && !Ships.TryGetValue(info.ship_id, out ship)){
				SpawnShip(info);
			}
		}
	}
}
