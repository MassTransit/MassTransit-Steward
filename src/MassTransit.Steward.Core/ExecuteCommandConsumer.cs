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
namespace MassTransit.Steward.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using Contracts;
    using MassTransit.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;


    public class ExecuteCommandConsumer :
        Consumes<DispatchCommand>.Context
    {
        static readonly ILog _log = Logger.Get<ExecuteCommandConsumer>();
        readonly CommandExecutionAgent _agent;

        public ExecuteCommandConsumer(CommandExecutionAgent agent)
        {
            _agent = agent;
        }

        public void Consume(IConsumeContext<DispatchCommand> context)
        {
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat("ExecuteCommand: {0} at {1}", context.Message.DispatchId,
                    context.Message.CreateTime);
            }

            string body;
            using (var ms = new MemoryStream())
            {
                context.BaseContext.CopyBodyTo(ms);

                body = Encoding.UTF8.GetString(ms.ToArray());
            }

            if (string.Compare(context.ContentType, "application/vnd.masstransit+json",
                StringComparison.OrdinalIgnoreCase)
                == 0)
                body = TranslateJsonBody(body, context.Message.Destination.ToString());
            else if (string.Compare(context.ContentType, "application/vnd.masstransit+xml",
                StringComparison.OrdinalIgnoreCase) == 0)
                body = TranslateXmlBody(body, context.Message.Destination.ToString());
            else
                throw new InvalidOperationException("Only JSON and XML messages can be executed");

            var executionContext = new ConsumerCommandExecutionContext(context, body);

            _agent.Execute(executionContext);
        }

        static string TranslateJsonBody(string body, string destination)
        {
            JObject envelope = JObject.Parse(body);

            envelope["destinationAddress"] = destination;

            JToken message = envelope["message"];

            JToken command = message["command"];
            JToken commandType = message["commandTypes"];

            envelope["message"] = command;
            envelope["messageType"] = commandType;

            return JsonConvert.SerializeObject(envelope, Formatting.Indented);
        }

        static string TranslateXmlBody(string body, string destination)
        {
            using (var reader = new StringReader(body))
            {
                XDocument document = XDocument.Load(reader);

                XElement envelope = (from e in document.Descendants("envelope") select e).Single();

                XElement destinationAddress =
                    (from a in envelope.Descendants("destinationAddress") select a).Single();

                XElement message = (from m in envelope.Descendants("message") select m).Single();
                IEnumerable<XElement> messageType = (from mt in envelope.Descendants("messageType") select mt);

                XElement command = (from p in message.Descendants("command") select p).Single();
                IEnumerable<XElement> commandType = (from pt in message.Descendants("commandTypes") select pt);

                message.Remove();
                messageType.Remove();

                destinationAddress.Value = destination;

                message = new XElement("message");
                message.Add(command.Descendants());
                envelope.Add(message);

                envelope.Add(commandType.Select(x => new XElement("messageType", x.Value)));

                return document.ToString();
            }
        }

        static string ToString(Uri uri)
        {
            if (uri == null)
                return "";

            return uri.ToString();
        }
    }
}