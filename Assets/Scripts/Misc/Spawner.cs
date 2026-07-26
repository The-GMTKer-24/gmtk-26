using System;
using UnityEngine;
using Random = UnityEngine.Random;

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
                Vector2 offset = Vector2.zero;
                for (int i = 0; i < 100; i++)
                {
                    offset = (Vector3)(Random.onUnitCircle * _radius);
                    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, offset, _radius);
                    if (hits[0].collider.gameObject != this.gameObject)
                        throw new Exception("I don't know how this ****ing code works!!!");
                    //print("Length: " + hits.Length);
                    if (hits.Length == 1) {break;}

                    if (i > 90) throw new Exception("Spawner search timed out!");
                }
                
                print ("Spawning " + _prefab.name + " at " + transform.position);
                Vector2 spawnPosition = offset + (Vector2)transform.position;
                Instantiate(_prefab, spawnPosition, Quaternion.identity);
                _remainingTime += _spawnInterval;
            }
        }
    }
}