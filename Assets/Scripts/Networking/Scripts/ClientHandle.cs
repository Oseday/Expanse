﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/// <summary>Sent from server to client.</summary>
public enum ServerPackets
{
    welcome = 1,
    spawnPlayer = 2,
    PhysicsTick = 3,
    despawnPlayer = 4
}

/// <summary>Sent from client to server.</summary>
public enum ClientPackets
{
    welcomeReceived = 1,
    PlayerPosVelUpdate = 2
}

public class ClientHandle : MonoBehaviour
{
    public static Dictionary<int, Client.PacketHandler> InitPacketDict(){
        return new Dictionary<int, Client.PacketHandler>
            {
                { (int)ServerPackets.welcome, ClientHandle.Welcome },
                { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
                { (int)ServerPackets.PhysicsTick, ClientHandle.PhysicsTick },
                { (int)ServerPackets.despawnPlayer, ClientHandle.DespawnPlayer }
            };
    }

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
        using (Packet _packet = new Packet((int)ClientPackets.PlayerPosVelUpdate)){
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
        
        for (int i = 0; i < length; i++)
        {
            int pid = _packet.ReadInt();
            Vector3 pos = _packet.ReadVector3();
            Quaternion rot = _packet.ReadQuaternion();
            Vector3 vel = _packet.ReadVector3();
            Vector3 rotvel = _packet.ReadVector3();

            PlayerManager player;
            RigidbodyNetworkController rnc;

            if (!(pid==Client.instance.myId) && GameNetworkManager.players.TryGetValue(pid, out player)){
                rnc = player.GetComponent<RigidbodyNetworkController>();

                rnc.NetUpdate(pos,vel,rot,rotvel,ticker);

                rnc.username = player.username;
            }
        }
    }
}
