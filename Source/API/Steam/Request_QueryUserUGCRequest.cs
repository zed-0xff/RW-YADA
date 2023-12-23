using Steamworks;

namespace YADA.API.Steam;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

// https://partner.steamgames.com/doc/api/ISteamUGC#CreateQueryUserUGCRequest
class QueryUserUGCRequest : SteamQueryUGCRequest {
    public AccountID_t AccountID = SteamUser.GetSteamID().GetAccountID();
    public EUserUGCList ListType;
    // https://partner.steamgames.com/doc/api/ISteamUGC#EUGCMatchingUGCType
    public EUGCMatchingUGCType MatchingUGCType;
    EUserUGCListSortOrder SortOrder;

    public AppId_t CreatorAppID = (AppId_t)294100;
    public AppId_t ConsumerAppID = (AppId_t)294100;
    public int Page = 1;

    protected override CallResult processSteamInternal(){
        var handle = SteamUGC.CreateQueryUserUGCRequest(
                AccountID,
                ListType,
                MatchingUGCType,
                SortOrder,
                CreatorAppID,
                ConsumerAppID,
                (uint)Page
                );

        return sendSteamRequest(handle);
    }
}
