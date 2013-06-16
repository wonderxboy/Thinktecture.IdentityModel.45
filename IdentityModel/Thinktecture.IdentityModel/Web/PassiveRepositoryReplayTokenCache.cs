using System;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Thinktecture.IdentityModel.Web
{
    public class PassiveRepositoryReplayTokenCache : TokenReplayCache
    {
        readonly IReplayTokenCacheRepository _replayTokenCacheRepository;
        readonly TokenReplayCache _inner;

        public PassiveRepositoryReplayTokenCache(IReplayTokenCacheRepository replayTokenCacheRepository)
            : this(replayTokenCacheRepository,
                   FederatedAuthentication.FederationConfiguration.IdentityConfiguration.Caches.TokenReplayCache)
        {
        }

        public PassiveRepositoryReplayTokenCache(IReplayTokenCacheRepository replayTokenCacheRepository, TokenReplayCache inner)
        {
            if (replayTokenCacheRepository == null) throw new ArgumentNullException("replayTokenCacheRepository");
            if (inner == null) throw new ArgumentNullException("inner");

            _replayTokenCacheRepository = replayTokenCacheRepository;
            _inner = inner;
        }

        static readonly object _lastCleanupLock = new object();
        static DateTime? _lastCleanup;
        const int CleanupIntervalHours = 6;

        void CleanupOldTokens()
        {
            lock (_lastCleanupLock)
            {
                if (_lastCleanup == null || _lastCleanup < DateTime.UtcNow.AddHours(-CleanupIntervalHours))
                {
                    _lastCleanup = DateTime.UtcNow;

                    Task.Factory.StartNew(
                        delegate
                        {
                            // cleanup old tokens
                            DateTime date = DateTime.UtcNow.AddHours(-CleanupIntervalHours);
                            _replayTokenCacheRepository.RemoveAllBefore(date);
                        })
                    .ContinueWith(task =>
                    {
                        // don't take down process if this fails 
                        // if ThrowUnobservedTaskExceptions is enabled
                        if (task.IsFaulted)
                        {
                            var ex = task.Exception;
                        }
                    });
                }
            }
        }

        public override void AddOrUpdate(string key, SecurityToken securityToken, DateTime expirationTime)
        {
            CleanupOldTokens();

            if (key == null) throw new ArgumentNullException("key");

            _inner.AddOrUpdate(key, securityToken, expirationTime);

            var item = new ReplayTokenCacheItem()
            {
                Key = key,
                Expires = expirationTime,
            };

            _replayTokenCacheRepository.AddOrUpdate(item);
        }

        public override SecurityToken Get(string key)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(string key)
        {
            CleanupOldTokens();

            if (key == null) throw new ArgumentNullException("key");

            return (_inner.Contains(key) || _replayTokenCacheRepository.Contains(key));
        }

        public override void Remove(string key)
        {
            CleanupOldTokens();

            if (key == null) throw new ArgumentNullException("key");

            _inner.Remove(key);
            _replayTokenCacheRepository.Remove(key);
        }
    }
}