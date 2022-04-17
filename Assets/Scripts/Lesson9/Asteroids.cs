using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

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
    [SerializeField] private float _rotationSpeed;

    private Asteroid[] _asteroids;

    [SerializeField] private float _circleInSecond = 1f;

    private const float _circleRadians = Mathf.PI * 2;

    private TransformAccessArray _transformAccessArray;
    private Vector3[] _positions;
    private float[] _distances;
    private float[] _currentAngels;
    private float[] _circleInSeconds;
    private Vector3[] _rotations;

    // Start is called before the first frame update
    void Start()
    {
        CreateAsteroids();
    }

    // Update is called once per frame
    void Update()
    {
        //for (int i = 0; i < _asteroids.Length; i++)
        //{
        //    var p = _aroundPoint.position;
        //    p.x += Mathf.Sin(_asteroids[i].CurrentAngle) * _asteroids[i].Distance;
        //    p.z += Mathf.Cos(_asteroids[i].CurrentAngle) * _asteroids[i].Distance;
        //    _asteroids[i].Transform.position = p;
        //    _asteroids[i].CurrentAngle += _circleRadians * _circleInSecond * Time.deltaTime;

        //    var r = _asteroids[i].Transform.rotation.eulerAngles;
        //    r += _asteroids[i].Rotation * Time.deltaTime * _rotationSpeed;
        //    _asteroids[i].Transform.rotation = Quaternion.Euler(r);
        //}

        CalculateMovement();
        ApplyMovement();
    }

    private void CalculateMovement()
    {
        NativeArray<Vector3> positions = new NativeArray<Vector3>(_positions, Allocator.TempJob);

        NativeArray<float> angles = new NativeArray<float>(_currentAngels, Allocator.TempJob);

        NativeArray<float> distances = new NativeArray<float>(_distances, Allocator.TempJob);

        NativeArray<float> circleInSecond = new NativeArray<float>(_circleInSeconds, Allocator.TempJob);

        var job = new CalculatePositionsParallelForJob
        {
            Positions = positions,
            Angles = angles,
            Distances = distances,
            CircleInSeconds = circleInSecond,
            CircleInRadians = _circleRadians,
            AroundPosition = _aroundPoint.position,
            DeltaTime = Time.deltaTime
        };

        var jobHandle = job.Schedule(_asteroidAmount, 0);

        jobHandle.Complete();

        positions.CopyTo(_positions);
        angles.CopyTo(_currentAngels);

        positions.Dispose();
        angles.Dispose();
        distances.Dispose();
        circleInSecond.Dispose();
    }

    private void ApplyMovement()
    {
        NativeArray<Vector3> positions = new NativeArray<Vector3>(_positions, Allocator.TempJob);

        NativeArray<Vector3> rotations = new NativeArray<Vector3>(_rotations, Allocator.TempJob);

        var job = new TransformMovementJob
        {
            Positions = positions,
            Rotations = rotations,
            RotationSpeed = _rotationSpeed,
            DeltaTime = Time.deltaTime
        };

        var jobHandle = job.Schedule(_transformAccessArray);

        jobHandle.Complete();

        positions.Dispose();
        rotations.Dispose();
    }

    private void CreateAsteroids()
    {
        //_asteroids = new Asteroid[_asteroidAmount];

        Transform[] transforms = new Transform[_asteroidAmount];

        _positions = new Vector3[_asteroidAmount];
        _rotations = new Vector3[_asteroidAmount];
        _currentAngels = new float[_asteroidAmount];
        _distances = new float[_asteroidAmount];
        _circleInSeconds = new float[_asteroidAmount];

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

            _distances[i] = (_aroundPoint.position - go.transform.position).magnitude;
            _positions[i] = go.transform.position;
            _rotations[i] = Random.insideUnitSphere;
            _currentAngels[i] = angle;
            _circleInSeconds[i] = _circleInSecond;

            //_asteroids[i] = new Asteroid
            //{
            //    Transform = go.transform,
            //    Distance = (_aroundPoint.position - go.transform.position).magnitude,
            //    CurrentAngle = angle,
            //    Rotation = Random.insideUnitSphere
            //};

            transforms[i] = go.transform;
        }

        _transformAccessArray = new TransformAccessArray(transforms);
    }

    private void OnDestroy()
    {
        _transformAccessArray.Dispose();
    }

    private struct CalculatePositionsParallelForJob : IJobParallelFor
    {
        public Vector3 AroundPosition;
        public float CircleInRadians;
        public float DeltaTime;

        [WriteOnly]
        public NativeArray<Vector3> Positions;

        public NativeArray<float> Angles;

        [ReadOnly]
        public NativeArray<float> Distances;

        [ReadOnly]
        public NativeArray<float> CircleInSeconds;

        public void Execute(int index)
        {
            var p = AroundPosition;
            p.x += Mathf.Sin(Angles[index]) * Distances[index];
            p.z += Mathf.Cos(Angles[index]) * Distances[index];
            Positions[index] = p;
            Angles[index] = Angles[index] + CircleInRadians * CircleInSeconds[index] * DeltaTime;
        }
    }

    private struct TransformMovementJob : IJobParallelForTransform
    {
        public float RotationSpeed;
        public float DeltaTime;

        [ReadOnly]
        public NativeArray<Vector3> Positions;

        [ReadOnly]
        public NativeArray<Vector3> Rotations;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = Positions[index];

            var r = transform.rotation.eulerAngles;
            r += Rotations[index] * DeltaTime * RotationSpeed;
            transform.rotation = Quaternion.Euler(r);
        }
    }
}
