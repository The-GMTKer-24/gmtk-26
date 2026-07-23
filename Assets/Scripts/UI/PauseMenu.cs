using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject ui;
        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (UIManager.Instance.Paused)
                {
                    UIManager.Instance.UnPause();
                    ui.SetActive(false);
                }
                else
                {
                    UIManager.Instance.Pause();
                    ui.SetActive(true);
                }
            }
        }
    }
}