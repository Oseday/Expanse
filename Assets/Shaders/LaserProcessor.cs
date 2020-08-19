using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class LaserProcessor : MonoBehaviour
{
    struct Laser
    {
        public Vector3 start;
        public Vector3 end;
        public float width;
        public float startTime;
        public Laser(Vector3 _start, Vector3 _end, float _width){
            start=_start;
            end=_end;
            width=_width;
            startTime = Time.time;
        }
    }

    public Material DefaultMaterial;

    public float defWidth;
    
    private List<Laser> Lasers = new List<Laser>{};

    Mesh defaultMesh;

    private void Start() {
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


        for (int i = 0; i < 5; i++)
        {
            AddLaser(Random.insideUnitSphere*4f, Random.insideUnitSphere*4f, null);
        }

    }


    private void Update() {
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        Debug.Log(Lasers.Count);


        int lasercount = Lasers.Count;

        if (lasercount<=0){return;}

        float[] starttimes = new float[lasercount];
        float[] randoms = new float[lasercount];
        float[] widths = new float[lasercount];
        Vector4[] starts = new Vector4[lasercount];
        Vector4[] ends = new Vector4[lasercount];

        Matrix4x4[] matrices = new Matrix4x4[lasercount];

        int i = 0;
        foreach (Laser laser in Lasers)
        {
            starttimes[i] = laser.startTime;
            randoms[i] = Random.value;
            widths[i] = laser.width;
            starts[i] = laser.start;
            ends[i] = laser.end;
            matrices[i] = Matrix4x4.identity;
            i++;
        }

        block.SetFloatArray("startTime",starttimes);
        block.SetFloatArray("random",randoms);
        block.SetFloatArray("width",widths);
        block.SetVectorArray("start",starts);
        block.SetVectorArray("end",ends);

        Debug.Log(block.GetFloatArray("random")[0]);
        Debug.Log(block.GetFloatArray("random")[1]);
        Debug.Log(block.GetFloatArray("random")[2]);

        /*Graphics.DrawMeshInstancedProcedural(
            defaultMesh,
            0,
            DefaultMaterial,
            new Bounds(Vector3.zero, Vector3.one*999999f),
            lasercount,block,ShadowCastingMode.Off,true, 0,null,LightProbeUsage.BlendProbes,null
        );*/

        Graphics.DrawMeshInstanced(defaultMesh,0,DefaultMaterial,matrices,lasercount,block);

    }

    private void _AddLaser(Vector3 start, Vector3 end, float? width){
        Lasers.Add(new Laser(start, end, width.HasValue ? width.Value : defWidth));
    }

    public void AddLaser(Vector3 start, Vector3 end, float? width){_AddLaser(start, end, width.HasValue ? width.Value : defWidth);}
}
