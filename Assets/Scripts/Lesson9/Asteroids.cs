using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroids : MonoBehaviour
{
    private struct Asteroid
    {
        public Transform Transform;
        public float CurrentAngle;
        public float Distance;
        public Vector3 Rotation;
    }

    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    
    [SerializeField] private Transform _aroundPoint;

    [SerializeField, Min(10)] private int _asteroidAmount;
    [SerializeField, Min(0)] private float _beltRadius;
    [SerializeField, Min(0)] private float _beltWidth;
    [SerializeField] private float _minScale;
    [SerializeField] private float _maxScale;
    [SerializeField] private float _spinSpeed;

    private Asteroid[] _asteroids;

    [SerializeField] private float _circleInSecond = 1f;

    private const float _circleRadians = Mathf.PI * 2;

    // Start is called before the first frame update
    void Start()
    {
        CreateAsteroids();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _asteroids.Length; i++)
        {
            var p = _aroundPoint.position;
            p.x += Mathf.Sin(_asteroids[i].CurrentAngle) * _asteroids[i].Distance;
            p.z += Mathf.Cos(_asteroids[i].CurrentAngle) * _asteroids[i].Distance;
            _asteroids[i].Transform.position = p;
            var r = _asteroids[i].Transform.rotation.eulerAngles;
            r += _asteroids[i].Rotation * Time.deltaTime * _spinSpeed;
            _asteroids[i].Transform.rotation = Quaternion.Euler(r);
            _asteroids[i].CurrentAngle += _circleRadians * _circleInSecond * Time.deltaTime;
        }
    }

    private void CreateAsteroids()
    {
        _asteroids = new Asteroid[_asteroidAmount];

        var arcAngle = 360 * Mathf.PI / 180;

        var start = _aroundPoint.rotation.y * Mathf.PI / 180 - arcAngle / 2;
        for (int i = 0; i < _asteroidAmount; i++)
        {
            var angle = start + i * (arcAngle / (_asteroidAmount));

            var initialDistance = Random.Range(_beltRadius, _beltRadius + _beltWidth);
            var scale = _minScale <= _maxScale ? Random.Range(_minScale, _maxScale) : Random.Range(_maxScale, _minScale);

            var go = new GameObject($"Asteroid {i}");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = material;
            go.transform.localScale = Vector3.one * scale;

            go.transform.position = _aroundPoint.position + (initialDistance * new Vector3(-Mathf.Sin(angle), _aroundPoint.position.y, Mathf.Cos(angle)));
            go.transform.rotation = Random.rotation;

            _asteroids[i] = new Asteroid {
                Transform = go.transform,
                Distance = (_aroundPoint.position - go.transform.position).magnitude,
                CurrentAngle = angle,
                Rotation = Random.insideUnitSphere
            };
        }
    }
}
