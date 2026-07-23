using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;

namespace UI
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;

        [SerializeField] private TMP_Text upgrade1Title;
        [SerializeField] private TMP_Text upgrade2Title;
        [SerializeField] private TMP_Text upgrade3Title;
        [SerializeField] private TMP_Text upgrade1Description;
        [SerializeField] private TMP_Text upgrade2Description;
        [SerializeField] private TMP_Text upgrade3Description;
        [SerializeField] private TMP_Text upgrade1Cost;
        [SerializeField] private TMP_Text upgrade2Cost;
        [SerializeField] private TMP_Text upgrade3Cost;
        [SerializeField] private Button upgrade1;
        [SerializeField] private Button upgrade2;
        [SerializeField] private Button upgrade3;

        private Upgrade option1;
        private Upgrade option2;
        private Upgrade option3;

        public void Awake()
        {
            Instance = this;
        }

        public void ShowShop(Upgrade option1, Upgrade option2, Upgrade option3)
        {
            UIManager.Instance.Pause();
            this.option1 = option1;
            this.option2 = option2;
            this.option3 = option3;
            
            upgrade1Title.SetText(this.option1.upgradeName);
            upgrade2Title.SetText(this.option2.upgradeName);
            upgrade3Title.SetText(this.option3.upgradeName);
            
            upgrade1Description.SetText(this.option1.description);
            upgrade2Description.SetText(this.option2.description);
            upgrade3Description.SetText(this.option3.description);
            
            upgrade1Cost.SetText($"{this.option1.cost} seconds");
            upgrade2Cost.SetText($"{this.option2.cost} seconds");
            upgrade3Cost.SetText($"{this.option3.cost} seconds");
            
            upgrade1.gameObject.SetActive(true);
            upgrade2.gameObject.SetActive(true);
            upgrade3.gameObject.SetActive(true);
        }

        public void BuyUpgrade(int option)
        {
            if (GetUpgrade(option switch
                {
                    1 => option1, 2 => option2, 3 => option3,
                    _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
                }))
            {
                switch (option)
                {
                    case 1:
                        upgrade1.gameObject.SetActive(false);
                        break;
                    case 2:
                        upgrade2.gameObject.SetActive(false);
                        break;
                    case 3:
                        upgrade3.gameObject.SetActive(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(option), option, null);
                }
            }
        }

        private bool GetUpgrade(Upgrade upgrade)
        {
            if (Player.Player.Instance.TimeEntity.GetTime() > upgrade.cost)
            {
                Player.Player.Instance.TimeEntity.DealDamage(upgrade.cost);
                UpgradeManager.Instance.AddUpgrade(upgrade);
                return true;
            }

            return false;
        }

        public void CancelShop()
        {
            option1 = null;
            option2 = null;
            option3 = null;
            upgrade1.gameObject.SetActive(false);
            upgrade2.gameObject.SetActive(false);
            upgrade3.gameObject.SetActive(false);
        }
    }
}