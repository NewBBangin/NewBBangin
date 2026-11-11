using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
   private static T _instance;

   public static T instance
   {
      get
      {
         if (_instance == null)
         {
            _instance = (T)GameObject.FindAnyObjectByType<T>();

            if (_instance == null)
            {
               GameObject go = new GameObject(typeof(T).Name);
               _instance = go.AddComponent<T>();
               DontDestroyOnLoad(_instance.gameObject);
            }
         }
         return _instance;
      }
   }
}
