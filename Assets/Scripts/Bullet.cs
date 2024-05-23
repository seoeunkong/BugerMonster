using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2[] _point = new Vector2[4];

    [SerializeField][Range(0, 1)] private float t = 0;
    [SerializeField] public float spd = 1f;
    public const float posA = 0.55f;
    public const float posB = 0.45f;

    private Character _enemy = null;
    private float _attackPower;

    private void FixedUpdate()
    {
        if (Fire())
        {
            if (t > 1) return;
            if(!this.gameObject.activeSelf) this.gameObject.SetActive(true);
            t += Time.deltaTime * spd;
            DrawTrajectory();

            if(t >= 0.9f)
            {
                _enemy.Hit(_attackPower);
                Deactivate();
            }
        }
    }

    public void Init(Character master, Character enemy)
    {
        transform.position = master.transform.position;

        _point[0] = master.transform.position; // P0
        _point[1] = SetPoint(master.transform.position); // P1
        _point[2] = SetPoint(enemy.transform.position); // P2
        _point[3] = enemy.transform.position; // P3

        _enemy = enemy;
    }

    private bool Fire() => _enemy != null;
    public void SetAttackPower(float damage) => _attackPower = damage;

    private Vector2 SetPoint(Vector2 origin)
    {
        float x, y;

        x = posA * Mathf.Cos(Random.Range(0, 360) * Mathf.Deg2Rad) + origin.x;
        y = posB * Mathf.Sin(Random.Range(0, 360) * Mathf.Deg2Rad) + origin.y;
        return new Vector2(x, y);
    }

    private void DrawTrajectory()
    {
        transform.position = new Vector3(
        FourPointBezier(_point[0].x, _point[1].x, _point[2].x, _point[3].x),
        FourPointBezier(_point[0].y, _point[1].y, _point[2].y, _point[3].y), -6);
    }

    private void Deactivate()
    {
        _enemy = null;
        t = 0;
        this.gameObject.SetActive(false);
    }
    


    /// <summary>
    /// 3차 베지어 곡선.
    /// </summary>
    /// <param name="a">시작 위치</param>
    /// <param name="b">시작 위치에서 얼마나 꺾일 지 정하는 위치</param>
    /// <param name="c">도착 위치에서 얼마나 꺾일 지 정하는 위치</param>
    /// <param name="d">도착 위치</param>
    /// <returns></returns>
    private float FourPointBezier(float a, float b, float c, float d)
    {
        return Mathf.Pow((1 - t), 3) * a
        + Mathf.Pow((1 - t), 2) * 3 * t * b
        + Mathf.Pow(t, 2) * 3 * (1 - t) * c
        + Mathf.Pow(t, 3) * d;
    }

}

