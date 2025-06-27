using System;
using System.Diagnostics;

namespace DG.XrmPluginSync.Model
{
    [DebuggerDisplay("{EntityTypeName} {Name} ({Id})")]
    public abstract class EntityBase
    {
        protected EntityBase(string? entityTypeName)
        {
            EntityTypeName = entityTypeName;
        }

        public Guid? Id { get; set; }

        public required string Name { get; set; }

        public string? EntityTypeName { get; set; }
    }
}
