using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using Verse;

namespace zed_0xff.YADA.API;

class Server {
    static Instance instance = null;

    public static void Toggle(bool newEnable, int newPort){
        if( newEnable ){
            if( instance != null ){
                // restart instance if port differs
                if( instance.Port != newPort ){
                    instance.Stop();
                    instance = new Instance(newPort);
                }
            } else {
                // start instance
                instance = new Instance(newPort);
            }
        } else {
            // stop instance if exists
            if( instance != null ){
                instance.Stop();
                instance = null;
            }
        }
    }

    class Instance {
        int port;
        HttpListener listener = new HttpListener();
        Thread thread;

        public int Port => port;

        public Instance(int port){
            this.port = port;
            string prefix = "http://127.0.0.1:" + port + "/";
            Log.Message("[d] YADA starting API thread on " + prefix);
            listener.Prefixes.Add(prefix);
            thread = new Thread(ThreadFunc);
            thread.Start();
        }

        void ThreadFunc(){
            listener.Start();
            while(true){
                HttpListenerContext context = listener.GetContext(); // throws HttpListenerException on listener close
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string responseString = null;
                try {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("KeepAlive: {0}\n", request.KeepAlive);
                    sb.AppendFormat("Local end point: {0}\n", request.LocalEndPoint?.ToString());
                    sb.AppendFormat("Remote end point: {0}\n", request.RemoteEndPoint?.ToString());
                    sb.AppendFormat("Is local? {0}\n", request.IsLocal);
                    sb.AppendFormat("HTTP method: {0}\n", request.HttpMethod);
                    sb.AppendFormat("Protocol version: {0}\n", request.ProtocolVersion);
                    sb.AppendFormat("Is authenticated: {0}\n", request.IsAuthenticated);
                    sb.AppendFormat("Is secure: {0}\n", request.IsSecureConnection);
                    sb.AppendFormat("QueryString: {0}\n", request.QueryString);
                    foreach ( String k in request.QueryString.AllKeys ){
                        sb.AppendFormat("    {0}: {1}\n", k, request.QueryString[k]);
                    }

                    sb.AppendFormat("Url: {0}\n", request.Url);
                    sb.AppendFormat("Url.Segments: {0}\n", string.Join(", ", request.Url.Segments));
                    //sb.AppendFormat("RawUrl: {0}\n", request.RawUrl);
                    sb.AppendFormat("Headers:\n");
                    foreach ( String k in request.Headers.AllKeys ){
                        sb.AppendFormat("    {0}: {1}\n", k, request.Headers[k]);
                    }

                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(request.InputStream);
                    Request r = DirectXmlToObject.ObjectFromXml<Request>(xmlDocument.DocumentElement, false);
                    Log.Warning("[d] " + r + ": " + r.foo);

                    responseString = sb.ToString();
                } catch (Exception ex) {
                    if( response.StatusCode == 200 ){
                        response.StatusCode = 500;
                        responseString = ex.ToString();
                    } else {
                        // error message like "HTTP/1.1 411 Length Required" already handled by framework
                        continue;
                    }
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer,0,buffer.Length);
                output.Close();
            }
        }

        public void Stop(){
            listener.Stop();
            thread.Join();
        }

        public static void ListenerCallback(IAsyncResult result) {
            HttpListener listener = (HttpListener) result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer,0,buffer.Length);
            // You must close the output stream.
            output.Close();
        }
    }
}
