using System.Collections.Generic;

namespace PangLib.PET.DataModels
{
    public class PolygonIndex
    {
        public uint Index;
        public float X;
        public float Y;
        public float Z;
        public List<UVMapping> UVMappings = new List<UVMapping>();
    }
}
