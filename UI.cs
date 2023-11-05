﻿using System;
using NAudio.Wave;
using Spectre.Console;

namespace jammer
{
    internal class UI
    {
        static public void Ui(WaveOutEvent outputDevice)
        {
            var loopText = Program.isLoop ? "looping: true" : "looping: false";
            var isPlayingText = outputDevice.PlaybackState == PlaybackState.Playing ? "Playing" : "Paused";
            var ismuteText = Program.isMuted ? "Muted" : "";

            // currentPositionInSeconds
            int cupMinutes = (int)(Program.currentPositionInSeconds / 60);
            int cupSeconds = (int)(Program.currentPositionInSeconds % 60);

            string currentPositionInSecondsText = $"{cupMinutes}:{cupSeconds:D2}";

            // render table
            var tableJam = new Table();
            var table = new Table();

            tableJam.AddColumn("♫ Jamming to: " + Program.audioFilePath + " ♫");

            table.AddColumn("State");

            table.AddColumn("Current Position");

            table.AddColumn("Looping");

            table.AddColumn("Volume");

            table.AddColumn("Muted");

            table.AddRow(isPlayingText, currentPositionInSecondsText + " / " + Program.positionInSecondsText, loopText, Math.Round(outputDevice.Volume * 100) + " % " , ismuteText);


            AnsiConsole.Write(tableJam);
            AnsiConsole.Write(table);
        }
    }
}
