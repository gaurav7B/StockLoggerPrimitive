using System.Collections.Concurrent;
using System.Diagnostics;

namespace StockLogger.BackgroundServices.Helper_methods
{

    public static class Speaker
    {
        private static readonly ConcurrentQueue<string> _queue = new();
        private static bool _isSpeaking = false;
        private static readonly object _lock = new();

        public static void Speak(string text)
        {
            text = text.Replace("'", "");
            _queue.Enqueue(text);
            ProcessQueue();
        }

        private static void ProcessQueue()
        {
            lock (_lock)
            {
                if (_isSpeaking) return;
                _isSpeaking = true;
            }

            Task.Run(() =>
            {
                while (_queue.TryDequeue(out var message))
                {
                    SpeakInternal(message);
                }

                lock (_lock)
                {
                    _isSpeaking = false;
                }
            });
        }

        private static void SpeakInternal(string text)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-Command \"Add-Type -AssemblyName System.Speech; " +
                            $"$speak = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
                            $"$speak.Speak('{text}')\"",
                UseShellExecute = true,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            process.WaitForExit(); // 🔥 THIS is key (wait until speech finishes)
        }
    }

    //public static class Speaker
    //{
    //    public static void Speak(string text)
    //    {
    //        Task.Run(() =>
    //        {
    //            text = text.Replace("'", ""); // safety

    //            var psi = new ProcessStartInfo
    //            {
    //                FileName = "powershell",
    //                Arguments = $"-Command \"Add-Type -AssemblyName System.Speech; " +
    //                            $"$speak = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
    //                            $"$speak.Speak('{text}')\"",
    //                UseShellExecute = true,
    //                CreateNoWindow = true
    //            };

    //            Process.Start(psi);
    //        });
    //    }
    //}
}
