using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] musicClips, winClips, loseClips, bonusClips;
    public AudioClip clearSound, starCollectedSound;

    [Range(0f, 1f)]
    public float musicVolume = 1f, fxVolume = 1.0f;

    public float lowPitch = 0.95f, highPitch = 1.05f;

    void OnEnable()
    {
        PlayMusic();
        GamePiece.PieceCleared += PlayClearSound;
        Board.OnBonusUpdate += PlayBonusSound;
        GameManager.GameOver += PlayGameOverSound;
        LevelGoal.StarCollected += PlayStarCollectedSound;
    }

    private void OnDisable()
    {
        GamePiece.PieceCleared -= PlayClearSound;
        Board.OnBonusUpdate -= PlayBonusSound;
        GameManager.GameOver -= PlayGameOverSound;
        LevelGoal.StarCollected -= PlayStarCollectedSound;
    }

    public AudioSource PlayClipAtPoint(AudioClip clip, float volume = 1f, Vector3 position = default(Vector3))
    {
        if(clip == null) return null;

        GameObject go = new GameObject($"SFX_{clip.name}");
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.pitch = Random.Range(lowPitch, highPitch);
        source.volume = volume;
        source.Play();

        Destroy(go, clip.length);
        return source;
    }

    public AudioSource PlayRandom(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return null;

        return PlayClipAtPoint(clips[Random.Range(0, clips.Length)], volume);
    }

    public void PlayMusic()
    {
        PlayRandom(musicClips, musicVolume).loop = true;
    }
      
    public void PlayBonusSound(bool increase)
    {
        if(increase) PlayRandom(bonusClips, fxVolume);
    }

    private void PlayGameOverSound(bool isWinner)
    {
        PlayRandom(isWinner ? winClips : loseClips, fxVolume);
    }

    private void PlayClearSound(Vector3 position, int _)
    {
        PlayClipAtPoint(clearSound, fxVolume, position);
    }

    private void PlayStarCollectedSound(int starsNo)
    {
        PlayClipAtPoint(starCollectedSound, fxVolume);
    }
}
