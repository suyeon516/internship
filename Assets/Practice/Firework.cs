using UnityEngine;
using System.Collections.Generic;
public class Firework : MonoBehaviour
{
    //불꽃입자
    List<GameObject> balls = new List<GameObject> ();
    List<Vector3> v = new List<Vector3> ();
    Vector3 a = new Vector3(0,-3,0);

    float t = 0;
    float dt = 0.01f;

    bool explosion = false;
    //바닥
    GameObject ground;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        for (int i = 0; i < 100; i++)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            b.transform.position = new Vector3(0, -4, 0);
            b.transform.localScale = Vector3.one * 0.1f * 2f;
            b.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
            balls.Add(b);

            //초기 속도 지정
            v.Add(new Vector3(0,7.0f,0));

        }
        //바닥
           ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position= new Vector3(0,-5, 0);
            ground.transform.localScale = new Vector3(15, 0.01f, 15);
    }

    // Update is called once per frame
    void Update()
    {
        if (t < 15)
        {
            if(t>1 && explosion==false)
            {
                explosion = true;
                // 폭발 시 속도 변경
                /*      foreach로 꺼낸 건 복사본이라 원본이 안바뀜
                foreach(Vector3 velo in v)
                {
                    Vector3 vp = vec(Random.value - 0.5, Random.value - 0.5, Random.value - 0.5);
                    velo += vp;
                }
                */
                for (int i = 0; i < v.Count; i++)
                {
                    Vector3 vp = new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
                    v[i] += vp; // 리스트에 직접 대입해야 반영
                }
            }

            for(int i = 0; i < 100; i++)
            {
                v[i] += a * dt;
                balls[i].transform.position += v[i] * dt;
                // 바닥 충돌 시
                if (balls[i].transform.position.y < ground.transform.position.y)
                {
                    Vector3 pos = balls[i].transform.position;
                    pos.y = ground.transform.position.y;
                    balls[i].transform.position = pos;

                    Vector3 vel = v[i];
                    vel.y = -0.8f * vel.y; // 튕기는 속도 (반대 방향, 80%로 감쇠)
                    v[i] = vel;
                    balls[i].GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
                }
            }

            t += dt;
        }
    }
}
