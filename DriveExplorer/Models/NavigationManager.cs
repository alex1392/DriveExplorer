using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriveExplorer.Models {
	public class NavigationManager<T> where T : class {
		private readonly List<T> list = new List<T>();
		private readonly int capacity;
		private int index = -1;
		private bool AddLock = false;

		public event EventHandler CanGoPreviousChanged;
		public event EventHandler CanGoNextChanged;
		public event EventHandler CurrentChanged;
		public bool CanGoPrevious => Index > 0;
		public bool CanGoNext => Index < list.Count - 1;
		public T Current => index >= 0 && index < list.Count ?
			list[Index] : default;


		public int Index {
			get => index;
			set {
				if (value != index) {
					index = value;
					CurrentChanged?.Invoke(this, null);
				}
			}
		}

		public NavigationManager(int capacity = 10)
		{
			this.capacity = capacity;
		}

		public void Add(T item)
		{
			if (AddLock) {
				return;
			}
			if (Index < list.Count - 1) {
				list.RemoveRange(Index + 1, list.Count - (Index + 1));
			}
			list.Add(item);
			Index++;
			CanGoPreviousChanged?.Invoke(this, null);
			CanGoNextChanged?.Invoke(this, null);
			if (list.Count > capacity) {
				list.RemoveAt(0);
				index--;
			}
		}


		public async Task GoPreviousAsync(Func<T, Task> func)
		{
			if (!CanGoPrevious) {
				return;
			}
			var canGoNextCache = CanGoNext;
			Index--;
			if (!canGoNextCache) {
				CanGoNextChanged?.Invoke(this, null);
			}
			if (!CanGoPrevious) {
				CanGoPreviousChanged?.Invoke(this, null);
			}
			AddLock = true;
			await func.Invoke(list[index]).ConfigureAwait(false);
			AddLock = false;
		}

		public async Task GoNextAsync(Func<T, Task> func)
		{
			if (!CanGoNext) {
				return;
			}
			var canGoPreviousCache = CanGoPrevious;
			Index++;
			if (!canGoPreviousCache) {
				CanGoPreviousChanged?.Invoke(this, null);
			}
			if (!CanGoNext) {
				CanGoNextChanged?.Invoke(this, null);
			}
			AddLock = true;
			await func.Invoke(list[index]).ConfigureAwait(false);
			AddLock = false;
		}

	}
}
