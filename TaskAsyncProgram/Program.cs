// See https://aka.ms/new-console-template for more information
//直接使用线程池
ThreadPool.QueueUserWorkItem(s => Test.Go());

Console.WriteLine("主线程:" + Thread.CurrentThread.ManagedThreadId);

await Test.GoAsync();
await Task.Run(() => { Console.WriteLine("Task.Run:" + Thread.CurrentThread.ManagedThreadId); });



//使用任务并行库
var t = new Task(() => { Test.Go(); });
t.Start();

Console.WriteLine("Task的ID：" + t.Id);
Console.WriteLine("Task的状态：" + t.Status);
/*Task的状态：
（1）Created:首次创建，还没有运行时。可以类比线程的Unstarted状态。
（2）WaitingForActivation：任务已隐式创建并会在未来某个时间自动开始，这通常会出现在任务的接续中，后面的任务需要等待前面的任务完成才会开始。
（3）WaitingToRun：任务已被任务调度器调度，还没开始运行。
（4）Running：运行中，和线程的Running状态相同。
（5）WaitingForChildrenToComplete：任务本身运行完毕，等待子任务运行完毕。线程本身不存在父子关系，所以没有这个状态。
（6）RanToCompletion：运行完成，任务终止的三种状态之一。
（7）Canceled：被取消，任务终止的三种状态之一。
（8）Faulted：运行中出现异常，任务终止的三种状态之一。
 IsCompleted属性为true等同任务处于终止的三种状态之一，
想了解是否正常完成，查询状态是否为RanToCompletion，想了解是否取消查询状态是否为Canceled或IsCancelled属性；
想了解是否出现异常，查询IsFaulted属性或状态是否为Faulted。
 */

//使用任务并行库的TaskFactory
Task.Factory.StartNew(() => { Test.Go(); });

//直接运行比new Task少了设置选项的功能
Task.Run(() => { Test.Go(); }).ContinueWith((Task t) => { Test.Go("ContinueWith:"); });
//ContinueWith也有TaskContinuationOptions枚举和TaskCreationOptions相似
/*两个较为特殊的标志是ExecuteSynchronously:指定接续任务和前一个任务相同的线程。
    LazyCancellation：如果取消了第一个接续任务，则后面的接续的接续的任务不会完成，除非第一个接续任务完成。
    不设置的情况下，无论它的前一个任务状态如何都会无条件执行。
*/

//支持取消且为长任务的任务工厂，使用当前的任务调度器
var ctn = new CancellationToken();
var factory = new TaskFactory(ctn, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, TaskScheduler.Current);
factory.StartNew(() => { Test.Go(); });

//只能用工厂实现接续方法
var testtask = Task.Run(() =>
{
    //普通任务
});
var whenContinueFactory = new TaskFactory();
var continueTask = whenContinueFactory.ContinueWhenAll(new[] { testtask }, t1 =>
{
    //接续任务

});

/*以下方法都会阻塞当前线程
 * Task的Wait，WaitAll，WaitAny等待可以设置超时时间，时间到了任务还没完成一样解除，继续往下执行
 * 一般情况下不建议让阻塞，最好使用任务的ContinueWith方法接续
 */

Console.WriteLine("TestCompletedTaskAsync前:" + Thread.CurrentThread.ManagedThreadId);
await Test.TestCompletedTaskAsync();
Console.WriteLine("TestCompletedTaskAsync后:" + Thread.CurrentThread.ManagedThreadId);
await Test.TestYieldAsync();
Console.WriteLine("TestYieldAsync后:" + Thread.CurrentThread.ManagedThreadId);


//任务的分子关系分为附加子任务和分离子任务，默认情况下是附加子任务,设置 附加子任务，因此主任务需要等待所有
var parentTask = new Task<int>(() =>
{
    //主任务创建一个子任务
    new Task(() =>
    {
        Thread.Sleep(3000);//仅限于演示
        Console.WriteLine("子任务结束");
    }, TaskCreationOptions.AttachedToParent).Start();//父任务的状态取决于附加子任务的状态，附加子任务引发的异常会传播到父任务，如果是分离子任务则不会。
    return 1;
});

//主任务的接续
parentTask.ContinueWith(t => { Console.WriteLine("主任务的接续"); });
parentTask.Start();


//异常处理最佳方式用接续方法，TaskContinuationOptions可以设置针对前任务是异常，取消，正常完成等待设置不同的接续任务
Task<int> taskResult = Task.Run<int>(() =>
{
    //throw new Exception("运气太差");
    return 3;
});
taskResult.ContinueWith(t1 =>
{
    Console.WriteLine("发生异常");
    foreach (var ex in t1.Exception.Flatten().InnerExceptions)
    {
        Console.WriteLine(ex);
    }
}, TaskContinuationOptions.OnlyOnFaulted);
taskResult.ContinueWith(t1 => { Console.WriteLine("运行完成"); }, TaskContinuationOptions.OnlyOnRanToCompletion);

Console.ReadKey();