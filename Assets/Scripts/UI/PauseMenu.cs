using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject ui;
        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            
            if (UIManager.Instance.GetContext() == UIContext.ShopMenu)
            {
                ShopManager.Instance.CancelShop();
                return;
            }
            if (UIManager.Instance.Paused)
            {
                UnPause();
            }
            else
            {
                UIManager.Instance.Pause();
                ui.SetActive(true);
            }
        }

        public void UnPause()
        {
            UIManager.Instance.UnPause();
            ui.SetActive(false);
        }
    }
}