﻿using Xunit;
using DriveExplorer.MicrosoftApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace DriveExplorer.MicrosoftApi.Tests {
	public class GraphManagerTestFixture {
		private readonly AuthProvider authProvider;
		public readonly GraphManager graphManager;

		public GraphManagerTestFixture() {
			var services = new ServiceCollection();
			services.AddSingleton<ILogger, DebugLogger>();
			services.AddSingleton(sp => new AuthProvider(sp.GetService<ILogger>(), AuthProvider.Authority.Organizations));
			services.AddSingleton<GraphManager>();
			var serviceProvider = services.BuildServiceProvider();
			authProvider = serviceProvider.GetService<AuthProvider>();
			graphManager = serviceProvider.GetService<GraphManager>();
			authProvider.GetAccessTokenWithUsernamePassword().Wait();
		}
	}
	public class GraphManagerTests : IClassFixture<GraphManagerTestFixture> {
		private readonly GraphManager graphManager;
		private readonly ITestOutputHelper logger;

		public GraphManagerTests(GraphManagerTestFixture fixture, ITestOutputHelper logger) {
			graphManager = fixture.graphManager;
			this.logger = logger;
		}
		[Fact()]
		public void GraphManagerTest() {
			Assert.NotNull(graphManager);
		}

		[Fact()]
		public async Task GetMeAsyncTestAsync() {
			var user = await graphManager.GetMeAsync().ConfigureAwait(false);
			Assert.NotNull(user);
		}

		[Fact()]
		public void GetDriveRootAsyncTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void SearchDriveAsyncTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void GetFileAsyncTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void GetChildrenAsyncTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void UploadFileAsyncTest() {
			throw new NotImplementedException();
		}

		[Fact()]
		public void UpdateFileAsyncTest() {
			throw new NotImplementedException();
		}
	}
}