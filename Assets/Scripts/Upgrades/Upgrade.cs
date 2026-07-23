using UnityEngine;

namespace Upgrades
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "Upgrades/Upgrade Asset", order = 0)]
    public class Upgrade : ScriptableObject
    {
        public string upgradeName;
        public string description;
        public float cost;
        public GameObject upgradePrefab;
    }
}