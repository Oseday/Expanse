using UnityEngine;

public class InterpolationMethods : MonoBehaviour 
{
    public static Vector3 Hermite(Vector3 p0, Vector3 p1, Vector3 v0, Vector3 v1, float t){
        return (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0 
				+ (t * t * t - 2.0f * t * t + t) * v0 
				+ (-2.0f * t * t * t + 3.0f * t * t) * p1 
				+ (t * t * t - t * t) * v1;
    }

	public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
		Vector3 a = 2f * p1;
		Vector3 b = p2 - p0;
		Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

		//The cubic polynomial: a + b * t + c * t^2 + d * t^3
		Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

		return pos;
	}
}
