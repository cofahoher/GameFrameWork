using System;
using System.Collections;
using System.Collections.Generic;

namespace BaseUtil
{
    public class Container
    {

        public static List<T> random_shuffle<T>(List<T> ori)
        {
            List<T> newList = new List<T>();
            var random = new Random();

            foreach (var act in ori)
            {
                newList.Insert(random.Next(newList.Count), act);
            }
            return newList;
        }
    }
}
