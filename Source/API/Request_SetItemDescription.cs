using Steamworks;
using System.Threading;
using System.Xml.Linq;
using System;
using Verse;

namespace zed_0xff.YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_SetItemDescription : Request {
    public PublishedFileId_t PublishedFileId = PublishedFileId_t.Invalid;
    public string description;

    protected override CallResult processInternal(){
        if( PublishedFileId == PublishedFileId_t.Invalid ){
            throw new ArgumentException("PublishedFileId is not set");
        }

        UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), PublishedFileId);
        if( !SteamUGC.SetItemDescription(handle, description) ){
            throw new ArgumentException("SteamUGC.SetItemDescription() returned false");
        }
        SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(handle, null);

        var cr = CallResult<SubmitItemUpdateResult_t>.Create(delegate(SubmitItemUpdateResult_t result, bool bIOFailure){
                doc.Add(new XElement("result", result.m_eResult.ToString()));
                doc.Add(new XElement("bIOFailure", bIOFailure.ToString()));

                autoEvent.Set();
                });
        cr.Set(hAPICall);
        return cr;
    }
}
