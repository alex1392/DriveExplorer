using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DriveExplorerTests {
    public class Test {
        [Fact]
        public void MyTestMethod() {
            var a = new MyClass();
            a.MyProperty = 1;
        }

        class MyClass {

            public event EventHandler MyEvent;
            public MyClass() {
                MyEvent += async (sender, e) => await MyMethodAsync();
            }

            public int MyProperty {
                set { 
                    MyEvent(this, null);
                    Func<Task> func = MyMethodAsync;
                    var task = func.Invoke();
                    var obj = func.DynamicInvoke();
                    MyMethodAsync();
                }
            }

            private async Task MyMethodAsync() {
                await Task.Run(() =>
                {
                    foreach (var i in Enumerable.Range(1, 100)) {
                        Debug.WriteLine(i);
                    }
                });
            }
        }
    }
}
