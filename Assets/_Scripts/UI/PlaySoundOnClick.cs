using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundOnClick : MonoBehaviour
    {
        private void Start()
        {
            var btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(PlaySound);
            }
        }

        private void PlaySound()
        {
             if (Core.SoundManager.Instance != null)
             {
                 Core.SoundManager.Instance.PlayClick();
             }
        }
    }
}
