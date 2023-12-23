using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Steamworks;
using System.Reflection;
using Verse;

namespace YADA.API.Steam;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

abstract class SteamQueryUGCRequest : SteamRequest {
    public bool ReturnAdditionalPreviews;
    public bool ReturnChildren;
    public bool ReturnKeyValueTags;
    public bool ReturnLongDescription;
    public bool ReturnMetadata;
    public bool ReturnOnlyIDs;
    public bool ReturnTotalOnly;
    public int  ReturnPlaytimeStats;

    public List<string> requiredTags;
    public Dictionary<string, string> requiredKeyValueTags;

    ///

    private UGCQueryHandle_t handle;

    protected virtual CallResult sendSteamRequest(UGCQueryHandle_t handle){
        this.handle = handle;

        SteamUGC.SetReturnAdditionalPreviews(handle, ReturnAdditionalPreviews);
        SteamUGC.SetReturnChildren(handle, ReturnChildren);
        SteamUGC.SetReturnKeyValueTags(handle, ReturnKeyValueTags);
        SteamUGC.SetReturnLongDescription(handle, ReturnLongDescription);
        SteamUGC.SetReturnMetadata(handle, ReturnMetadata);
        SteamUGC.SetReturnOnlyIDs(handle, ReturnOnlyIDs);
        SteamUGC.SetReturnTotalOnly(handle, ReturnTotalOnly);

        if( ReturnPlaytimeStats != 0 ){
            SteamUGC.SetReturnPlaytimeStats(handle, (uint)ReturnPlaytimeStats);
        }

        if( requiredTags != null ){
            foreach( string tag in requiredTags ){
                SteamUGC.AddRequiredTag(handle, tag);
            }
        }

        if( requiredKeyValueTags != null ){
            foreach( var kv in requiredKeyValueTags ){
                SteamUGC.AddRequiredKeyValueTag(handle, kv.Key, kv.Value);
            }
        }

        SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
        var cr = CallResult<SteamUGCQueryCompleted_t>.Create(delegate(SteamUGCQueryCompleted_t result, bool bIOFailure){
                try {
                    OnGotItemDetails(result, bIOFailure);
                } finally {
                    autoEvent.Set();
                }
                });
        cr.Set(hAPICall);
        return cr;
    }

    public static XElement SteamObj2Xml( object obj ){
        Type type = obj.GetType();
        XElement r = DirectXmlSaver.XElementFromObject(obj, type);
        foreach (FieldInfo fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)){
            if( fi.Name.EndsWith("_") ){
                string publicFieldName = fi.Name.TrimEnd('_');
                MethodInfo getter = fi.DeclaringType.GetMethod("get_" + publicFieldName, BindingFlags.Instance | BindingFlags.Public);
                if( getter == null )
                    continue;

                XElement oldChild = r.Descendants(fi.Name).OfType<XElement>().First();
                string value = getter.Invoke(obj, null)?.ToString();
                if( string.IsNullOrEmpty(value) ){
                    oldChild.Remove();
                    continue;
                }

                XElement newChild = new XElement( publicFieldName, value );
                if( oldChild != null ){
                    oldChild.ReplaceWith(newChild);
                } else {
                    r.Add(newChild);
                }
            }
        }
        return r;
    }

    void OnGotItemDetails(SteamUGCQueryCompleted_t result, bool bIOFailure){
        if( bIOFailure ){
            doc.Add(new XElement("IOFailure", bIOFailure.ToString()));
            return;
        }

        doc.Add( DirectXmlSaver.XElementFromObject(result, result.GetType()) );
        var holder = new XElement("Results");
        doc.Add(holder);

        for (uint i = 0; i < result.m_unNumResultsReturned; i++){
            var li = new XElement("li");
            holder.Add(li);
            if( SteamUGC.GetQueryUGCResult(handle, i, out SteamUGCDetails_t pDetails) ){
                li.Add( SteamObj2Xml(pDetails) );
                if( SteamUGC.GetQueryUGCMetadata(handle, i, out string metadata, Constants.k_cchDeveloperMetadataMax) && metadata != "" ){
                    li.Add(new XElement("Metadata", metadata));
                }
                uint nTags = SteamUGC.GetQueryUGCNumTags(handle, i);
                if( nTags != 0 ){
                    var tags = new XElement("Tags");
                    li.Add(tags);
                    for( uint j=0; j<nTags; j++){
                        SteamUGC.GetQueryUGCTag(handle, i, j, out string tagName, 255);
                        tags.Add(new XElement("li", tagName));
                    }
                }
                uint nKeyValueTags = SteamUGC.GetQueryUGCNumKeyValueTags(handle, i);
                if( nKeyValueTags != 0 ){
                    var tags = new XElement("KeyValueTags");
                    li.Add(tags);
                    for( uint j=0; j<nKeyValueTags; j++){
                        SteamUGC.GetQueryUGCKeyValueTag(handle, i, j, out string tagKey, 255, out string tagValue, 255);
                        tags.Add(new XElement(tagKey, tagValue));
                    }
                }
                uint nAddPreviews = SteamUGC.GetQueryUGCNumAdditionalPreviews(handle, i);
                if( nAddPreviews != 0 ){
                    var previews = new XElement("AdditionalPreviews");
                    li.Add(previews);
                    for( uint j=0; j<nAddPreviews; j++){
                        SteamUGC.GetQueryUGCAdditionalPreview(handle, i, j, out string pchURLOrVideoID, 1024, out string pchOriginalFileName, 1024, out EItemPreviewType pPreviewType);
                        previews.Add(new XElement("li",
                                new XElement("URLOrVideoID", pchURLOrVideoID),
                                new XElement("OriginalFileName", pchOriginalFileName),
                                new XElement("ItemPreviewType", pPreviewType)
                                ));
                    }
                }
            }
        }
    }

    protected override void finalize(){
        SteamUGC.ReleaseQueryUGCRequest(handle);
        base.finalize();
    }
}

