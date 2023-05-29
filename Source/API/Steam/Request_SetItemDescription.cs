using Steamworks;
using System.Threading;
using System.Xml.Linq;
using System;
using Verse;

namespace YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_SetItemDescription : SteamRequest {
    public PublishedFileId_t PublishedFileId = PublishedFileId_t.Invalid;
    public string Description;

    protected override CallResult processSteamInternal(){
        if( PublishedFileId == PublishedFileId_t.Invalid ){
            throw new ArgumentException("PublishedFileId is not set");
        }

        UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), PublishedFileId);
        if( !SteamUGC.SetItemDescription(handle, Description) ){
            throw new ArgumentException("SteamUGC.SetItemDescription() returned false");
        }
        SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(handle, null);

        var cr = CallResult<SubmitItemUpdateResult_t>.Create(delegate(SubmitItemUpdateResult_t result, bool bIOFailure){
                if( bIOFailure ){
                    doc.Add(new XElement("IOFailure", bIOFailure.ToString()));
                }
                doc.Add( DirectXmlSaver.XElementFromObject(result, result.GetType()) );

                autoEvent.Set();
                });
        cr.Set(hAPICall);
        return cr;
    }
}
