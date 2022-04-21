using UnityEngine;

namespace Lesson10.Examples
{
    public class RangeAttributeTest : MonoBehaviour
    {
        [RangeAttribute(0, 20), SerializeField] private int _integer;
        [RangeAttribute(0f, 20f), SerializeField] private float _float;
        [RangeAttribute(0f, 20), SerializeField] private string _string;
    }

}