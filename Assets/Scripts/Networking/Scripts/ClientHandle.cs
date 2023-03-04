using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{

	public static void Welcome(Packet _packet)
	{
		string _msg = _packet.ReadString();
		int _myId = _packet.ReadInt();

		Debug.Log($"Message from server: {_msg}");
		Client.instance.myId = _myId;

		using (Packet _wdpacket = new Packet((int)ClientPackets.welcomeReceived)){
			_wdpacket.Write(Client.instance.myId);
			_wdpacket.Write(UIManager.instance.usernameField.text);

			ClientSend.SendTCPData(_wdpacket);
		}

		Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
	}

	public static void PlayerPosVelUpdate(Transform transform, Rigidbody body){
		using (Packet _packet = new Packet((int)ClientPackets.ShipPhysicsUpdate)){
			_packet.Write(transform.position);
			_packet.Write(transform.rotation);
			_packet.Write(body.velocity);
			_packet.Write(body.angularVelocity);

			ClientSend.SendUDPData(_packet);
		}
	}

	public static void DespawnPlayer(Packet _packet){
		int _id = _packet.ReadInt();
		GameNetworkManager.instance.DespawnPlayer(_id);
	}

	public static void SpawnPlayer(Packet _packet)
	{
		int _id = _packet.ReadInt();
		string _username = _packet.ReadString();
		Vector3 _position = _packet.ReadVector3();
		Quaternion _rotation = _packet.ReadQuaternion();

		GameNetworkManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
	}

	private static long LastNetTickUpdate = 0;

	public static void PhysicsTick(Packet _packet)
	{
		//_packet.Decompress();

		long times = _packet.ReadLong(); //don't use UDP signals from the past

		//Debug.Log(times);

		//Debug.Log(times-LastNetTickUpdate);
		if (times<LastNetTickUpdate){return;}
		float ticker = (times-LastNetTickUpdate)/10000000f;
		LastNetTickUpdate = times;


		//LastNetTickUpdate = Time.realtimeSinceStartup;

		int length = _packet.ReadInt();

		//Debug.Log(length);

		float dt = 1f/GameNetworkManager.instance.PhysicsServerTickTime;

		Dictionary<int, Ship> ShipsDict = new Dictionary<int,Ship>(length);
		
		for (int i = 0; i < length; i++)
		{
			int ship_id = _packet.ReadInt(); //ship type could be inferred from ship_id
			int owner_id = _packet.ReadInt();
			Vector3 pos = _packet.ReadVector3();
			Quaternion rot = _packet.ReadQuaternion();
			Vector3 vel = _packet.ReadVector3();
			Vector3 rotvel = _packet.ReadVector3();
			
			//Debug.Log($"packet id:{ship_id}, oid.{owner_id}");

			if (owner_id!=Client.instance.myId){
				Ship ship; //Ship.TryGetOrCreate(ship_id, "DefaultShip", pos, rot, vel, rotvel);
				if(Ship.Ships.TryGetValue(ship_id, out ship)){
					ship.networkController.NetUpdate(pos,vel,rot,rotvel,ticker);

					ShipsDict[ship_id]=ship;
				}
			} else {
				Ship ship;
				if (Ship.Ships.TryGetValue(ship_id, out ship)){
					ShipsDict[ship_id]=ship;
				}
			}

		}

		List<int> toRemove = new List<int>();

		foreach (var ship_id in Ship.Ships.Keys)
		{
			//Debug.Log(ship_id);
			if (!ShipsDict.ContainsKey(ship_id) || ShipsDict[ship_id]==null){//(ShipsDict[ship_id]==false){
				//Ship.Ships[ship_id].Remove();
				toRemove.Add(ship_id);
			}
		}

		foreach (var ship_id in toRemove.ToArray())
		{
			if (ship_id!=0){
				Ship ship;
				if (Ship.Ships.TryGetValue(ship_id, out ship) && ship!=null){
					ship.Remove();
				}
			}
		}
	}
}
