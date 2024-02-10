using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NAudio.Wave;

namespace Steel_Engine
{
    public class AudioSource
    {
        public WaveStream stream;
        public WaveChannel32 audioChannel;
        public float volume { get; private set; }
        public float panning { get; private set; }
        public bool isPlaying { get; private set; }

        public AudioSource(string name, float volume) // only works with .wav files
        {
            stream = new WaveFileReader(InfoManager.usingDataPath + @$"\Audio\{name}.wav");
            audioChannel = new WaveChannel32(stream, volume, 0f);

            this.volume = volume;
            panning = 0f;

            // add this audio source to the output
            AudioManager.AddAudioSource(this);
        }

        public void SetVolume(float zeroToOne)
        {
            volume = zeroToOne;
            audioChannel.Volume = volume;
        }

        public void SetPanning(float minusOneToOne)
        {
            panning = minusOneToOne;
            audioChannel.Pan = panning;
        }

        public AudioSource(string name) // only works with .wav files
        {
            stream = new WaveFileReader(InfoManager.usingDataPath + @$"\Audio\{name}.wav");
            audioChannel = new WaveChannel32(stream, 1, 0f);

            volume = 1;
            panning = 0f;

            // add this audio source to the output
            AudioManager.AddAudioSource(this);
        }

        public void SetAudioFromPath(string path)
        {
            stream = new WaveFileReader(path);
        }

        public void SetAudioFromName(string name)
        {
            stream = new WaveFileReader(InfoManager.usingDataPath + @$"\Audio\{name}.wav");
        }

        public void Play()
        {
            AudioManager.PlayAudio(audioChannel);
            isPlaying = true;
        }

        public void Restart()
        {
            Stop();
            audioChannel.Position = 0;
            isPlaying = true;
        }

        public void Stop()
        {
            AudioManager.StopAudio(audioChannel);
            isPlaying = false;
        }
    }

    public static class AudioManager
    {
        private static List<AudioSource> sources = new List<AudioSource>();
        private static MixingWaveProvider32 mixer = new MixingWaveProvider32();
        private static DirectSoundOut dso = new DirectSoundOut(DirectSoundOut.DSDEVID_DefaultPlayback);

        public static void Init()
        {
            // is slow and so can take a few seconds
            mixer = new MixingWaveProvider32();
            dso.Init(mixer);
            dso.Play();
        }

        public static void StopDSO()
        {
            // TESTCODE ???
            //dso.Stop();
        }

        public static void PlayDSO()
        {
            dso.Play();
        }

        public static void PlayAudio(WaveChannel32 channel)
        {
            mixer.AddInputStream(channel);
        }

        public static void StopAudio(WaveChannel32 channel)
        {
            mixer.RemoveInputStream(channel);
        }

        public static void CleanupAudioSources()
        {
            mixer = new MixingWaveProvider32();
            dso = new DirectSoundOut(DirectSoundOut.DSDEVID_DefaultPlayback);
            dso.Init(mixer);
            sources.Clear();
        }

        public static void AddAudioSource(AudioSource source)
        {
            sources.Add(source);
        }
    }
}
