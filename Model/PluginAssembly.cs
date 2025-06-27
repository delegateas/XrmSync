using System.Collections.Generic;

namespace DG.XrmPluginSync.Model
{
    public class PluginAssembly() : EntityBase("")
    {
        public required string Version { get; set; }
        public required string DllPath { get; set; }
        public required string Hash { get; set; }
        public List<PluginTypeEntity> PluginTypes { get; set; } = [];

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            else if (this is null)
            {
                return false;
            }
            else if (obj is null)
            {
                return false;
            }
            else if (GetType() != obj.GetType())
            {
                return false;
            }

            if (obj is not PluginAssembly assembly)
            {
                return false;
            }


            return Hash == assembly.Hash;
        }

        public override int GetHashCode()
        {
            return (Hash?.GetHashCode()) ?? 0;
        }
    }
}
