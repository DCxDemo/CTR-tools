﻿using CTRFramework.Shared;
using CTRFramework.Vram;
using System;
using System.Collections.Generic;

namespace CTRFramework
{
    public class CtrTex
    {
        public TextureLayout[] midlods = new TextureLayout[3];
        public TextureLayout[] hi = new TextureLayout[16];
        public List<TextureLayout> animframes = new List<TextureLayout>(); //this actually has several lods too

        public uint ptrHi;
        public bool isAnimated = false;


        public CtrTex(BinaryReaderEx br, int mosaic)
        {
            Read(br, mosaic);
        }

        public void Read(BinaryReaderEx br, int mosaic)
        {
            int pos = (int)br.BaseStream.Position;

            if ((pos & 2) > 0)
            {
                Console.WriteLine("!!!");
                Console.ReadKey();
            }

            //this apparently defines animated texture, really
            if ((pos & 1) == 1)
            {
                isAnimated = true;

                br.BaseStream.Position -= 1;

                uint texpos = br.ReadUInt32();
                int numFrames = br.ReadInt16();
                int whatsthat = br.ReadInt16();

                if (whatsthat != 0)
                    Helpers.Panic(this, PanicType.Assume, $"whatsthat is not null! {whatsthat}");

                if (br.ReadUInt32() != 0)
                    Helpers.Panic(this, PanicType.Assume, "not 0!");

                uint[] ptrs = br.ReadArrayUInt32(numFrames);

                foreach (uint ptr in ptrs)
                {
                    br.Jump(ptr);
                    animframes.Add(TextureLayout.FromStream(br));
                }

                br.Jump(texpos);
            }

            for (int i = 0; i < 3; i++)
                midlods[i] = TextureLayout.FromStream(br);

            //Console.WriteLine(br.BaseStream.Position.ToString("X8"));
            //Console.ReadKey();

            //if (mosaic != 0)
            {
                ptrHi = br.ReadUInt32();

                if (Scene.ReadHiTex)
                    //loosely assume we got a valid pointer
                    if (ptrHi > 0x30000 && ptrHi < 0xB0000)
                    {
                        br.Jump(ptrHi);

                        for (int i = 0; i < 16; i++)
                            hi[i] = TextureLayout.FromStream(br);
                    }
            }
        }
    }
}