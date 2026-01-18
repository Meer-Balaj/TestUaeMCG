using UnityEngine;

namespace Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _flipSound;
        [SerializeField] private AudioClip _matchSound;
        [SerializeField] private AudioClip _mismatchSound;
        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private AudioClip _gameWinSound;
        [SerializeField] private AudioClip _gameFailSound;

        [Header("Settings")]
        [Range(0f, 1f)] [SerializeField] private float _volume = 1f;

        private AudioSource _audioSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayFlip() => PlaySound(_flipSound);
        public void PlayMatch() => PlaySound(_matchSound);
        public void PlayMismatch() => PlaySound(_mismatchSound);
        public void PlayClick() => PlaySound(_clickSound);
        public void PlayGameWin() => PlaySound(_gameWinSound);
        public void PlayGameFail() => PlaySound(_gameFailSound);

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip, _volume);
            }
        }
    }
}
