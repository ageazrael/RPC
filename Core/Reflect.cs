using System;
using System.Reflection;

namespace BlackHole.Core
{
    public static class Reflect
    {
        public static object Construct(Type rType, object[] args)
        {
            var rArgTypes = new Type[args.Length];
            for (var nIndex = 0; nIndex < args.Length; ++nIndex)
                rArgTypes[nIndex] = args[nIndex].GetType();

            var rConstructr = rType.GetConstructor(BindingFlags.Public|BindingFlags.Instance, null, rArgTypes, null);
            if (null != rConstructr)
                return rConstructr.Invoke(args);
            else
                return null;
        }
        public static T Construct<T>(params object[] args)
        {
            return (T)Construct(typeof(T), args);
        }
        public static ResultType TConstruct<ResultType>(Type rRealType, params object[] args)
        {
            return (ResultType)Construct(rRealType, args);
        }
    }
}
