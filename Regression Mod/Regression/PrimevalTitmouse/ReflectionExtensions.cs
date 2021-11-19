using System.Reflection;

namespace PrimevalTitmouse
{
  public static class ReflectionExtensions
  {
    public static T GetField<T>(this object o, string fieldName) where T : class
    {
      FieldInfo field = o.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      object obj1;
      if (field == (FieldInfo) null)
      {
        obj1 = (object) null;
      }
      else
      {
        object obj2 = o;
        obj1 = field.GetValue(obj2);
      }
      return obj1 as T;
    }
  }
}
