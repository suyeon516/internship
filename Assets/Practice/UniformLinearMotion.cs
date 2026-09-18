using UnityEngine;
using System.Collections;

// 대소문자 주의!!!
public class UniformLinearMotion : MonoBehaviour
{
    GameObject ball;
    Vector3 v = new Vector3(0.8f,0,0);

    float t = 0;
    float dt = 0.01f;

    void Start()
    {
        ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // ball.pos
        ball.transform.position = new Vector3(-2,0,0);
        ball.transform.localScale = Vector3.one * 0.2f * 2f;    // 유니티 기본 sphere 크기 지름 2

        StartCoroutine(MoveBall());

    }
    // IEnumerator는 yield return을 사용하기 위해....
    IEnumerator MoveBall() {
        while (t < 4)
        {
            ball.transform.position += v * dt;
            t += dt;

            yield return new WaitForSeconds(dt);
        }
    }
    // Update()는 매 프레임마다 자동 실행
    
}
