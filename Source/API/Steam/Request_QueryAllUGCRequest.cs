using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System;
using Verse;

namespace YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

// XXX unfinished

class Request_CreateQueryAllUGCRequest : SteamRequest {
    // https://partner.steamgames.com/doc/api/ISteamUGC#EUGCQuery
    public EUGCQuery eQueryType;
    // https://partner.steamgames.com/doc/api/ISteamUGC#EUGCMatchingUGCType
    public EUGCMatchingUGCType eMatchingeMatchingUGCTypeFileType;
    public AppId_t nCreatorAppID = (AppId_t)294100;
    public AppId_t nConsumerAppID = (AppId_t)294100;
    public int unPage = 1;

    protected override CallResult processSteamInternal(){
        UGCQueryHandle_t handle = SteamUGC.CreateQueryAllUGCRequest( eQueryType, eMatchingeMatchingUGCTypeFileType, nCreatorAppID, nConsumerAppID, (uint)unPage );
        return null;
    }
}
