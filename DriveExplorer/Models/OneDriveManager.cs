﻿using Cyc.MicrosoftApi;
using Cyc.Standard;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public class OneDriveManager : IDriveManager {
		private readonly ILogger logger;
		private readonly ItemFactoryBase factory;
		private readonly MicrosoftApiManager microsoftManager;


		public event EventHandler<IItem> LoginCompleted;
		public event EventHandler<IItem> LogoutCompleted;
		public event EventHandler BeforeTaskExecuted {
			add => microsoftManager.BeforeTaskExecuted += value;
			remove => microsoftManager.BeforeTaskExecuted -= value;
		}
		public event EventHandler TaskExecuted {
			add => microsoftManager.TaskExecuted += value;
			remove => microsoftManager.TaskExecuted -= value;
		}

		public OneDriveManager(ILogger logger, MicrosoftApiManager microsoftManager, OneDriveItem.Factory factory) {
			this.logger = logger;
			this.microsoftManager = microsoftManager;
			this.factory = factory;
		}

		public async Task LoginAsync() {
			if (microsoftManager == null) {
				return;
			}
			var result = await microsoftManager.LoginInteractively().ConfigureAwait(true);
			await CreateOneDriveAsync(result?.Account).ConfigureAwait(false);
		}
		/// <summary>
		/// TODO: not auto login at launch, just retrieve account cache, and login when user want to access the drive
		/// </summary>
		/// <returns></returns>
		public async Task AutoLoginAsync() {
			if (microsoftManager == null) {
				return;
			}
			await foreach (var result in microsoftManager.LoginAllUserSilently().ConfigureAwait(true)) {
				await CreateOneDriveAsync(result?.Account).ConfigureAwait(true);
			}
		}
		public async Task LogoutAsync(IItem item) {
			if (microsoftManager == null) {
				return;
			}
			if (!(item is OneDriveItem oneDriveItem)) {
				throw new InvalidOperationException();
			}
			if (await microsoftManager.LogoutAsync(oneDriveItem.UserAccount).ConfigureAwait(true)) {
				LogoutCompleted.Invoke(this, oneDriveItem);
			}
		}
		private async Task CreateOneDriveAsync(IAccount account) {
			if (microsoftManager == null) {
				return;
			}
			if (account is null) {
				return;
			}
			var root = await microsoftManager.GetDriveRootAsync(account).ConfigureAwait(true);
			if (root == null) {
				return;
			}
			var item = factory.CreateRoot(microsoftManager, root, account);
			LoginCompleted?.Invoke(this, item);
		}

	}
}
