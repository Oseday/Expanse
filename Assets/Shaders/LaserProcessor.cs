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
        //Material mat;
        //MaterialPropertyBlock block;
        public float startTime;
        public Laser(Vector3 _start, Vector3 _end, float _width){
            start=_start;
            end=_end;
            width=_width;
            //mat=_mat;
            startTime = Time.time;
            /*block = new MaterialPropertyBlock();
            block.SetFloat("StartTime", Time.time);
            block.SetFloat("Random", Random.value);
            block.SetVector("Start", _start);
            block.SetVector("End", _end);*/
        }
    }

    public Material DefaultMaterial;

    public float defWidth;

    //public Shader shaderT;
    
    private List<Laser> Lasers = new List<Laser>{};

    //private ref Camera cameras;

    //private mat 

    Mesh defaultMesh;

    private void Start() {
        //cameras = ref (gameObject.GetComponent<Camera>());
        //mat = new Material(shaderT);
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

        //AddLaser(Vector3.zero, Vector3.one*5f, null);
        //AddLaser(Vector3.zero, Vector3(), null);

        for (int i = 0; i < 5; i++)
        {
            AddLaser(Random.insideUnitSphere*4f, Random.insideUnitSphere*4f, null);
        }

    }

     

    //private void OnRenderImage(RenderTexture src, RenderTexture dest) {
    private void Update() {

        //Combine meshes
        /*Mesh CompleteMesh = new Mesh();

        CombineInstance[] combine = new CombineInstance[Lasers.Length];

        foreach (Laser laser in Lasers)
        {
            combine[i].mesh = 
        }

        CompleteMesh.CombineMeshes(combine,true);*/

        MaterialPropertyBlock block = new MaterialPropertyBlock();

        Debug.Log(Lasers.Count);


        int lasercount = Lasers.Count;

        if (lasercount<=0){return;}

        float[] starttimes = new float[lasercount];
        float[] randoms = new float[lasercount];
        float[] widths = new float[lasercount];
        Vector4[] starts = new Vector4[lasercount];
        Vector4[] ends = new Vector4[lasercount];

        /*block = new MaterialPropertyBlock();
        block.SetFloat("StartTime", Time.time);
        block.SetFloat("Random", Random.value);
        block.SetVector("Start", _start);
        block.SetVector("End", _end);*/

        int i = 0;
        foreach (Laser laser in Lasers)
        {
            starttimes[i] = laser.startTime;
            randoms[i] = Random.value;
            widths[i] = laser.width;
            starts[i] = laser.start;
            ends[i] = laser.end;
            Debug.Log(ends[i]);
            i++;
        }

        block.SetFloatArray("startTime",starttimes);
        block.SetFloatArray("random",randoms);
        block.SetFloatArray("width",widths);
        block.SetVectorArray("start",starts);
        block.SetVectorArray("end",ends);

        Graphics.DrawMeshInstancedProcedural(
            defaultMesh,
            0,
            DefaultMaterial,
            new Bounds(Vector3.zero, Vector3.one*999999f),
            lasercount,block,ShadowCastingMode.Off,true, 0,null,LightProbeUsage.BlendProbes,null
        );
        

        //Material mat = new Material(shaderT);
        //Graphics.DrawProcedural(mat, new Bounds(Vector3.zero, Vector3.one*999999f), MeshTopology.Lines, Lasers.Length*2, Lasers.Length, null, null, ShadowCastingMode.Off, true, 0);
        
        //transform.loss

        /*foreach (Laser laser in Lasers)
        {
            Graphics.DrawProcedural(

            );
        }*/
        //Graphics.Blit(src, dest, mat);
        //Graphics.DrawMesh()
    }

    private void _AddLaser(Vector3 start, Vector3 end, float? width){
        //Lasers[Lasers.Length] = new Laser(start, end, width);
        Lasers.Add(new Laser(start, end, width.HasValue ? width.Value : defWidth));
    }

    public void AddLaser(Vector3 start, Vector3 end, float? width){_AddLaser(start, end, width.HasValue ? width.Value : defWidth);}
    //public void AddLaser(Vector3 start, Vector3 end, float width, Material mat){_AddLaser(start, end, width);}
}
