using UnityEngine;
using System;

public class EarthMoon : MonoBehaviour   // Earth에 붙임
{
    public Transform moon;               // Inspector에서 Moon 연결
    public int stepsPerFrame = 20;       // 한 프레임에 계산을 몇 번 돌릴지 

    const double G = 6.67e-11;
    const double earthMass = 5.972e24;
    const double moonMass = 7.36e22;
    const double dt = 60;                // 계산 한 번에 흐르는 시간
    const double view = 1e-7;            // 화면 축소 비율 
    const float sf = 6f;                 // 지구 크기 보정
    const float moonSf = 60f;            // 달은 너무 작아 보여서 따로 더 키움

    // x, y만 double로 관리
    double ex, ey, evx, evy;             // 지구 위치, 속도
    double mx, my, mvx, mvy;             // 달 위치, 속도

    void Start()
    {
        double r = 385000e3;             // 지구-달 거리
        mx = r; my = 0;
        mvx = 0; mvy = 1022;
        ex = ey = evx = evy = 0;

        // 크기 (Sphere 크기값은 지름이라 반지름 x 2)
        transform.localScale = Vector3.one * (float)(2 * 6400000 * sf * view);
        moon.localScale = Vector3.one * (float)(2 * 173700 * moonSf * view);

        Camera.main.transform.position = new Vector3(0, 0, -100);
        Camera.main.transform.LookAt(Vector3.zero);

        ApplyPositions();
    }

    void Update()
    {
        for (int i = 0; i < stepsPerFrame; i++)
            Step();
        ApplyPositions();
    }

    void Step()
    {
        double rx = mx - ex;             // r = Moon.pos - Earth.pos
        double ry = my - ey;
        double d = Math.Sqrt(rx * rx + ry * ry);

        double F = G * earthMass * moonMass / (d * d);
        double fx = -F * rx / d;         // 달이 받는 힘 (지구 쪽으로)
        double fy = -F * ry / d;

        // 속도 갱신 (달은 +f, 지구는 -f)
        mvx += fx / moonMass * dt;
        mvy += fy / moonMass * dt;
        evx += -fx / earthMass * dt;
        evy += -fy / earthMass * dt;

        // 위치 갱신
        mx += mvx * dt;
        my += mvy * dt;
        ex += evx * dt;
        ey += evy * dt;
    }

    void ApplyPositions()
    {
        transform.position = new Vector3((float)(ex * view), (float)(ey * view), 0);
        moon.position = new Vector3((float)(mx * view), (float)(my * view), 0);
    }
}