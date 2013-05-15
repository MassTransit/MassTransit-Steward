// Copyright 2007-2013 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Steward.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using BusConfigurators;
    using Configurators;
    using Contracts;
    using Core.Agents;
    using Core.Consumers;
    using EndpointConfigurators;
    using Exceptions;
    using Magnum.Extensions;
    using NUnit.Framework;
    using Saga;
    using Testing;
    using Transports;


    [TestFixture]
    public abstract class DispatchTestFixture
    {
        readonly EndpointFactoryConfiguratorImpl _endpointFactoryConfigurator;
        EndpointCache _endpointCache;
        CancellationToken _cancellationToken;
        static Timer _timer;
        static CancellationTokenSource _cancellationTokenSource;

        protected DispatchTestFixture()
            : this(new Uri("loopback://localhost"))
        {
            TestTimeout = Debugger.IsAttached
                ? 30.Seconds()
                : 5.Minutes();
        }

        protected DispatchTestFixture(Uri baseUri)
        {
            BaseUri = baseUri;

            var defaultSettings = new EndpointFactoryDefaultSettings();

            _endpointFactoryConfigurator = new EndpointFactoryConfiguratorImpl(defaultSettings);
            _endpointFactoryConfigurator.SetPurgeOnStartup(true);

            CommandTestContexts = new Dictionary<Type, DispatchTestContext>();
        }

        protected Uri LocalUri { get; private set; }
        protected IServiceBus LocalBus { get; private set; }
        protected IEndpoint DispatchEndpoint { get; private set; }
        protected Uri BaseUri { get; private set; }
        protected IDictionary<Type, DispatchTestContext> CommandTestContexts { get; private set; }
        protected IEndpointFactory EndpointFactory { get; private set; }
        protected IEndpointCache EndpointCache { get; set; }

        [TestFixtureSetUp]
        public void ActivityTextFixtureSetup()
        {
            if (_endpointFactoryConfigurator != null)
            {
                ConfigurationResult result =
                    ConfigurationResultImpl.CompileResults(_endpointFactoryConfigurator.Validate());

                try
                {
                    EndpointFactory = _endpointFactoryConfigurator.CreateEndpointFactory();

                    _endpointCache = new EndpointCache(EndpointFactory);

                    EndpointCache = new EndpointCacheProxy(_endpointCache);
                }
                catch (Exception ex)
                {
                    throw new ConfigurationException(result, "An exception was thrown during endpoint cache creation",
                        ex);
                }
            }

            ServiceBusFactory.ConfigureDefaultSettings(x =>
                {
                    x.SetEndpointCache(EndpointCache);
                    x.SetConcurrentConsumerLimit(4);
                    x.SetReceiveTimeout(50.Milliseconds());
                    x.EnableAutoStart();
                });

            LocalUri = new Uri(BaseUri, "local");

            AddCommandContext<DispatchMessageConsumer, DispatchMessage>(() =>
                {
                    var agent = new MessageDispatchAgent(LocalBus);

                    return new DispatchMessageConsumer(agent);
                });

            SetupCommands();

            LocalBus = CreateServiceBus(ConfigureLocalBus);

            DispatchEndpoint = LocalBus.GetEndpoint(GetActivityContext<DispatchMessage>().ExecuteUri);
        }

        [TestFixtureTearDown]
        public void ActivityTestFixtureFixtureTeardown()
        {
            foreach (DispatchTestContext activityTestContext in CommandTestContexts.Values)
                activityTestContext.Dispose();

            LocalBus.Dispose();

            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Dispose();
            if (_timer != null)
                _timer.Dispose();

            _endpointCache.Clear();

            if (EndpointCache != null)
            {
                EndpointCache.Dispose();
                EndpointCache = null;
            }

            ServiceBusFactory.ConfigureDefaultSettings(x => { x.SetEndpointCache(null); });
        }

        protected void AddTransport<T>()
            where T : class, ITransportFactory, new()
        {
            _endpointFactoryConfigurator.AddTransportFactory<T>();
        }

        protected void AddCommandContext<TConsumer, T>(Func<TConsumer> consumerFactory)
            where T : class
            where TConsumer : class, Consumes<T>.Context
        {
            var context = new DispatchTestContext<TConsumer, T>(BaseUri, consumerFactory);

            CommandTestContexts.Add(typeof(T), context);
        }

        protected virtual void ConfigureLocalBus(ServiceBusConfigurator configurator)
        {
            configurator.ReceiveFrom(LocalUri);
        }

        protected virtual IServiceBus CreateServiceBus(Action<ServiceBusConfigurator> configurator)
        {
            return ServiceBusFactory.New(x =>
                {
                    configurator(x);

                    foreach (DispatchTestContext context in CommandTestContexts.Values)
                        context.ConfigureServiceBus(x);
                });
        }

        protected virtual bool WaitForSubscription<T>()
            where T : class
        {
            return CommandTestContexts.Values.All(x => x.ExecuteBus.HasSubscription<T>().Any());
        }

        protected DispatchTestContext GetActivityContext<T>()
        {
            return CommandTestContexts[typeof(T)];
        }

        protected CancellationToken TestCancellationToken
        {
            get
            {
                if (_cancellationToken == CancellationToken.None)
                    _cancellationToken = Delay((int)TestTimeout.TotalMilliseconds);

                return _cancellationToken;
            }
        }

        protected TimeSpan TestTimeout { get; set; }

        public static CancellationToken Delay(int millisecondsTimeout)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = null;

            _timer = new Timer(delegate
                {
                    _timer.Dispose();
                    _cancellationTokenSource.Cancel();
                }, null, Timeout.Infinite, Timeout.Infinite);

            _timer.Change(millisecondsTimeout, Timeout.Infinite);

            return _cancellationTokenSource.Token;
        }

        protected void ConfigureEndpointFactory(Action<EndpointFactoryConfigurator> configure)
        {
            if (_endpointFactoryConfigurator == null)
                throw new ConfigurationException("The endpoint factory configurator has already been executed.");

            configure(_endpointFactoryConfigurator);
        }

        protected static InMemorySagaRepository<TSaga> SetupSagaRepository<TSaga>()
            where TSaga : class, ISaga
        {
            var sagaRepository = new InMemorySagaRepository<TSaga>();

            return sagaRepository;
        }

        protected abstract void SetupCommands();
    }
}