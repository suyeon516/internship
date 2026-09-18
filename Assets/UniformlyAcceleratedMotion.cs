using UnityEngine;
using System.Collections;

public class UniformlyAcceleratedMotion : MonoBehaviour
{
    GameObject ball;
    Vector3 v = new Vector3(0, 0, 0);
    Vector3 a = new Vector3 (0.35f, 0, 0);
    float t = 0;
    float dt = 0.01f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        ball.transform.position = new Vector3(-2, 0, 0);
        ball.transform.localScale = Vector3.one * 0.2f * 2f;
        ball.GetComponent<Renderer>().material.color = Color.green;

        TrailRenderer trail = ball.AddComponent<TrailRenderer>();

        trail.time = 10;
        trail.startWidth = 0.05f;
        trail.endWidth = 0.05f;
        trail.minVertexDistance = 0.02f;

        StartCoroutine(MoveBall());

    }

    IEnumerator MoveBall()
    {
        while (t < 4)
        {
            v += a * dt;
            ball.transform.position += v * dt;
            t += dt;

            yield return new WaitForSeconds(dt);
        }
    }
}
// 수치적 방법
// 오일러크로머 : 속도 업뎃 -> 위치 업뎃
// 오일러:        위치 업뎃 -> 속도 업뎃

// 해석적인 방법 (시간 t에서으ㅢ 위치를 한번에 계산)
// x(t) = x + v*t + 1/2*a*t*t
