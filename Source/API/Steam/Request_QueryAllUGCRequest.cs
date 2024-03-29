using Steamworks;

namespace YADA.API.Steam;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class QueryAllUGCRequest : SteamQueryUGCRequest {
    // https://partner.steamgames.com/doc/api/ISteamUGC#EUGCQuery
    public EUGCQuery QueryType;
    // https://partner.steamgames.com/doc/api/ISteamUGC#EUGCMatchingUGCType
    public EUGCMatchingUGCType MatchingeMatchingUGCTypeFileType;
    public AppId_t CreatorAppID = (AppId_t)294100;
    public AppId_t ConsumerAppID = (AppId_t)294100;
    public int Page = 1;

    public string SearchText;

    protected override CallResult processSteamInternal(){
        var handle = SteamUGC.CreateQueryAllUGCRequest(
                QueryType,
                MatchingeMatchingUGCTypeFileType,
                CreatorAppID,
                ConsumerAppID,
                (uint)Page
                );

        if( !string.IsNullOrEmpty(SearchText) ){
            SteamUGC.SetSearchText(handle, SearchText);
        }

        return sendSteamRequest(handle);
    }
}
