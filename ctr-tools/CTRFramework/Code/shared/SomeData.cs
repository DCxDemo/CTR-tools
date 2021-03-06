﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CTRFramework.Shared
{
    public class SomeData : IReadWrite
    {
        short s1;
        short s2;

        short[] data;

        public void Read(BinaryReaderEx br)
        {
            s1 = br.ReadInt16();
            s2 = br.ReadInt16();

            data = new short[4];

            for (int i = 0; i < 4; i++)
                data[i] = br.ReadInt16();
        }

        public void Write(BinaryWriterEx bw, List<UIntPtr> patchTable = null)
        {
            bw.Write(s1);
            bw.Write(s2);

            foreach (short s in data)
                bw.Write(s);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(s1 + " " + s2 + " ");

            foreach (short s in data)
                sb.Append(s + " ");

            return sb.ToString();
        }
    }

}
