﻿using System;
using System.Threading;
using NAudio.Wave;
using NAudio.Vorbis;
using NVorbis;
using NAudio.Utils;
class Program
{
    static Double volume = 0.1f;
    static bool running = false;
    static WaveOutEvent outputDevice = new WaveOutEvent();
    static string audioFilePath = "";
    static bool isLoop = false;
    static Double currentPositionInSeconds = 0.0f;
    static Double positionInSeconds = 0.0f;

    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: jammer <path_to_audio_file>");
            return;
        }

        audioFilePath = args[0];

        try
        {
            string extension = System.IO.Path.GetExtension(audioFilePath).ToLower();

            switch (extension)
            {

                case ".wav":
                    PlayWav(audioFilePath);
                    break;
                case ".mp3":
                    PlayMp3(audioFilePath);
                    break;
                case ".ogg":
                    PlayOgg(audioFilePath);
                    break;
                case ".flac":
                    PlayFlac(audioFilePath);
                    break;
                default:
                    Console.WriteLine("Unknown file extension: " + extension);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void PlayWav(string audioFilePath)
    {
        using (var reader = new WaveFileReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Handle key events for volume adjustment
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            outputDevice.Volume = 0.5f;
            running = true;

            Thread thread = new Thread(() => Controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static void PlayMp3(string audioFilePath)
    {
        using (var reader = new Mp3FileReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Handle key events for volume adjustment
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            outputDevice.Volume = 0.5f;
            running = true;

            Thread thread = new Thread(() => Controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static void PlayOgg(string audioFilePath)
    {
        using (var reader = new NAudio.Vorbis.VorbisWaveReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Handle key events for volume adjustment
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            outputDevice.Volume = 0.5f;
            running = true;

            Thread thread = new Thread(() => Controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();


        }
    }

    static void PlayFlac(string audioFilePath)
    {
        using (var reader = new AudioFileReader(audioFilePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(reader);
            outputDevice.Play();

            // Handle key events for volume adjustment
            outputDevice.PlaybackStopped += (sender, e) => { outputDevice.Dispose(); };
            outputDevice.Volume = 0.5f;
            running = true;

            Thread thread = new Thread(() => Controls(volume, running, outputDevice, reader));
            thread.Start();

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            manualEvent.WaitOne();
        }
    }

    static void Controls(Double volume, bool running, WaveOutEvent outputDevice, Object reader)
    {
        WaveStream audioStream = (WaveStream)reader;
        long newPosition;
        bool isPlaying = true;
        bool isMuted = false;
        float oldVolume = 0.0f;

        while (running)
        {
            currentPositionInSeconds = (double)audioStream.Position / audioStream.WaveFormat.AverageBytesPerSecond;
            positionInSeconds = (double)audioStream.Length / audioStream.WaveFormat.AverageBytesPerSecond;
            Console.Clear();
            Console.WriteLine(UI(isPlaying, outputDevice.Volume, isMuted));
            if (audioStream.Position >= audioStream.Length)
            {
                if (isLoop)
                {
                    audioStream.Position = 0;
                }
                else
                {
                    Console.WriteLine("Song ended.");
                }
            }
            if (outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                outputDevice.Init(audioStream);
                outputDevice.Play();
            }
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (isMuted) {
                            isMuted = false;
                            outputDevice.Volume = oldVolume;
                        }
                        else {
                            outputDevice.Volume = Math.Min(outputDevice.Volume + 0.05f, 1.0f);
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (isMuted) {
                            isMuted = false;
                            outputDevice.Volume = oldVolume;
                        }
                        else {
                            outputDevice.Volume = Math.Max(outputDevice.Volume - 0.05f, 0.0f);
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        newPosition = audioStream.Position - (audioStream.WaveFormat.AverageBytesPerSecond * 5);

                        if (newPosition < 0)
                        {
                            newPosition = 0; // Go back to the beginning if newPosition is negative
                        }

                        audioStream.Position = newPosition;
                        break;
                    case ConsoleKey.RightArrow:
                        newPosition = audioStream.Position + (audioStream.WaveFormat.AverageBytesPerSecond * 5);

                        if (newPosition > audioStream.Length)
                        {
                            newPosition = audioStream.Length; // Go back to the beginning if newPosition is negative
                            outputDevice.Stop();
                        }

                        audioStream.Position = newPosition;
                        break;
                    case ConsoleKey.Spacebar:
                        if (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            outputDevice.Pause();
                            isPlaying = false;
                        }
                        else
                        {
                            outputDevice.Play();
                            isPlaying = true;
                        }
                        break;
                    case ConsoleKey.Q:
                        running = false;
                        break;
                    case ConsoleKey.L:
                        isLoop = !isLoop;
                        Console.WriteLine("looping: " + isLoop);
                        break;
                    case ConsoleKey.M:
                        if (isMuted)
                        {
                            outputDevice.Volume = oldVolume;
                            isMuted = false;
                        }
                        else
                        {
                            oldVolume = outputDevice.Volume;
                            outputDevice.Volume = 0.0f;
                            isMuted = true;
                        }
                        break;
                }
            }
            Thread.Sleep(5); // don't hog the CPU
        }
    }

    static string UI(bool isPlaying, double volume, bool isMuted)
    {
        string loopText;
        string isPlayingText;
        string ismuteText;

        if (isLoop) {
            loopText = "looping: true";
        } else {
            loopText = "looping: false";
        }

        if (isPlaying) {
            isPlayingText = "Playing";
        } else {
            isPlayingText = "Stopped";
        }

        if (isMuted) {
            ismuteText = "Muted";
        } else {
            ismuteText = "";
        }


        return (Math.Round(currentPositionInSeconds * 100) / 100) + " : "+ (Math.Round(positionInSeconds * 100) / 100) +
        "\nPress 'Up Arrow' to increase volume, 'Down Arrow' to decrease volume, and 'Q' to quit.\n" +
        loopText + "\n" + 
        isPlayingText + "\n" +
        "Volume: " + Math.Round(volume * 100) + "%" + "\n" +
        ismuteText;

    }
}
    