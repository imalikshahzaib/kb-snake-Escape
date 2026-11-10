using UnityEngine;

namespace ArrowMaze
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("Audio Clips")]
        public AudioClip bgMusic;
        public AudioClip clickSfx;
        public AudioClip successSfx;
        public AudioClip collisionSfx;
        public AudioClip gameOverSfx;
        public AudioClip levelCompleteSfx;
        public AudioClip hintSfx;
        public AudioClip eraseSfx;
        public AudioClip directionalSfx;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Load saved volumes or default to 0.5/1.0
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 1f);

            SetMusicVolume(musicVol);
            SetSfxVolume(sfxVol);

            if (bgMusic != null && musicSource != null)
            {
                musicSource.clip = bgMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void SetMusicVolume(float volume)
        {
            if (musicSource != null) musicSource.volume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }

        public void SetSfxVolume(float volume)
        {
            if (sfxSource != null) sfxSource.volume = volume;
            PlayerPrefs.SetFloat("SfxVolume", volume);
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxSource.volume);
            }
        }

        public void PlayClick() => PlaySfx(clickSfx);
        public void PlaySuccess() => PlaySfx(successSfx);
        public void PlayCollision() => PlaySfx(collisionSfx);
        public void PlayGameOver() => PlaySfx(gameOverSfx);
        public void PlayLevelComplete() => PlaySfx(levelCompleteSfx);
        public void PlayHint() => PlaySfx(hintSfx);
        public void PlayErase() => PlaySfx(eraseSfx);
        public void PlayDirectional() => PlaySfx(directionalSfx);
    }
}
