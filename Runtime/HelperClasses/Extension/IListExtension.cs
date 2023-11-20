using System.Collections;

namespace CommonBase
{
    public static class IListExtension
    {
        public static bool ValidIndex(this IList list, int index)
        {
            if (list == null)
            {
                return false;
            }
            return list.Count > index && index >= 0;
        }

        public static bool IsNullOrEmpty(this IList list)
        {
            return list == null || list.Count == 0;
        }

        public static bool NotLastIndex(this int index, IList list)
        {
            if (list == null)
            {
                return false;
            }
            return list.Count - 1 > index;
        }

        public static void CircularAdvance()
        {

        }
    }
}
