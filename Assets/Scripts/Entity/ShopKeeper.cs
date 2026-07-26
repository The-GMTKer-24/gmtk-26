using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Upgrades;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Entity
{
    public class ShopKeeper : MonoBehaviour
    {
        private const int UpgradeChoices = 3;

        [SerializeField] private List<Upgrade> upgrades = new();
        [SerializeField] private InputAction interactAction;

        private readonly Upgrade[] rolled = new Upgrade[UpgradeChoices];

        private void OnEnable()
        {
            interactAction.Enable();
        }

        private void OnDisable()
        {
            interactAction.Disable();
        }

#if UNITY_EDITOR
        [ContextMenu("Find All Upgrade Assets")]
        private void FindAllUpgradeAssets()
        {
            Undo.RecordObject(this, "Find All Upgrade Assets");

            upgrades.Clear();

            string[] upgradeGuids =
                AssetDatabase.FindAssets($"t:{nameof(Upgrade)}");

            foreach (string guid in upgradeGuids)
            {
                string assetPath =
                    AssetDatabase.GUIDToAssetPath(guid);

                Upgrade upgrade =
                    AssetDatabase.LoadAssetAtPath<Upgrade>(assetPath);

                if (upgrade != null && !upgrades.Contains(upgrade))
                {
                    upgrades.Add(upgrade);
                }
            }

            upgrades.Sort((first, second) =>
                string.Compare(
                    first.name,
                    second.name,
                    StringComparison.OrdinalIgnoreCase));

            EditorUtility.SetDirty(this);

            Debug.Log(
                $"Found and added {upgrades.Count} Upgrade assets.",
                this);
        }
#endif

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!interactAction.IsPressed())
            {
                return;
            }

            List<Upgrade> availableUpgrades = GetAvailableUpgrades();

            if (availableUpgrades.Count < UpgradeChoices)
            {
                Debug.LogWarning(
                    $"The shop requires {UpgradeChoices} available upgrades, " +
                    $"but only {availableUpgrades.Count} were found.",
                    this);

                Destroy(gameObject);
                return;
            }

            RollUpgrades(availableUpgrades);

            ShopManager.Instance.ShowShop(
                rolled[0],
                rolled[1],
                rolled[2]);

            Destroy(gameObject);
        }

        private List<Upgrade> GetAvailableUpgrades()
        {
            List<Upgrade> availableUpgrades = new();

            foreach (Upgrade upgrade in upgrades)
            {
                if (upgrade == null)
                {
                    continue;
                }

                if (UpgradeManager.Instance.DoesHaveUpgrade(upgrade))
                {
                    continue;
                }

                if (!availableUpgrades.Contains(upgrade))
                {
                    availableUpgrades.Add(upgrade);
                }
            }

            return availableUpgrades;
        }

        private void RollUpgrades(List<Upgrade> availableUpgrades)
        {
            for (int i = 0; i < UpgradeChoices; i++)
            {
                int randomIndex = Random.Range(0, availableUpgrades.Count);

                rolled[i] = availableUpgrades[randomIndex];
                availableUpgrades.RemoveAt(randomIndex);
            }
        }
    }
}