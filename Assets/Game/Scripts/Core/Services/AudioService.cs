using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace JellyQuestUnity
{
        public sealed class AudioService
        {
            private readonly JellyQuestApp _app;
            private readonly LegacySpriteLibrary _sprites;
            private readonly AudioSource _musicSource;
            private readonly AudioSource _markerSource;
            private readonly AudioSource _sfxSource;
            private readonly Dictionary<string, (float start, float duration, float volume)> _markerMap = new Dictionary<string, (float start, float duration, float volume)>
            {
                { "bouton", (0f, 3.4307483f, 1f) },
                { "correct", (5f, 1.8035147f, 0.6f) },
                { "great", (8f, 1.010703f, 0.8f) },
                { "objectif_complete", (11f, 5.12f, 0.8f) },
                { "you_win", (18f, 3.3619728f, 0.6f) },
                { "shot", (23f, 0.1839229f, 1f) },
                { "leaves", (25f, 1.8818821f, 1f) },
                { "power_up", (28f, 1.2026984f, 1f) }
            };

            private Coroutine _markerCoroutine;
            private bool _muted;

            public AudioService(JellyQuestApp app, LegacySpriteLibrary sprites)
            {
                _app = app;
                _sprites = sprites;

                var musicObject = new GameObject("AudioMusic", typeof(AudioSource));
                musicObject.transform.SetParent(app.transform, false);
                _musicSource = musicObject.GetComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
                _musicSource.volume = 0.3f;

                var markerObject = new GameObject("AudioMarker", typeof(AudioSource));
                markerObject.transform.SetParent(app.transform, false);
                _markerSource = markerObject.GetComponent<AudioSource>();
                _markerSource.playOnAwake = false;

                var sfxObject = new GameObject("AudioSfx", typeof(AudioSource));
                sfxObject.transform.SetParent(app.transform, false);
                _sfxSource = sfxObject.GetComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
            }

            public bool Muted => _muted;

            public void SetMuted(bool muted)
            {
                _muted = muted;
                _musicSource.mute = muted;
                _markerSource.mute = muted;
                _sfxSource.mute = muted;
            }

            public void PlayMusicLoop()
            {
                var clip = _sprites.GetAudioClip("JellyQuest/Raw/assets/music/song17");
                if (_muted || clip == null)
                {
                    return;
                }

                if (_musicSource.clip != clip)
                {
                    _musicSource.clip = clip;
                }

                if (!_musicSource.isPlaying)
                {
                    _musicSource.Play();
                }
            }

            public void StopMusic()
            {
                if (_musicSource.isPlaying)
                {
                    _musicSource.Stop();
                }
            }

            public void PauseMusic()
            {
                if (_musicSource.isPlaying)
                {
                    _musicSource.Pause();
                }
            }

            public void ResumeMusic()
            {
                if (!_muted && _musicSource.clip != null)
                {
                    _musicSource.UnPause();
                }
            }

            public void PlayMarker(string markerName)
            {
                if (_muted || !_markerMap.ContainsKey(markerName))
                {
                    return;
                }

                var clip = _sprites.GetAudioClip("JellyQuest/Raw/assets/music/sounds");
                if (clip == null)
                {
                    return;
                }

                if (_markerCoroutine != null)
                {
                    _app.StopCoroutine(_markerCoroutine);
                    _markerCoroutine = null;
                }

                var marker = _markerMap[markerName];
                _markerCoroutine = _app.StartCoroutine(PlayMarkerRoutine(clip, marker.start, marker.duration, marker.volume));
            }

            public void PlayClip(string resourcePath, float volume = 1f)
            {
                if (_muted)
                {
                    return;
                }

                var clip = _sprites.GetAudioClip(resourcePath);
                if (clip != null)
                {
                    _sfxSource.PlayOneShot(clip, volume);
                }
            }

            private System.Collections.IEnumerator PlayMarkerRoutine(AudioClip clip, float startTime, float duration, float volume)
            {
                _markerSource.Stop();
                _markerSource.clip = clip;
                _markerSource.volume = volume;
                _markerSource.time = Mathf.Clamp(startTime, 0f, clip.length - 0.05f);
                _markerSource.Play();
                yield return new WaitForSecondsRealtime(duration);
                _markerSource.Stop();
            }
        }
}
