namespace Wirex.Engine.Core
{
    public static class Executors
    {
        public static Executor Immediate() => new ImmediateExecutor();
        public static Executor ThreadPool() => new ThreadPoolExecutor();
        public static Executor Dispatcher() => new DispatcherExecutor();
    }
}