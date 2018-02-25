using System;
using System.ComponentModel.DataAnnotations;
using PQT.Domain.Helpers;
using NS.Entity;

namespace PQT.Domain
{
    public abstract class Entity : NS.Entity.Entity, IEquatable<Entity>
    {
        protected Entity()
        {
            this.CreatedTime = DateTime.Now;
        }
        [Key]
        public int ID { get; set; }

        #region IEquatable<Entity> Members

        public bool Equals(Entity other)
        {
            return ID != 0 && ID == other.ID;
        }

        #endregion

        #region Comparison

        public override int GetHashCode()
        {
            return ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return GetType().IsInstanceOfType(obj) &&
                   Equals((Entity) obj);
        }

        #endregion
    }
}
