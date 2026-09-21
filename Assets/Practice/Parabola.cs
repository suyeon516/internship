using UnityEngine;

public class Parabola :MonoBehaviour
{
    GameObject ball;
    GameObject  ground;

    Vector3 v = new Vector3(1,1, 0);    // 공 초기 속도
    Vector3 a = new Vector3(0, -0.35f, 0);   // 공 가속도

    float t = 0;
    float dt = 0.01f;

    void Start()
    {
        ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.position = new Vector3(-2,0,0);
        ball.transform.localScale = Vector3.one * 0.2f * 2f;
        // Vector3. one = new Vector3(1,1,1)이랑 똑같음
        ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.transform.position = new Vector3(0, -4, 0);
        ground.transform.localScale = new Vector3(15, -0.01f, 5); 

    }
    void Update()
    {
        if (ball.transform.position.y > ground.transform.position.y)
        {
            v += a * dt;
            ball.transform.position += v * dt;
            t += dt;
        }
    }
}
