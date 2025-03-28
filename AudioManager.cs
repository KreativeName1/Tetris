using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Tetris;

public class AudioManager
{
    private SoundEffect _gameStart;
    private SoundEffect _hardDrop;
    private SoundEffect _land;

    private int _lastIndex = -1;
    private SoundEffect _levelUp;
    private SoundEffect _lineClear;
    private SoundEffect _pause;
    private SoundEffect _rotateSound;

    public int MaxMusicIndex;
    public int MinMusicIndex;
    public List<Song> MusicList;

    public bool IsPaused { get; private set; }

    public void LoadContent(ContentManager content)
    {
        _lineClear = content.Load<SoundEffect>("sound/line_clear");
        _rotateSound = content.Load<SoundEffect>("sound/block_rotate");
        _land = content.Load<SoundEffect>("sound/block_land");
        _levelUp = content.Load<SoundEffect>("sound/level_up");
        _pause = content.Load<SoundEffect>("sound/pause");
        _hardDrop = content.Load<SoundEffect>("sound/hard_drop");

        MusicList = new List<Song>
        {
            content.Load<Song>("music/gameover"),
            content.Load<Song>("music/title"),
            content.Load<Song>("music/theme_a"),
            content.Load<Song>("music/theme_b"),
            content.Load<Song>("music/theme_c"),
            content.Load<Song>("music/theme_d")
        };

        MaxMusicIndex = MusicList.Count - 1;
        MinMusicIndex = 1;
    }

    public void PlayBackgroundMusic(int index, bool ignoreSameSongPlayback = false, bool loop = true)
    {
        if (MusicList.Count <= index || index < 0)
        {
            MediaPlayer.Stop();
            return;
        }

        if (ignoreSameSongPlayback && index == _lastIndex) return;

        MediaPlayer.Stop();
        MediaPlayer.Play(MusicList[index]);
        MediaPlayer.IsRepeating = loop;
        MediaPlayer.Volume = 0.5f;
        _lastIndex = index;
    }


    public void PlayLineClear()
    {
        _lineClear?.Play();
    }

    public void PlayRotateSound()
    {
        _rotateSound?.Play();
    }

    public void PlayLand()
    {
        _land?.Play();
    }

    public void PlayLevelUp()
    {
        _levelUp?.Play();
    }

    public void PlayPause()
    {
        _pause?.Play();
    }

    public void PlayHardDrop()
    {
        _hardDrop?.Play();
    }

    public void Pause()
    {
        IsPaused = true;
        MediaPlayer.Pause();
    }

    public void Resume()
    {
        IsPaused = false;
        MediaPlayer.Resume();
    }
}