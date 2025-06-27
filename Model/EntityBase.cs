using System;

namespace DG.XrmPluginSync.Model
{
    public abstract class EntityBase
    {
        protected EntityBase()
        {
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
