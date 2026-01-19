using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioClip _flip;
        [SerializeField] private AudioClip _match;
        [SerializeField] private AudioClip _mismatch;
        [SerializeField] private AudioClip _click;
        [SerializeField] private AudioClip _win;
        [SerializeField] private AudioClip _fail;

        private AudioSource _source;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            transform.SetParent(null); // Ensure it's a root object
            DontDestroyOnLoad(gameObject);
            _source = GetComponent<AudioSource>();
        }

        public void PlayFlip() => Play(_flip);
        public void PlayMatch() => Play(_match);
        public void PlayMismatch() => Play(_mismatch);
        public void PlayClick() => Play(_click);
        public void PlayWin() => Play(_win);
        public void PlayFail() => Play(_fail);

        private void Play(AudioClip clip)
        {
            if (clip) _source.PlayOneShot(clip);
        }
    }
}
