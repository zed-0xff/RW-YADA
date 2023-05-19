using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System;
using Verse;

namespace zed_0xff.YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_SendQueryUGCRequest : Request {
    public List<PublishedFileId_t> PublishedFileIds;

    private int detailsQueryCount;
    private UGCQueryHandle_t handle;

    protected override CallResult processInternal(){
        detailsQueryCount = PublishedFileIds.Count();
        handle = SteamUGC.CreateQueryUGCDetailsRequest(PublishedFileIds.ToArray(), (uint)detailsQueryCount);
        SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
        var cr = CallResult<SteamUGCRequestUGCDetailsResult_t>.Create(delegate(SteamUGCRequestUGCDetailsResult_t result, bool bIOFailure){
                try {
                    OnGotItemDetails(result, bIOFailure);
                } finally {
                    autoEvent.Set();
                }
                });
        cr.Set(hAPICall);
        return cr;
    }

    void OnGotItemDetails(SteamUGCRequestUGCDetailsResult_t result, bool bIOFailure){
        doc.Add(new XElement("bIOFailure", bIOFailure.ToString()));
        if( bIOFailure )
            return;

        for (int i = 0; i < detailsQueryCount; i++){
            if( SteamUGC.GetQueryUGCResult(handle, (uint)i, out SteamUGCDetails_t pDetails) ){
                doc.Add( DirectXmlSaver.XElementFromObject(pDetails, pDetails.GetType()) );
            }
        }
    }
}
