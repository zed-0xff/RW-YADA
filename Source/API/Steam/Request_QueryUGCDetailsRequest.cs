using Steamworks;
using System.Collections.Generic;

namespace YADA.API.Steam;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class QueryUGCDetailsRequest : SteamQueryUGCRequest {
    public List<PublishedFileId_t> PublishedFileIds;

    protected override CallResult processSteamInternal(){
        var handle = SteamUGC.CreateQueryUGCDetailsRequest(
                PublishedFileIds.ToArray(),
                (uint)PublishedFileIds.Count);

        return sendSteamRequest(handle);
    }
}
