using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldenTicket.Lottery
{
    /**
     * <summary>
     * Extension methods for IList<T> objects to allow for random shuffling of their elements.
     * Fisher-Yates algorithm implementation from http://stackoverflow.com/a/22668974/249016
     * </summary>
     */
    public static class ListShuffle
    {
        /**
         * <summary>Randomly shuffles the order of elements in a list</summary>
         * <param name="list">List to shuffle</param>
         * <param name="rnd">Random number generator</param>
         */
        public static void Shuffle<T>(this IList<T> list, Random rnd)
        {
            for (var i = 0; i < list.Count; i++)
                list.Swap(i, rnd.Next(i, list.Count));
        }

        /**
         * <summary>Swap two elements order in the list</summary>
         * <param name="list">List to swap elements in</param>
         * <param name="i">First element to swap</param>
         * <param name="j">Second element to swap</param>
         */
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}