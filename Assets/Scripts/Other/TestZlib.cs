using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.IO.Compression;

using NetOpt;

public class TestZlib : MonoBehaviour
{
    //private static int maxsize = 100*1024*1024;
    void Start()
    {
        /*using (Packet p = new Packet()){
            for (int i = 0; i < 100; i++)
            {
                p.Write((int)68);
            }

            //NetOpt.Compressor.Compress()
            var buffer = new PacketBuffer(p.ToArray());
            var compd = Compressor.Compress(buffer,100,Compressor.Algorithm.ZSTDF);

            compd = Compressor.Decompress(compd,maxsize,Compressor.Algorithm.ZSTDF);

            byte[] bv = compd.GetBytes(true);

            Debug.Log(bv);
            Debug.Log(bv[0]);
            Debug.Log(bv[1]);
            Debug.Log(bv[2]);
            Debug.Log(bv[3]);

            //Unity.IO.Compression.DeflateInput.
            
            /*p.Compress();
            p.Decompress();
            p.Reset(false);
            int a = p.ReadInt();
            Debug.Log(a);*/
        //}
    }
}
