using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public interface ISingleton
    {
        void DestroySingleton();
    }

    public abstract class Singleton<T> : ISingleton, IDestruct where T : ISingleton, IDestruct, new()
    {
        static T ms_instance;

        public static T Instance
        {
            get
            {
                if (ms_instance == null)
                {
                    ms_instance = new T();
                    SingletonManager.AddSingleton(ms_instance);
                }
                return ms_instance;
            }
        }

        public void DestroySingleton()
        {
            if (ms_instance != null)
                ms_instance.Destruct();
            ms_instance = default(T);
        }

        public abstract void Destruct();
    }

    public class SingletonManager
    {
        static List<ISingleton> ms_all_singletons = new List<ISingleton>();

        public static void AddSingleton(ISingleton singleton)
        {
            ms_all_singletons.Add(singleton);
        }

        public static void DestroyAllSingletons()
        {
            for (int i = 0; i < ms_all_singletons.Count; ++i)
                ms_all_singletons[i].DestroySingleton();
            ms_all_singletons.Clear();
        }
    }

    class TestSingleton : Singleton<TestSingleton>
    {
        public override void Destruct()
        {
        }
    }
}