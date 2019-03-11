using System.Collections.Generic;
using System.Linq;

namespace PictureApp.API.Providers
{
    public class ComputedPassword
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == GetType() && Equals(this, (ComputedPassword) obj);
        }

        public byte[] Hash { get; }

        public byte[] Salt { get; }

        public static ComputedPassword Create(byte[] hash, byte[] salt)
        {
            return new ComputedPassword(hash, salt);
        }

        public static bool operator ==(ComputedPassword lhs, ComputedPassword rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator !=(ComputedPassword lhs, ComputedPassword rhs)
        {
            return !Equals(lhs, rhs);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Hash != null ? Hash.GetHashCode() : 0) * 397) ^ (Salt != null ? Salt.GetHashCode() : 0);
            }
        }

        protected static bool Equals(ComputedPassword lhs, ComputedPassword rhs)
        {
            if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs)) return false;
            if (ReferenceEquals(null, lhs)) return false;
            if (ReferenceEquals(null, rhs)) return false;
            if (ReferenceEquals(rhs, lhs)) return true;

            return !lhs.Hash.Where((t, i) => t != rhs.Hash[i]).Any() && !lhs.Salt.Where((t, i) => t != rhs.Salt[i]).Any();
        }

        private ComputedPassword(byte[] hash, byte[] salt)
        {
            Hash = hash;
            Salt = salt;
        }
    }
}