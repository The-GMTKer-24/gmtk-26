using System;
using UnityEngine;
using Upgrades;

namespace UI
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;

        
        public void Awake()
        {
            Instance = this;
        }

        public void ShowShop(Upgrade option1, Upgrade option2, Upgrade option3)
        {
            
        }
        
        public void CancelShop()
        {
            
        }
    }
}