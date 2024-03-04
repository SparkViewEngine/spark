using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Spark;

public class InMemoryCacheService(IMemoryCache cache) : ICacheService
{
    public object Get(string identifier)
    {
        return cache.Get(identifier);
    }

    public void Store(string identifier, CacheExpires expires, ICacheSignal signal, object item)
    {
        var option = new MemoryCacheEntryOptions();

        if (expires != null)
        {
            if (expires.Sliding > CacheExpires.NoSlidingExpiration)
            {
                option.SlidingExpiration = expires.Sliding;
            }
            else
            {
                option.AbsoluteExpiration = expires.Absolute;
            }
        }

        if (signal != null)
        {
            option.AddExpirationToken(new SignalChangeToken(signal));
        }

        cache.Set(identifier, item, option);
    }

    private class SignalChangeToken : IChangeToken
    {
        private readonly ICacheSignal signal;
        private bool hasChanged;
        
        public SignalChangeToken(ICacheSignal signal)
        {
            this.signal = signal;
            this.signal.Changed += this.SignalOnChanged;
        }

        private void SignalOnChanged(object sender, EventArgs e)
        {
            this.hasChanged = true;
        }

        public bool HasChanged => this.hasChanged;

        public bool ActiveChangeCallbacks => true;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return new StopListeningToSignal(this);
        }

        private class StopListeningToSignal(SignalChangeToken signalChangeToken) : IDisposable
        {
            public void Dispose()
            {
                signalChangeToken.signal.Changed -= signalChangeToken.SignalOnChanged;
            }
        }
    }
}