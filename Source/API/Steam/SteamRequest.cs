using System;
using System.Threading;
using Steamworks;
using UnityEngine;

namespace YADA.API.Steam;

abstract class SteamRequest : Request {
    protected const int TIMEOUT_MS = 15000;
    protected AutoResetEvent autoEvent = new AutoResetEvent(false);
    bool run0;

    protected abstract CallResult processSteamInternal();

    protected override void processInternal(){
        run0 = Application.runInBackground;
        CallResult cr = processSteamInternal();
        if( cr != null ){
            Application.runInBackground = true; // or SteamUGC reqs won't run while in bg
            if( !autoEvent.WaitOne(TIMEOUT_MS) ){
                finalize();
                throw new TimeoutException();
            }
        }

        finalize();
    }

    protected virtual void finalize(){
        if( !run0 ){
            Application.runInBackground = false;
        }
    }
}
