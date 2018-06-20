using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackHole.Core
{
    public interface IInterface { }
    public interface IInterfaceFactory
    {
        IInterface Create();
    }
    public class DefaultInterfaceFactory<T> : IInterfaceFactory
        where T : IInterface
    {
        IInterface IInterfaceFactory.Create()
        {
            return Reflect.Construct<T>();
        }
    }
    public static class ImplementContainer
    {
        public static bool InitializeInstance<T>(IInterface rInterface)
            where T : IInterface
        {
            return InitializeInstance(typeof(T), rInterface);
        }
        public static void UninitializeInstance<T>()
        {
            UninitializeInstance(typeof(T));
        }
        public static T GetInstance<T>()
            where T : IInterface
        {
            return (T)GetInstance(typeof(T));
        }


        public static T Construct<T>()
            where T : IInterface
        {
            return (T)Construct(typeof(T));
        }
        public static bool Implement<T>(IInterfaceFactory rFactory)
            where T : IInterface
        {
            return Implement(typeof(T), rFactory);
        }
        public static bool ImplementDefault<InterfaceType, ImplementType>()
            where InterfaceType : IInterface
            where ImplementType : IInterface
        {
            return Implement(typeof(InterfaceType), new DefaultInterfaceFactory<ImplementType>());
        }


        public static bool InitializeInstance(Type rInterfaceType, IInterface rInterface)
        {
            if (!rInterfaceType.IsInterface)
                return false;

            if (Instances.ContainsKey(rInterfaceType))
                return false;

            Instances[rInterfaceType] = rInterface;
            return true;
        }
        public static void UninitializeInstance(Type rInterfaceType)
        {
            Instances.Remove(rInterfaceType);
        }
        public static IInterface GetInstance(Type rInterfaceType)
        {
            var rInterface = default(IInterface);
            Instances.TryGetValue(rInterfaceType, out rInterface);
            return rInterface;
        }
        public static bool Implement(Type rType, IInterfaceFactory rFactory)
        {
            if (!rType.IsInterface)
                return false;

            Factory[rType] = rFactory;
            return true;
        }
        public static IInterface Construct(Type rType)
        {
            if (!rType.IsInterface)
                return null;

            if (!Factory.TryGetValue(rType, out var rFactor))
                return null;

            return rFactor.Create();
        }

        static Dictionary<Type, IInterfaceFactory>  Factory     = new Dictionary<Type, IInterfaceFactory>();
        static Dictionary<Type, IInterface>         Instances   = new Dictionary<Type, IInterface>();
    }
}
