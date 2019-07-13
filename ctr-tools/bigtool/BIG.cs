﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using p = bigtool.Properties.Resources;
using Newtonsoft.Json.Linq;

namespace bigtool
{
    public struct Pair
    {
        public uint size;
        public uint offset;

        public Pair(uint o, uint s)
        {
            offset = o;
            size = s;
        }
    }

    class BIG
    {
        BinaryReader br;
        MemoryStream ms;

        public uint totalFiles = 0;

        public List<Pair> pairs = new List<Pair>();

        Dictionary<int, string> names = new Dictionary<int, string>();

        List<CTRFile> ctrfiles = new List<CTRFile>();


        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        JObject json;

        public BIG()
        {
        }

        public string DetectBig(string md5)
        {
            if (File.Exists(verpath))
            {
                json = JObject.Parse(File.ReadAllText(verpath));

                for (int i = 0; i < ((JArray)json["versions"]).Count; i++)
                {
                    if (md5 == json["versions"][i]["big_md5"].ToString())
                        return
                            String.Format(
                                "{0} ({1})",
                                json["versions"][i]["name"].ToString(),
                                json["versions"][i]["timestamp"].ToString()
                            );
                }
            }

            return "Unknown";
        }


        public string path;
        string basepath;
        string filelist;
        string bigname;
        string bigpath;
        string extpath;
        string verpath;

        public void InitPaths(string fn)
        {
            path = fn;
            basepath = System.AppDomain.CurrentDomain.BaseDirectory;
            filelist = Path.Combine(basepath, "filenames.txt");
            bigname = Path.GetFileNameWithoutExtension(fn);
            bigpath = Path.GetDirectoryName(fn);
            if (bigpath == null || bigpath == "") bigpath = basepath;
            extpath = Path.Combine(bigpath, bigname) + "\\";
            verpath = Path.Combine(basepath, "versions.json");
            
            //uncomment if something goes wrong
            /*
            Console.WriteLine(path);
            Console.WriteLine(basepath);
            Console.WriteLine(filelist);
            Console.WriteLine(bigname);
            Console.WriteLine(bigpath);
            Console.WriteLine(extpath);
            */
        }


        public BIG(string fn)
        {
            Console.WriteLine("Begin: " + fn);
            InitPaths(fn);

            bool listExists = LoadNames(filelist);
            Console.WriteLine("File list - {0}\r\n", (listExists ? "OK" : "ERROR"));

            Console.WriteLine(p.calc_md5);

            string md5 = CalculateMD5(fn);
            string reg = DetectBig(md5);

            Console.WriteLine("MD5 = " + md5);
            Console.WriteLine(reg + "\r\n");

            if (reg == "Unknown")
                File.WriteAllText("unknown_md5.txt", md5);

            ms = new MemoryStream(File.ReadAllBytes(fn));
            br = new BinaryReader(ms);

            br.BaseStream.Position += 4;
            totalFiles = br.ReadUInt32();

            for (int i = 0; i < totalFiles; i++)
                pairs.Add(new Pair(br.ReadUInt32(), br.ReadUInt32()));
        }


        StringBuilder biglist = new StringBuilder();

        public void Export()
        {
            int i = 0;

            Directory.CreateDirectory(extpath);
            Console.Write(pairs.Count + " files:\r\n");

            foreach (Pair p in pairs)
            {
                //detect known formats

                br.BaseStream.Position = p.offset * 2048;

                uint h = br.ReadUInt32();
                uint h2 = br.ReadUInt32();

                string knownext = "";

                switch (h)
                {
                    case 0x00000010: if (h2 == 2) knownext = ".tim"; break;
                    case 0x00000020: knownext = ".vram"; break;
                    case 0x80010160: knownext = ".str"; break;
                    //default: knownext = ""; break;
                }

                if (p.size == 0) knownext = ".null";

                //--------------

                br.BaseStream.Position = p.offset * 2048;

                string knownname = "";

                if (names.ContainsKey(i))
                    knownname = names[i];

                
                string fname = 
                    extpath +
                    i.ToString("0000") + 
                    (knownname !="" ? ("_" + knownname) : "") + 
                    knownext;

                /*
                biglist.Append(i.ToString("0000") +
                    (knownname != "" ? ("_" + knownname) : "") +
                    knownext + "\r\n");
                */


                if (knownname == "") knownname = i.ToString("0000") + knownext;

                if (!knownname.Contains("\\")) knownname = i.ToString("0000") + "_" + knownname;

                biglist.Append(knownname + "\r\n");

                fname = Path.Combine(extpath, knownname);


                if (p.size > 0)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fname));
                    File.WriteAllBytes(fname, br.ReadBytes((int)p.size));
                }

                Console.Write(".");

                i++;

                //Console.WriteLine(fname);
            }

            File.WriteAllText(bigname + ".txt", biglist.ToString());

            Console.Write("\r\n");

        }


        public void Build(string txt)
        {
            string path = Path.GetFileNameWithoutExtension(txt);
            string[] files = File.ReadAllLines(txt);


            Console.WriteLine(path);

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = ".\\" + path + "\\" + files[i];
                ctrfiles.Add(new CTRFile(files[i]));
            }

            Console.WriteLine("we'll need " + TotalSize() + " Bytes for " + ctrfiles.Count + " files:");

            byte[] final_big = new byte[TotalSize()];
            MemoryStream ms = new MemoryStream(final_big);
            BinaryWriter bw = new BinaryWriter(ms);

            bw.BaseStream.Position = 4;
            bw.Write(ctrfiles.Count);

            bw.BaseStream.Position = 3 * 2048;

            foreach (CTRFile c in ctrfiles)
            {
                Console.Write(".");

                uint pos = (uint)bw.BaseStream.Position;
                c.offset = pos / 2048;


                bw.Write(c.data);

                bw.BaseStream.Position = pos + c.padded_size;
            }

            Console.WriteLine();

            bw.BaseStream.Position = 8;

            foreach (CTRFile c in ctrfiles)
            {
                bw.Write(c.offset);
                bw.Write(c.size);
            }

            Console.WriteLine(p.disk_dump);

            File.WriteAllBytes(path + ".BIG", final_big);

            Console.WriteLine(p.big_created);

        }



        public uint TotalSize()
        {
            //let's hardcode it as you can't add files anyway.
            //ideally you should calculate the amount of sectors used for size/offset array
            uint to_alloc = 2048 * 3;

            foreach (CTRFile c in ctrfiles)
                to_alloc += c.padded_size;

            return to_alloc;
        }

        private bool LoadNames(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string[] buf = File.ReadAllLines(path);

                    foreach (string b in buf)
                    {
                        if (b.Trim() != "")
                        {
                            string[] bb = b.Replace(" ", "").Split('=');

                            int x = -1;
                            Int32.TryParse(bb[0], out x);

                            if (x == -1)
                            {
                                Console.WriteLine("List parsing error at: {0}", bb);
                                continue;
                            }

                            names.Add(x, bb[1]);
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}