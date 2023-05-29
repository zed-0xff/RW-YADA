using Steamworks;
using System;
using System.Threading;
using System.Xml.Linq;

namespace YADA.API;

abstract class Request {
    protected const int TIMEOUT_MS = 15000;
    protected AutoResetEvent autoEvent = new AutoResetEvent(false);

    protected XElement doc = new XElement("Response");

    public XElement Process(){
        CallResult cr = processInternal();
        if( cr != null ){
            if( !autoEvent.WaitOne(TIMEOUT_MS) ){
                finalize();
                throw new TimeoutException();
            }
        }

        finalize();
        return doc;
    }

    protected virtual void finalize(){
    }

    protected abstract CallResult processInternal();
}

