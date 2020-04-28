﻿namespace CTRFramework
{
    enum Flags
    {
        s = 1 << 7,
        l = 1 << 6,
        b1 = 1 << 5,
        b2 = 1 << 4,
        k = 1 << 3,
        v = 1 << 2,
        b3 = 1 << 1,
        b4 = 1 << 0
    }

    class MshCommand
    {
        public uint value;

        public byte flagsval { get { return (byte)(value >> (8 * 3) & 0xFF); } }

        public Flags flags { get { return (Flags)flagsval; } } 

        public byte stackIndex { get { return (byte)(value >> 16 & 0xFF); } }
        public byte colorIndex { get { return (byte)(value >> 9 & 0x7F); } }
        //public byte texIndex { get { return (byte)(value & 0x1FF); } }

        public MshCommand(uint x)
        {
            value = x;
            //Console.WriteLine(ToString());
        }

        public override string ToString()
        {
            return 
                value.ToString("X8") + "\t" +
                flagsval + "\t" +
                stackIndex + "\t" +
                colorIndex + "\t";
        }
    }
}
