using Microsoft.Extensions.Primitives;

namespace IPMS.Business.Common.AWS
{
    public class PeriodicWatcher : IDisposable
    {
        private readonly TimeSpan _refreshInterval;
        private IChangeToken _changeToken;
        private readonly Timer _timer;
        private CancellationTokenSource _cancellationTokenSource;

        public PeriodicWatcher(TimeSpan refreshInterval)
        {
            _refreshInterval = refreshInterval;
            _timer = new Timer(OnChange, null, TimeSpan.Zero, _refreshInterval);
        }

        private void OnChange(object? state)
        {
            _cancellationTokenSource?.Cancel();
        }

        public IChangeToken Watch()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

            return _changeToken;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
