using CommonServiceLocator;

namespace DriveExplorer.IoC
{
	public interface IIocContainer : IServiceLocator
	{
		/// <summary>
			/// Checks whether at least one instance of a given class is already created in the container.
			/// </summary>
			/// <typeparam name="TClass">The class that is queried.</typeparam>
			/// <returns>True if at least on instance of the class is already created, false otherwise.</returns>
		bool IsCreated<TClass>()
			where TClass : class;

		/// <summary>
			/// Gets a value indicating whether a given type T is already registered.
			/// </summary>
			/// <typeparam name="TClass">The type that the method checks for.</typeparam>
			/// <returns>True if the type is registered, false otherwise.</returns>
		bool IsRegistered<TClass>()
			where TClass : class;

		/// <summary>
			/// Registers a given type for a given interface.
			/// </summary>
			/// <typeparam name="TInterface">The interface for which instances will be resolved.</typeparam>
			/// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
		void Register<TInterface, TClass>()
			where TInterface : class
			where TClass : class, TInterface;

		/// <summary>
			/// Registers a given type.
			/// </summary>
			/// <typeparam name="TClass">The type that must be used to create instances.</typeparam>
		void Register<TClass>()
			where TClass : class;

		/// <summary>
			/// Resets the instance in its original states. This deletes all the
			/// registrations.
			/// </summary>
		void Reset();

		/// <summary>
			/// Unregisters a class from the cache and removes all the previously
			/// created instances.
			/// </summary>
			/// <typeparam name="TClass">The class that must be removed.</typeparam>
		void Unregister<TClass>()
			where TClass : class;

	}
}