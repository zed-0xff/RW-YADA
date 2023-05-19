using Steamworks;
using System.Threading;
using System.Xml;
using System;
using Verse;

namespace zed_0xff.YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

abstract class Request {
    public abstract XmlDocument Process();
}

class Request_SetItemDescription : Request {
    public PublishedFileId_t PublishedFileId = PublishedFileId_t.Invalid;
    public string description;

    const int TIMEOUT_MS = 15000;
    AutoResetEvent autoEvent = new AutoResetEvent(false);

    public override XmlDocument Process(){
        if( PublishedFileId == PublishedFileId_t.Invalid ){
            throw new ArgumentException("PublishedFileId is not set");
        }

        UGCUpdateHandle_t curUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), PublishedFileId);
        if( !SteamUGC.SetItemDescription(curUpdateHandle, description) ){
            throw new ArgumentException("SteamUGC.SetItemDescription() returned false");
        }
        SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(curUpdateHandle, null);

        XmlDocument doc = new XmlDocument();
        doc.AppendChild(doc.CreateElement("Response"));

        var submitResult = CallResult<SubmitItemUpdateResult_t>.Create(delegate(SubmitItemUpdateResult_t result, bool bIOFailure){
                var xr = doc.CreateElement("result");
                xr.InnerText = result.m_eResult.ToString();
                doc.DocumentElement.AppendChild(xr);

                var xb = doc.CreateElement("bIOFailure");
                xb.InnerText = bIOFailure.ToString();
                doc.DocumentElement.AppendChild(xb);

                autoEvent.Set();
                });
        submitResult.Set(hAPICall);

        if( !autoEvent.WaitOne(TIMEOUT_MS) ){
            throw new TimeoutException();
        }

        return doc;
    }
}
