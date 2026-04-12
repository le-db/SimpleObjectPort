using UnityEngine;

namespace SimpleObjectPort.Example
{
    [CreateAssetMenu(menuName = "SimpleObjectPort/MySO", fileName = "MySO.asset")]
    public class MySO : ScriptableObject
    {
        public string someData;
        public MySO someReference;
        public GameObject someGoReference;
    }
}
