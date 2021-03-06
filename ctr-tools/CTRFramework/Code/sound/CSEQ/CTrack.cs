﻿using CTRFramework.Shared;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTRFramework.Sound.CSeq
{
    public class CTrack
    {
        //meta
        public string name = "default_name";
        public string address = "";
        public int trackNum = 0;

        public int instrument = 0;
        public bool isDrumTrack = false;

        public List<Command> cmd = new List<Command>();

        public CTrack()
        {
        }

        public void Import(string filename)
        {
            MidiFile midi = new MidiFile(filename);
            FromMidiEventList(midi.Events.GetTrackEvents(1).ToList());
        }

        public void Read(BinaryReaderEx br, int num)
        {
            trackNum = num;
            address = br.HexPos();

            switch (br.ReadInt16())
            {
                case 0: isDrumTrack = false; break;
                case 1: isDrumTrack = true; break;
                default: Console.WriteLine("drum value not boolean at " + br.HexPos()); break;
            }

            Command cx;

            do
            {
                cx = new Command();
                cx.Read(br);

                if (cx.evt == CSEQEvent.ChangePatch)
                    instrument = cx.pitch;

                cmd.Add(cx);
            }
            while (cx.evt != CSEQEvent.EndTrack);

            name = "Track_" + trackNum.ToString("00") + (isDrumTrack ? "_drum" : "");
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(address + "\r\n");

            foreach (Command c in cmd)
                sb.Append(c.ToString());

            return sb.ToString();
        }


        public void FromMidiEventList(List<MidiEvent> events)
        {
            cmd.Clear();

            foreach (var evt in events)
                cmd.Add(Command.FromMidiEvent(evt));
        }


        public List<MidiEvent> ToMidiEventList(int MPQN, int channel, CSEQ seq)
        {
            List<MidiEvent> me = new List<MidiEvent>();
            //MidiEvent x;

            int absTime = 0;

            me.Add(new TextEvent(name, MetaEventType.SequenceTrackName, absTime));
            me.Add(new TempoEvent(MPQN, absTime));

            if (channel == 10)
            {
                me.Add(new ControlChangeEvent(absTime, channel, MidiController.BankSelect, 120));
                me.Add(new ControlChangeEvent(absTime, channel, MidiController.BankSelect, 0));
                me.Add(new PatchChangeEvent(absTime, channel, Meta.GetBankIndex(CSEQ.PatchName)));
            }

            if (CSEQ.UseSampleVolumeForTracks && !CSEQ.IgnoreVolume)
                me.Add(new ControlChangeEvent(absTime, channel, MidiController.MainVolume, seq.samplesReverb[instrument].Volume / 2));

            foreach (Command c in cmd)
            {
                me.AddRange(c.ToMidiEvent(absTime, channel, seq, this));
                absTime += c.wait;
            }

            return me;
        }


        public void WriteBytes(BinaryWriterEx bw)
        {
            bw.Write(isDrumTrack ? (short)1 : (short)0);

            foreach (Command c in cmd)
            {
                c.WriteBytes(bw);
            }
        }
    }
}
