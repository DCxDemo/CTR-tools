﻿using CTRFramework.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CTRFramework.Sound
{

    public class Bank
    {
        public static Dictionary<int, string> banknames = new Dictionary<int, string>();

        public Dictionary<int, byte[]> samples = new Dictionary<int, byte[]>();

        public static void ReadNames()
        {
            banknames = Meta.LoadNumberedList("banknames.txt");
        }

        public Bank()
        {
        }
        public Bank(BinaryReaderEx br)
        {
            Read(br);
        }

        public static Bank FromFile(string filename)
        {
            using (BinaryReaderEx br = new BinaryReaderEx(File.OpenRead(filename)))
            {
                return new Bank(br);
            }
        }

        public static Bank FromReader(BinaryReaderEx br)
        {
            return new Bank(br);
        }

        public void Read(BinaryReaderEx br)
        {
            int bankoffset = (int)br.BaseStream.Position;
            int sampCnt = br.ReadInt16();
            short[] info = br.ReadArrayInt16(sampCnt);

            br.BaseStream.Position = bankoffset + 0x800;

            int sample_start = 0;
            int sample_end = 0;


            byte[] buf;

            for (int i = 0; i < sampCnt; i++)
            {
                sample_start = (int)br.BaseStream.Position;

                br.BaseStream.Position += 16;

                //Console.WriteLine(br.BaseStream.Position.ToString("X8"));

                do
                {
                    buf = br.ReadBytes(16);
                }
                while (!frameIsEmpty(buf));

                sample_end = (int)br.BaseStream.Position;

                br.BaseStream.Position = sample_start;

                if (!samples.ContainsKey(info[i]))
                {
                    samples.Add(info[i], br.ReadBytes(sample_end - sample_start));
                }
                else
                {
                    Console.WriteLine("dupe key: {0}", info[i]);
                }
            }
        }



        public bool Contains(int key)
        {
            return samples.ContainsKey(key);
        }


        public void Export(int id, int freq, string path, string path2 = null, string name = null)
        {
            string pathSfxVag = Path.Combine(path, "wav");
            string pathSfxWav = Path.Combine(path, "vag");

            Helpers.CheckFolder(pathSfxVag);
            Helpers.CheckFolder(pathSfxWav);

            if (Contains(id))
            {
                string vagpath = Path.Combine(path, (path2 == null ? "vag" : path2));
                Directory.CreateDirectory(vagpath);

                //string vagname = vagpath + "\\" +  (name == null ?  (Howl.sampledict.ContainsKey(id) ? Howl.sampledict[id] : "sample_" + id.ToString("0000")) : name) + ".vag";
                string vagname = vagpath + "\\" + Howl.GetName(id, Howl.samplenames);


                /*
                if (name != null)
                {
                    vagname += (name != null ? name : "sample") + (Howl.samplenames.ContainsKey(id) ? "_" + Howl.samplenames[id] : "") + ".vag";
                }
                else
                {
                    vagname += "sample_" + id.ToString("0000") + ".vag";
                }
                */

                Console.WriteLine(vagname);

                using (BinaryReaderEx br = new BinaryReaderEx(new MemoryStream(samples[id])))
                {
                    VagSample vag = new VagSample();
                    if (freq != -1)
                        vag.sampleFreq = freq;
                    if (Howl.samplenames.ContainsKey(id))
                        vag.SampleName = Howl.samplenames[id];
                    vag.ReadFrames(br, samples[id].Length);

                    vag.Save(vagname);
                    vag.ExportWav(vagname.Replace("vag", "wav")); //lmao
                }
            }
        }

        public void ExportAll(int bnum, string path, string path2 = null)
        {
            int i = 0;
            foreach (KeyValuePair<int, byte[]> s in samples)
            {
                Export(s.Key, Howl.GetFreq(s.Key), path, path2, s.Key.ToString("0000") + "_" + s.Key.ToString("X4"));
                i++;
            }
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(samples.Count + " samples total\r\n");

            int cnt = 0;

            foreach (KeyValuePair<int, byte[]> s in samples)
            {
                sb.Append(s.ToString() + "\r\n");
                cnt++;
            }

            return sb.ToString();
        }



        #region [Private functions]

        private bool frameIsEmpty(byte[] buf)
        {
            foreach (byte b in buf)
                if (b != 0) return false;

            return true;
        }

        #endregion
    }
}
