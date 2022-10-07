using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Text;
using System.Timers;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Player.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";
        
        // private static string MoviePath = @"/home/paulm/Videos/compress/";
        // private static string OutputPath = @"/home/paulm/Videos/compress/compressed/";        
        private static string MoviePath = @"/media/paulm/ADATA HD720/Compression/";
        private static string OutputPath = @"/media/paulm/ADATA HD720/Compression/compressed/";

        private static int NoFrames = 2400;
        
        private IImage _original;
        public IImage Original
        { 
            get => _original;
            set => this.RaiseAndSetIfChanged(ref _original, value);
        }
        
        private IImage _compressed;
        public IImage Compressed
        { 
            get => _compressed;
            set => this.RaiseAndSetIfChanged(ref _compressed, value);
        }
        
        private bool _isCompressedShown;
        public bool IsCompressedShown
        { 
            get => _isCompressedShown;
            set => this.RaiseAndSetIfChanged(ref _isCompressedShown, value);
        }

        public int currentIndex = 1;

        public bool isFirstRun = false;
        
        private bool _isPlaying;
        public bool IsPlaying
        { 
            get => _isPlaying;
            set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
        }

        private Timer _timer = new Timer(33);
        
        public ReactiveCommand<Unit,Unit> PlayPauseCommand { get; }
        private void PlayPauseAction()
        {
            if (IsPlaying)
            {
                _timer.Enabled = true;
                return;
            }

            _timer.Enabled = false;
        }
 
        public ReactiveCommand<Unit,Unit> PrevCommand { get; }
        private void PrevAction()
        {
            _timer.Enabled = false;
            if (--currentIndex == -1)
            {
                currentIndex = NoFrames;
            }
            Original = new Bitmap(Path.Combine(MoviePath, $"xFrame{currentIndex.ToString().PadLeft(4, '0')}.bmp"));
            Compressed = new Bitmap(Path.Combine(OutputPath, $"xFrame{currentIndex.ToString().PadLeft(4, '0')}.bmp"));
        }
        public ReactiveCommand<Unit,Unit> NextCommand { get; }
        private void NextAction()
        { 
            _timer.Enabled = false;
            if (++currentIndex == NoFrames+1)
            {
                currentIndex = 1;
            }
            Original = new Bitmap(Path.Combine(MoviePath, $"xFrame{currentIndex.ToString().PadLeft(4, '0')}.bmp"));
            Compressed = new Bitmap(Path.Combine(OutputPath, $"xFrame{currentIndex.ToString().PadLeft(4, '0')}.bmp"));
        }
 
        
        
        public MainWindowViewModel()
        {
            if (isFirstRun) return;
            PrevCommand = ReactiveCommand.Create(PrevAction);
            NextCommand = ReactiveCommand.Create(NextAction);
            PlayPauseCommand = ReactiveCommand.Create(PlayPauseAction);
            _timer.Elapsed += OnElapsed;
            isFirstRun = true;
            for (int i = 1; i < 2; i++)
            {
                Original = new Bitmap(Path.Combine(MoviePath, $"xFrame{i.ToString().PadLeft(4, '0')}.bmp"));
                Compressed = new Bitmap(Path.Combine(OutputPath, $"xFrame{i.ToString().PadLeft(4, '0')}.bmp"));
            }
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (++currentIndex == NoFrames+1)
            {
                currentIndex = 1;
            }
            Original = new Bitmap(Path.Combine(MoviePath, $"xFrame{currentIndex.ToString().PadLeft(4, '0')}.bmp"));
            Compressed = new Bitmap(Path.Combine(OutputPath, $"xFrame{currentIndex.ToString().PadLeft(4, '0')}.bmp"));
        }
    }
}