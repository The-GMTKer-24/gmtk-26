using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Upgrades;
using Random = UnityEngine.Random;

namespace Entity
{
    public class ShopKeeper : MonoBehaviour
    {
        [SerializeField] private List<Upgrade> upgrades = new List<Upgrade>();

        [SerializeField] private InputAction interactAction;
        
        private Upgrade[] rolled = new Upgrade[3];
        private void OnEnable() => interactAction.Enable();
        private void OnDisable() => interactAction.Disable();
        
        public void OnTriggerStay2D(Collider2D other)
        {
            if (interactAction.IsPressed())
            {
                for (int i = 0; i < 3; i++)
                {
                    int index = Random.Range(0, upgrades.Count);
                    rolled[i] = upgrades[index];
                    upgrades.RemoveAt(index);
                }
                ShopManager.Instance.ShowShop(rolled[0],rolled[1],rolled[2]);
                Destroy(gameObject);
            }
            
        }
    }
}