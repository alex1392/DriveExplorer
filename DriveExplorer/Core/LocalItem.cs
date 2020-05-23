using System;
using System.Collections.Generic;
using System.Linq;
using Directory = System.IO.Directory;
using System.Threading.Tasks;
using DriveExplorer.IoC;
using System.Windows;
using System.IO;

namespace DriveExplorer {

    public class LocalItem : IItem {
        public ItemTypes Type { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }

        public async IAsyncEnumerable<IItem> GetChildrenAsync() {
            IEnumerable<string> paths;
            try {
                paths = Directory.GetFiles(FullPath).Concat(Directory.GetDirectories(FullPath));
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show(ex.Message);
                yield break;
            } catch (IOException ex) {
                MessageBox.Show(ex.Message);
                yield break;
            }
            foreach (var path in paths) {
                yield return IocContainer.Default.GetSingleton<LocalItemFactory>().Create(path);
            }
        }
    }
    
}
