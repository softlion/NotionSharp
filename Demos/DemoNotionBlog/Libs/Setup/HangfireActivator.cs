using Hangfire;

namespace VapoliaFr.Blazorr
{
    public class HangfireActivator : JobActivator
    {
        private readonly IServiceProvider serviceProvider;

        public HangfireActivator(IServiceProvider serviceProvider)
            => this.serviceProvider = serviceProvider;

        public override object? ActivateJob(Type type)
            => serviceProvider.GetService(type);
    }
}