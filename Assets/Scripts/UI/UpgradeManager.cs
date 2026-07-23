using System;
using System.Collections.Generic;
using UnityEngine;
using Upgrades;

namespace UI
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance;
        public List<GameObject> upgradeObjects = new List<GameObject>();
        public List<Upgrade> upgrades = new List<Upgrade>();
        public void Awake()
        {
            Instance = this;
        }

        public void AddUpgrade(Upgrade upgrade)
        {
            upgrades.Add(upgrade);
            GameObject upgradeObj = Instantiate(upgrade.upgradePrefab, transform);
            upgradeObjects.Add(upgradeObj);
        }
    }
}