using UnityEngine;

namespace Misc
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private float _spawnInterval;
        [SerializeField] private float _radius;
        
        private float _remainingTime;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _remainingTime = _spawnInterval;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _remainingTime -= Time.deltaTime;
            if (_remainingTime <= 0)
            {
                Vector3 offset = (Vector3)(Random.onUnitCircle * _radius);
                Instantiate(_prefab, offset + transform.position, Quaternion.identity);
                _remainingTime += _spawnInterval;
            }
        }
    }
}