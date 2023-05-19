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
                    XmlDocument xmlReq = new XmlDocument();
                    xmlReq.Load(request.InputStream);
                    Request r = DirectXmlToObject.ObjectFromXml<Request>(xmlReq.DocumentElement, false);
                    XmlDocument xmlResp = r.Process();
                    StringWriter sw = new StringWriter();
                    XmlTextWriter xw = new XmlTextWriter(sw);
                    xmlResp.WriteTo(xw);
                    responseString = sw.ToString();
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
