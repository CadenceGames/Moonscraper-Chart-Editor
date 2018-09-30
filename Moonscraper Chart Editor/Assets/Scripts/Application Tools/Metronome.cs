﻿// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : UpdateableService {
    ChartEditor editor;
    Vector3 initLocalPos;

    public AudioClip clap;
    OneShotSampleStream sampleStream;

    uint nextClapPos = 0;

    // Use this for initialization
    protected override void Start () {
        editor = ChartEditor.GetInstance();      
        initLocalPos = transform.localPosition;

        sampleStream = AudioManager.LoadSampleStream(clap, 15);

        base.Start();
    }

    // Update is called once per frame
    public override void OnServiceUpdate()
    {
        // Offset by audio calibration
        Vector3 pos = initLocalPos;
        pos.y += TickFunctions.TimeToWorldYPosition(GameSettings.audioCalibrationMS / 1000.0f * GameSettings.gameSpeed);
        transform.localPosition = pos;

        uint currentTickPos = editor.currentSong.WorldYPositionToTick(transform.position.y);

        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            if (currentTickPos >= nextClapPos)
            {
                if (GameSettings.metronomeActive)
                {
                    sampleStream.volume = GameSettings.sfxVolume * GameSettings.vol_master;
                    sampleStream.pan = GameSettings.audio_pan;
                    sampleStream.Play();
                }
            }
        }

        nextClapPos = CalculateNextBeatTickPosition(currentTickPos);
    }

    uint CalculateNextBeatTickPosition(uint currentTickPosition)
    {
        Song song = editor.currentSong;
        var timeSignatures = editor.currentSong.timeSignatures;
        uint standardMeasureLengthTicks = (uint)(Song.RESOLUTIONS_PER_MEASURE * song.resolution);

        int lastTsIndex = SongObjectHelper.FindClosestPositionRoundedDown(currentTickPosition, timeSignatures);
        TimeSignature currentTimeSignature = timeSignatures[lastTsIndex];
        TimeSignature nextTimeSignature = lastTsIndex + 1 < timeSignatures.Count ? timeSignatures[lastTsIndex + 1] : null;
        uint tickOrigin = currentTimeSignature.tick;
        float beatDeltaTick = standardMeasureLengthTicks / currentTimeSignature.beatsPerMeasure;

        if (nextTimeSignature != null && currentTickPosition + beatDeltaTick >= nextTimeSignature.tick)
            return nextTimeSignature.tick;

        uint deltaTick = currentTickPosition - tickOrigin;
        uint remainder = (uint)Mathf.Round(deltaTick % beatDeltaTick);

        return tickOrigin + deltaTick - remainder + (uint)Mathf.Round(beatDeltaTick);
    }
}