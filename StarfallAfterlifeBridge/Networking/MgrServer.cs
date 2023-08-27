using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking
{
    public class MgrServer : HttpServer
    {
        public delegate void QueryHandler(HttpListenerContext context, SfaHttpQuery query);

        protected QueryHandler QueryHandlerCallback { get; }

        public MgrServer(QueryHandler queryHandlerCallback)
        {
            QueryHandlerCallback = queryHandlerCallback;
        }


        protected override void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            string method = request.HttpMethod;
            SfaHttpQuery query;

            switch (method)
            {
                case "GET":
                    query = SfaHttpQuery.Parse(request.QueryString);
                    break;

                case "POST":
                    string postQueryText = null;

                    if (request.InputStream != null)
                        using (StreamReader reader = new StreamReader(request.InputStream))
                            postQueryText = reader.ReadToEnd();

                    query = SfaHttpQuery.Parse(postQueryText).Union(SfaHttpQuery.Parse(request.QueryString));

                    //Log($"POST Request: {postQueryText}");
                    break;

                default:
                    query = new SfaHttpQuery();
                    break;
            }

            if (query.Function is not null)
            {
                try
                {
                    HandleQuery(context, query);
                }
                catch { }
            }
        }

        protected virtual void HandleQuery(HttpListenerContext context, SfaHttpQuery query)
        {
            QueryHandlerCallback?.Invoke(context, query);
        }
    }
}
