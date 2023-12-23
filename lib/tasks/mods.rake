namespace :mods do
  desc "fetch mods"
  task :fetch do
    r = RimTool::YADA.request "Steam.QueryAllUGCRequest",
      QueryType:                        "k_EUGCQuery_RankedByLastUpdatedDate",
      MatchingeMatchingUGCTypeFileType: "k_EUGCMatchingUGCType_Items_ReadyToUse",
      ReturnLongDescription: true,
      SearchText: "copper",
      requiredTags: %w"Mod 1.4"

    puts r
  end
end
