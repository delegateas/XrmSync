using System.Collections.Generic;

namespace DG.XrmPluginSync.Model
{
    public class PluginAssembly : EntityBase
    {
        public string Version { get; set; }
        public string DllPath { get; set; }
        public string Hash { get; set; }
        public List<PluginTypeEntity> PluginTypes { get; set; } = new List<PluginTypeEntity>();

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

            var assembly = obj as PluginAssembly;
            if (obj is null)
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
