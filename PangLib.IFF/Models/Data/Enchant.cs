﻿using PangLib.IFF.Models.General;
using System;
using System.Runtime.InteropServices;
namespace PangLib.IFF.Models.Data
{
    /// <summary>
    /// Is Struct file Enchant.iff
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class Enchant
    {
        public uint Enable { get; set; }
        public uint TypeID { get; set; }
        public long Pang { get; set; }
    }
}
