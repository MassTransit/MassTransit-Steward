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
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;


    public class HttpProxyConsumer :
        Consumes<ExecuteHttpRequest>.Context
    {
        readonly HttpClient _client;

        public HttpProxyConsumer()
        {
            _client = new HttpClient();
        }

        public void Consume(IConsumeContext<ExecuteHttpRequest> context)
        {
            Console.WriteLine("Sending Request to {0}", context.Message.Url);

            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(30000);

                DateTime startTime = DateTime.UtcNow;
                Stopwatch timer = Stopwatch.StartNew();

                Task task = _client.GetAsync(context.Message.Url,
                    HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token)
                                   .ContinueWith(x =>
                                       {
                                           timer.Stop();

                                           Console.WriteLine("Request completing as {0} ({1})", x.Result.StatusCode,
                                               context.Message.Url);

                                           if (x.Result.IsSuccessStatusCode)
                                           {
                                               context.Respond(new HttpRequestSucceededEvent(x.Result.StatusCode));

                                               context.NotifyResourceUsageCompleted(context.Message.Url, startTime,
                                                   timer.Elapsed);
                                           }
                                           else
                                           {
                                               context.Respond(new HttpRequestFaultedEvent(x.Result.StatusCode));

                                               context.NotifyResourceUsageFailed(context.Message.Url,
                                                   x.Result.ReasonPhrase);
                                           }

                                           x.Result.Dispose();
                                       }, TaskContinuationOptions.OnlyOnRanToCompletion)
                                   .ContinueWith(
                                       x =>
                                           {
                                               context.Respond(
                                                   new HttpRequestFaultedEvent(HttpStatusCode.InternalServerError));
                                           },
                                       TaskContinuationOptions.OnlyOnFaulted);

                task.Wait(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                context.GenerateFault(ex);
            }
        }


        public class HttpRequestFaultedEvent :
            HttpRequestFaulted
        {
            public HttpRequestFaultedEvent(HttpStatusCode statusCode)
            {
                StatusCode = (int)statusCode;
            }

            public int StatusCode { get; private set; }
        }


        public class HttpRequestSucceededEvent :
            HttpRequestSucceeded
        {
            public HttpRequestSucceededEvent(HttpStatusCode statusCode)
            {
                StatusCode = (int)statusCode;
            }

            public int StatusCode { get; private set; }
        }
    }


    public interface HttpRequestSucceeded
    {
        int StatusCode { get; }
    }


    public interface HttpRequestFaulted
    {
        int StatusCode { get; }
    }


    public interface ExecuteHttpRequest
    {
        Uri Url { get; }
    }
}