using System.Runtime.Versioning;
using System.Speech.Synthesis;

namespace EBCEYS.DayOfAllLoversService.Middle
{
    [SupportedOSPlatform("windows")]
    internal class TextSpeekerService : IDisposable
    {
        private readonly SpeechSynthesizer speechSynthesizer = new();
        private readonly ILogger<TextSpeekerService> logger;

        public TextSpeekerService(ILogger<TextSpeekerService> logger, InstalledVoice voice)
        {
            this.logger = logger;
            speechSynthesizer.SelectVoice(voice.VoiceInfo.Name);
        }

        public void Speak(string text)
        {
            logger.LogTrace("Try to speek {text} by voice {voice}", text[..10], speechSynthesizer.Voice.Name);
            speechSynthesizer.Speak(text);
        }

        public void SpeakAsync(string text)
        {
            logger.LogTrace("Try to speek {text} by voice {voice}", text[..10], speechSynthesizer.Voice.Name);
            speechSynthesizer.SpeakAsync(text);
        }

        public void CancelAll()
        {
            logger.LogTrace("Pause and cancel all speakers...");
            speechSynthesizer.Pause();
            speechSynthesizer.SpeakAsyncCancelAll();
        }

        public void Dispose()
        {
            speechSynthesizer.Dispose();
        }
    }
}
