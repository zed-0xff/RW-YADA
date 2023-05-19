using Steamworks;
using System;
using System.Threading;
using System.Xml.Linq;

namespace zed_0xff.YADA.API;

abstract class Request {
    protected const int TIMEOUT_MS = 15000;
    protected AutoResetEvent autoEvent = new AutoResetEvent(false);

    protected XElement doc = new XElement("Response");

    public XElement Process(){
        CallResult cr = processInternal();
        if( !autoEvent.WaitOne(TIMEOUT_MS) ){
            throw new TimeoutException();
        }

        return doc;
    }

    protected abstract CallResult processInternal();
}

