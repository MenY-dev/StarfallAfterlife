using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public class SfaClientResponse : MessagingResponse
    {
        public SfaServerAction Action { get; init; }

        public override void SetInput(string text)
        {
            var doc = JsonHelpers.ParseNodeUnbuffered(text ?? "");
            base.SetInput((string)doc["message"]);
        }

        public override void SetInput(ReadOnlyMemory<byte> data)
        {
            if (data.Length > 0)
                data = data[1..];

            base.SetInput(data);
        }
    }
}
