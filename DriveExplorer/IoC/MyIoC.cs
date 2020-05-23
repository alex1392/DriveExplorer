using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;

namespace DriveExplorer.IoC {
	public class MyIoC : IIocContainer {
		#region Private members
		private readonly Dictionary<Type, Type> interfaceRegistry = new Dictionary<Type, Type>();
		private readonly Dictionary<Type, ConstructorInfo> constructorRegisty = new Dictionary<Type, ConstructorInfo>();
		private readonly Dictionary<Type, object> instanceRegistry = new Dictionary<Type, object>();
		#endregion

		private static readonly Lazy<MyIoC> @default = new Lazy<MyIoC>(() => new MyIoC());
		public static MyIoC Default => @default.Value;

		/// <summary>
		/// Check if there is an instance of given type created.
		/// </summary>
		/// <typeparam name="T">TClass or TInterface</typeparam>
		/// <returns></returns>
		public bool IsCreated<T>()
		where T : class {
			return IsCreated(typeof(T));
		}

		/// <summary>
		/// Check if given TClass or TInterface is registered.
		/// </summary>
		/// <typeparam name="T">TClass or TInterface</typeparam>
		/// <returns></returns>
		public bool IsRegistered<T>()
		where T : class {
			return IsRegistered(typeof(T));
		}

		public void Register<TClass>()
		where TClass : class {
			Register(typeof(TClass));
		}

		public void Register(Type TClass) {
			if (TClass.IsInterface) {
				throw new ArgumentException($"Interface {TClass} cannot be registered alone, use Register<TInterface, TClass>() instead.");
			}
			if (IsRegistered(TClass)) {
				throw new InvalidOperationException($"{TClass} has already been registered.");
			}
			constructorRegisty.Add(TClass, GetConstructor(TClass));
		}

		public void Register<TInterface, TClass>()
		where TInterface : class
		where TClass : class, TInterface {
			Register(typeof(TInterface), typeof(TClass));
		}

		public void Register(Type TInterface, Type TClass) {
			if (!TInterface.IsInterface || TClass.IsInterface) {
				throw new ArgumentException($"Input types are not valid, {TInterface.Name} should be an interface, {TClass} should be a class.");
			}
			if (IsRegistered(TInterface) || IsRegistered(TClass)) {
				throw new InvalidOperationException($"{TInterface.Name} has already been registered.");
			}

			interfaceRegistry.Add(TInterface, TClass);
			Register(TClass);
		}

		public void Reset() {
			interfaceRegistry.Clear();
			instanceRegistry.Clear();
			constructorRegisty.Clear();
		}

		public void Unregister<T>()
		where T : class {
			var type = typeof(T);
			if (type.IsInterface) {
				interfaceRegistry.Remove(type, out var instanceType);
				constructorRegisty.Remove(instanceType);
				instanceRegistry.Remove(instanceType);
			} else {
				constructorRegisty.Remove(type);
				instanceRegistry.Remove(type);
			}
		}

		public TClass GetSingleton<TClass>() where TClass : class {
			return GetInstance<TClass>();
		}

		public TClass GetSingleton<TClass>(string key) where TClass : class {
			return GetSingleton<TClass>();
		}

		public TClass GetTransient<TClass>() where TClass : class {
			var type = RegisterTypeCheck(typeof(TClass));
			if (type == null) {
				throw new InvalidOperationException("Given type has not been registered.");
			}
			return (TClass)CreateInstance(type);
		}

		public object GetService(Type serviceType) {
			var type = RegisterTypeCheck(serviceType);
			if (type == null) {
				throw new InvalidOperationException("Given type has not been registered.");
			}
			if (IsCreated(type)) {
				return instanceRegistry[type];
			}
			var instance = CreateInstance(type);
			instanceRegistry.Add(type, instance);
			return instance;
		}

		public object GetInstance(Type serviceType) {
			return GetService(serviceType);
		}

		public TService GetInstance<TService>() {
			return (TService) GetService(typeof(TService));
		}

		public object GetInstance(Type serviceType, string key) {
			return GetService(serviceType);
		}

		public TService GetInstance<TService>(string key) {
			return GetInstance<TService>();
		}

		public IEnumerable<object> GetAllInstances(Type serviceType) {
			return new List<object>() {
				GetService(serviceType)
			};
		}

		public IEnumerable<TService> GetAllInstances<TService>() {
			return new List<TService>() {
				GetInstance<TService>(),
			};
		}

		private bool IsCreated(Type type) {
			var resolvedType = RegisterTypeCheck(type);
			return resolvedType != null && instanceRegistry.ContainsKey(resolvedType);
		}

		private bool IsRegistered(Type type) {
			return RegisterTypeCheck(type) != null;
		}

		private Type RegisterTypeCheck(Type type) {
			if (!type.IsInterface && constructorRegisty.ContainsKey(type)) {
				return type;
			}
			if (type.IsInterface && interfaceRegistry.ContainsKey(type)) {
				return interfaceRegistry[type];
			}
			return null;
		}

		private ConstructorInfo GetConstructor(Type type) {
			var resolvedType = RegisterTypeCheck(type);
			var constructors = type.GetConstructors().Where(
				c => c.IsPublic && !c.IsStatic);
			if (constructors.Count() == 0) {
				throw new ArgumentException("No public non-static constructor found in given class.");
			} else if (constructors.Count() == 1) {
				return constructors.FirstOrDefault();
			}
			// if there are mupltiple public constructors
			// get preferred constructor
			var preferred = constructors.FirstOrDefault(
				c => Attribute.GetCustomAttribute(c, typeof(PreferredConstructorAttribute)) != null);
			if (preferred != null) {
				return preferred;
			}
			return constructors.FirstOrDefault();
		}

		private object CreateInstance(Type type) {
			var constructor = constructorRegisty[type];
			var parameterInfos = constructor.GetParameters();
			var parameters = new object[parameterInfos.Length];
			foreach (var paramInfo in parameterInfos) {
				parameters[paramInfo.Position] = GetService(paramInfo.ParameterType);
			}
			return constructor.Invoke(parameters);
		}

		
	}
}