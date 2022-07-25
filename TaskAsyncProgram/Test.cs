using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAsyncProgram
{
    public class Test
    {
        public static void Go(string preStr = "")
        {
            StringBuilder str = new StringBuilder();
            str.Append(preStr);
            str.Append("线程");
            str.Append(Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine(str.ToString());
        }

        public static async Task TestCompletedTaskAsync()
        {
            /*本质上来说是返回一个已经完成的Task对象，所以这时如果我们用await关键字去等待Task.CompletedTask，
            .NET Core认为没有必要再去线程池启动一个新的线程来执行await关键字之后的代码，
            所以实际上await Task.CompletedTask之前和之后的代码是在同一个线程上同步执行的，通俗易懂的说就是单线程的
            */
            await Task.CompletedTask;
    
            Console.WriteLine("TestCompletedTaskAsync的线程：" + Thread.CurrentThread.ManagedThreadId);
        }

        public static async Task TestYieldAsync()
        {
            /*Task.Yield()是真正使用Task来启动了一个线程，只不过这个线程什么都没有干，相当于在使用await Task.Yield()的时候，确实是在用await等待一个还没有完成的Task对象，
             * 所以这时调用线程（主线程）就会立即返回去做其它事情了，当调用线程（主线程）返回后，await等待的Task对象就立即变为完成了，
             * 这时await关键字之后的代码由另外一个线程池线程来执行。
             */
            await Task.Yield();
            Console.WriteLine("TestYieldAsync的线程：" + Thread.CurrentThread.ManagedThreadId);
        }

    }
}
