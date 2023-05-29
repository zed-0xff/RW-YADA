using System.Xml.Linq;

namespace YADA.API;

abstract class Request {
    protected XElement doc = new XElement("Response");

    protected abstract void processInternal();

    public XElement Process(){
        processInternal();
        return doc;
    }
}

