using System;
using System.Threading;
using Steamworks;
using UnityEngine;

namespace YADA.API.Steam;

abstract class SteamRequest : Request {
    protected const int TIMEOUT_MS = 15000;
    protected AutoResetEvent autoEvent = new AutoResetEvent(false);
    bool run0;
    bool runInBackgroundTouched;

    /// <summary>Set from main thread at mod load; Application.runInBackground is only read/set when on this thread.</summary>
    internal static Thread MainThread { get; set; }

    protected abstract CallResult processSteamInternal();

    protected override void processInternal(){
        bool onMain = MainThread != null && Thread.CurrentThread == MainThread;
        if (onMain) {
            run0 = Application.runInBackground;
            runInBackgroundTouched = true;
        }
        CallResult cr = processSteamInternal();
        if( cr != null ){
            if (onMain) {
                Application.runInBackground = true; // or SteamUGC reqs won't run while in bg
            }
            if( !autoEvent.WaitOne(TIMEOUT_MS) ){
                finalize();
                throw new TimeoutException();
            }
        }

        finalize();
    }

    protected virtual void finalize(){
        if( runInBackgroundTouched && !run0 ){
            Application.runInBackground = false;
        }
    }
}
