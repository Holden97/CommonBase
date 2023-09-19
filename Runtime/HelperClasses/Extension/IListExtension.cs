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
    }
}
