using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    public static void SendTCPData(Packet _packet)//does NOT insert int at top by default
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    public static void SendUDPData(Packet _packet) //inserts int at top by default
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }
}
