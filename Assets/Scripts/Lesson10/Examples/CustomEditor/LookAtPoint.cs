using UnityEngine;

namespace Lesson10.Examples
{
    [ExecuteInEditMode]
    public class LookAtPoint : MonoBehaviour
    {
        public Vector3 Point = Vector3.zero;

        public void Update()
        {
            transform.LookAt(Point);
        }
    }

}