using DriveExplorer.MicrosoftApi;
using Microsoft.Graph;
using System;
using System.Linq;
using Xunit;

namespace DriveExplorer.IoC {
	public class IocContainerTests : IDisposable {
		private readonly IocContainer ioc;

		public IocContainerTests() {
			ioc = new IocContainer();
		}

		public void Dispose() {
			ioc.Reset();
		}

		[Fact]
		public void GetMainWindowVM() {
			ioc.Register<IAuthenticationProvider>(() => AuthProvider.Default);
			ioc.Register<GraphManager>();
			ioc.Register<MainWindowVM>();
			var authProvider = ioc.GetSingleton<IAuthenticationProvider>();
			var graphManager = ioc.GetSingleton<GraphManager>();
			var mainWindowVM = ioc.GetSingleton<MainWindowVM>();
			Assert.NotNull(authProvider);
			Assert.NotNull(graphManager);
			Assert.NotNull(mainWindowVM);
		}

		[Fact]
		public void GetTrasient() {
			ioc.Register<IAuthenticationProvider>(() => AuthProvider.Default);
			ioc.Register<GraphManager>();
			var graph1 = ioc.GetTransient<GraphManager>();
			var graph2 = ioc.GetTransient<GraphManager>();
			Assert.NotEqual(graph1, graph2);
		}

		[Fact]
		public void GetDefaultIocTwice_IdenticalInstances() {
			//Given
			var expected = IocContainer.Default;
			//When
			var actual = IocContainer.Default;
			//Then
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Reset_IsRegister_False() {
			//Given

			ioc.Register<MyClass>();
			//When
			ioc.Reset();
			//Then
			var condition = ioc.IsRegistered<MyClass>();
			Assert.False(condition);
		}

		[Fact]
		public void DoRegisterClass_IsRegister_True() {
			//Given

			ioc.Register<MyClass>();
			//When
			var conditionA = ioc.IsRegistered<MyClass>();
			//Then
			Assert.True(conditionA);
		}

		[Fact]
		public void NoRegisterClass_IsRegister_False() {
			//Given

			//When
			var conditionB = ioc.IsRegistered<MyClass>();
			//Then
			Assert.False(conditionB);
		}

		[Fact]
		public void NoRegisterClass_GetInstance_Throws() {
			//Given

			//When
			Func<MyClass> func = ioc.GetInstance<MyClass>;
			//Then
			Assert.Throws<InvalidOperationException>(func);
		}

		[Fact]
		public void DoGetInstanceTwice_IdenticalInstances() {
			//Given

			ioc.Register<MyClass>();
			var instanceA = ioc.GetInstance<MyClass>();
			//When
			var instanceB = ioc.GetInstance<MyClass>();
			//Then
			Assert.Equal(instanceA, instanceB);
		}

		[Fact]
		public void RegisterDependedClass_GetDependentClassInstance_ResultNotNull() {
			//Given

			ioc.Register<DependedClass>();
			ioc.Register<DependentClass>();
			//When
			var instance = ioc.GetInstance<DependentClass>();
			//Then
			Assert.NotNull(instance);
		}

		[Fact]
		public void DoGetInstanceClass_IsCreated_True() {
			//Given

			ioc.Register<MyClass>();
			var instanceA = ioc.GetInstance<MyClass>();
			//When
			var conditionA = ioc.IsCreated<MyClass>();
			//Then
			Assert.True(conditionA);
		}

		[Fact]
		public void NoGetInstanceClass_IsCreated_False() {
			//Given

			ioc.Register<MyClass>();

			//When
			var conditionB = ioc.IsCreated<MyClass>();

			//Then
			Assert.False(conditionB);
		}

		[Fact]
		public void DoRegisterInterface_IsRegistered_True() {
			//Given

			ioc.Register<IClass, MyClass>();
			//When
			var condition = ioc.IsRegistered<IClass>();
			//Then
			Assert.True(condition);
		}

		[Fact]
		public void NoRegisterInterface_IsRegistered_False() {
			//Given

			//When
			var condition = ioc.IsRegistered<IClass>();
			//Then
			Assert.False(condition);
		}

		[Fact]
		public void DoGetInstanceInterface_IsCreated_True() {
			//Given

			ioc.Register<IClass, MyClass>();
			var instance = ioc.GetInstance<IClass>();
			//When
			var condition = ioc.IsCreated<IClass>();
			//Then
			Assert.True(condition);
		}

		[Fact]
		public void NoGetInstanceInterface_IsCreated_False() {
			//Given

			ioc.Register<IClass, MyClass>();
			//When
			var condition = ioc.IsCreated<IClass>();
			//Then
			Assert.False(condition);
		}

		[Fact]
		public void DoRegisterInterfaceWithClass_GetInstance_InstanceIsClass() {
			//Given

			ioc.Register<IClass, MyClass>();
			//When
			var instance = ioc.GetInstance<IClass>();
			//Then
			Assert.True(instance is MyClass);
		}

		[Fact]
		public void RegisterInterfaceWithRegisterClass_Throws() {
			//Given

			//When
			Action action = ioc.Register<IClass>;
			//Then
			Assert.Throws<ArgumentException>(action);
		}

		[Fact]
		public void UnregisterClass_IsRegistered_False() {
			//Given

			ioc.Register<MyClass>();
			ioc.Unregister<MyClass>();
			//When
			var condition = ioc.IsRegistered<MyClass>();
			//Then
			Assert.False(condition);
		}

		[Fact]
		public void UnregisterInterface_IsRegistered_False() {
			//Given

			ioc.Register<IClass, MyClass>();
			ioc.Unregister<IClass>();
			//When
			var condition = ioc.IsRegistered<IClass>();
			//Then
			Assert.False(condition);
		}

		[Fact]
		public void RegisterClassWithoutPublicCtor_Throws() {
			//Given

			//When
			Action action = ioc.Register<ClassWithoutPublicCtor>;
			//Then
			Assert.Throws<InvalidOperationException>(action);
		}


	}

	public class DependedClass {
		public DependedClass() { }
	}

	public class DependentClass {
		public DependentClass() { }

		[PreferredConstructor]
		public DependentClass(DependedClass c) { }
	}
	public interface IClass {

	}

	public class MyClass : IClass {
		public MyClass() { }
	}

	public class ClassWithoutPublicCtor : IClass {
		private ClassWithoutPublicCtor() { }
	}
}