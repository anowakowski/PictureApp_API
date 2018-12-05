using System;

namespace PictureApp.API.Extensions
{
    public class SystemGuid
    {
        private static readonly Func<Guid> Default = Guid.NewGuid;

        private static Func<Guid> _current = Default;

        public static Guid NewGuid()
        {
            return _current();
        }

        public static void Set(Func<Guid> newGuidFactory)
        {
            _current = newGuidFactory;
        }

        public static void Reset()
        {
            _current = Default;
        }
    }
}
