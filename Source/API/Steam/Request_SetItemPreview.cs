using Steamworks;
using System.Threading;
using System.Xml.Linq;
using System;
using Verse;

namespace YADA.API.Steam;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class SetItemPreview : SteamRequest {
    public PublishedFileId_t PublishedFileId = PublishedFileId_t.Invalid;
    public string PreviewFile;

    protected override CallResult processSteamInternal(){
        if( PublishedFileId == PublishedFileId_t.Invalid ){
            throw new ArgumentException("PublishedFileId is not set");
        }

        UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), PublishedFileId);
        if( !SteamUGC.SetItemPreview(handle, PreviewFile) ){
            throw new ArgumentException("SteamUGC.SetItemPreview() returned false");
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
