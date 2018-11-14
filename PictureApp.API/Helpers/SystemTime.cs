using System;

namespace PictureApp.API.Helpers
{
    public class SystemTime
    {
        private static readonly Func<DateTime> Default = () => DateTime.UtcNow;

        private static Func<DateTime> _current = Default;

        public static DateTime Now()
        {
            return _current();
        }

        public static void Set(Func<DateTime> newTimeFactory)
        {
            _current = newTimeFactory;
        }

        public static void Reset()
        {
            _current = Default;
        }
    }
}
