using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace YouthRise
{
    public sealed class StoryAudio : MonoBehaviour
    {
        private AudioSource music, narration;
        private AudioClip generatedMusic;
        private Process speech;
        public bool MusicEnabled { get; private set; }
        public bool VoiceEnabled { get; private set; }
        public string VoiceStatus { get; private set; } = "Narasi sintetis • suara sistem";
        public bool IsNarrating => narration != null && narration.isPlaying || SpeechActive();

        private void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            narration = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = narration.playOnAwake = false;
            music.loop = true;
            music.volume = 0.13f;
            music.spatialBlend = narration.spatialBlend = 0f;
            generatedMusic = CreateAmbientLoop();
            music.clip = generatedMusic;
            MusicEnabled = PlayerPrefs.GetInt("YouthRise.music", 1) == 1;
            VoiceEnabled = PlayerPrefs.GetInt("YouthRise.voice", 0) == 1;
            if (MusicEnabled) music.Play();
        }

        public void ToggleMusic()
        {
            MusicEnabled = !MusicEnabled;
            PlayerPrefs.SetInt("YouthRise.music", MusicEnabled ? 1 : 0);
            if (MusicEnabled) music.Play(); else music.Stop();
        }
        public void ToggleVoice()
        {
            VoiceEnabled = !VoiceEnabled;
            PlayerPrefs.SetInt("YouthRise.voice", VoiceEnabled ? 1 : 0);
            if (!VoiceEnabled) StopNarration();
        }
        private void Update()
        {
            music.volume = Mathf.MoveTowards(music.volume, IsNarrating ? 0.035f : 0.13f, Time.unscaledDeltaTime * 0.25f);
        }
        public void Speak(string text)
        {
            StopNarration();
            if (!VoiceEnabled || string.IsNullOrWhiteSpace(text)) return;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // Fixed script only; authored story text goes through stdin, never through shell interpolation.
            const string script = "[Console]::InputEncoding=[System.Text.Encoding]::UTF8; Add-Type -AssemblyName System.Speech; $n=New-Object System.Speech.Synthesis.SpeechSynthesizer; $v=$n.GetInstalledVoices() | Where-Object {$_.VoiceInfo.Culture.Name -eq 'id-ID'} | Select-Object -First 1; if($v){$n.SelectVoice($v.VoiceInfo.Name)}; $n.Rate=0; $n.Volume=85; $n.Speak([Console]::In.ReadToEnd()); $n.Dispose()";
            try
            {
                string shell = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");
                speech = new Process { StartInfo = new ProcessStartInfo(shell, "-NoProfile -NonInteractive -EncodedCommand " + Convert.ToBase64String(Encoding.Unicode.GetBytes(script)))
                { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardInput = true, StandardInputEncoding = Encoding.UTF8 } };
                speech.Start();
                speech.StandardInput.Write(text);
                speech.StandardInput.Close();
            }
            catch (Exception)
            {
                VoiceStatus = "Suara tidak tersedia • baca teks";
                StopNarration();
            }
#else
            VoiceStatus = "Platform ini memerlukan rekaman narasi";
#endif
        }
        public void PlayAuthoredNarration(string resourcePath, string text)
        {
            StopNarration();
            if (!VoiceEnabled) return;
            AudioClip clip = Resources.Load<AudioClip>(resourcePath);
            if (clip != null) { narration.clip = clip; narration.Play(); }
            else Speak(text);
        }
        private bool SpeechActive()
        {
            try { return speech != null && !speech.HasExited; } catch { return false; }
        }
        public void StopNarration()
        {
            if (narration != null) narration.Stop();
            if (speech == null) return;
            try { if (!speech.HasExited) speech.Kill(); } catch { /* Already exited. */ }
            speech.Dispose(); speech = null;
        }
        private void OnApplicationFocus(bool focused) { if (!focused) StopNarration(); }
        private void OnDestroy()
        {
            StopNarration();
            if (generatedMusic != null) Destroy(generatedMusic);
        }
        public static AudioClip CreateAmbientLoop()
        {
            const int rate = 22050;
            const float beat = 0.75f;
            int[] roots = { 48, 45, 41, 43 };
            int[] notes = { 0, 7, 12, 16, 7, 12, 19, 16 };
            float[] samples = new float[rate * 24];
            for (int step = 0; step < 32; step++)
            {
                double hz = 440d * Math.Pow(2d, (roots[step / 8] + notes[step % 8] - 69) / 12d);
                int start = (int)(step * beat * rate);
                for (int j = 0; j < rate * 3; j++)
                {
                    double t = (double)j / rate;
                    double envelope = Math.Min(1d, t / .025d) * Math.Exp(-t * 1.8d) * Math.Min(1d, (3d-t) / .15d);
                    double tone = Math.Sin(2d * Math.PI * hz * t) + .18d * Math.Sin(4d * Math.PI * hz * t);
                    samples[(start+j) % samples.Length] += (float)(tone * envelope * .18d);
                }
            }
            AudioClip clip = AudioClip.Create("YouthRise - Quiet Steps (original procedural score)", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
