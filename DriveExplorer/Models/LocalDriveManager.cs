﻿using Cyc.Standard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public class LocalDriveManager : IDriveManager {
		private readonly ILogger logger;

		public event EventHandler<IItem> LoginCompleted;
		public event EventHandler<IItem> LogoutCompleted;
		public event EventHandler BeforeTaskExecuted;
		public event EventHandler TaskExecuted;

		public LocalDriveManager(ILogger logger)
		{
			this.logger = logger;
		}
		public Task AutoLoginAsync()
		{
			string[] drivePaths = null;
			try {
				drivePaths = Directory.GetLogicalDrives();
			} catch (UnauthorizedAccessException ex) {
				logger?.Log(ex);
			}
			foreach (var drivePath in drivePaths) {
				var item = new LocalItem(drivePath);
				LoginCompleted?.Invoke(this, item);
			}
			return Task.CompletedTask;
		}

		[Obsolete]
		public Task LoginAsync()
		{
			throw new NotImplementedException();
		}
		[Obsolete]
		public Task LogoutAsync(IItem item)
		{
			throw new NotImplementedException();
		}
	}
}
