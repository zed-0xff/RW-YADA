using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System;
using HarmonyLib;
using Verse;

namespace YADA;

public class Parser {
    public PatchDef.Anyfix anyfix;
    public PatchDef patchDef;

    public List<Type> argTypes = new List<Type>();
    public List<string> argNames = new List<string>();

    public string error;

    public Parser(PatchDef.Anyfix anyfix, PatchDef patchDef){
        this.anyfix = anyfix;
        this.patchDef = patchDef;
    }

    public bool ParseArguments(){
        if( anyfix.arguments == null ) return true;

        foreach( string s in anyfix.arguments ){
            bool byRef = false;
            Type t = null;
            // "ref string ___foobar"
            var a = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if( a[0] == "ref" ){
                byRef = true;
                a.RemoveAt(0);
            }
            if( a.Count() == 2 ){
                t = AccessTools.TypeByName(a[0]);
                if( t == null ){
                    error = "unknown type " + a[0];
                    return false;
                }
                a.RemoveAt(0);
            }
            if( a.Count() == 1 ){
                if( t == null ){
                    if( a[0] == "__instance" ){
                        t = AccessTools.TypeByName(patchDef.className);
                    } else if( a[0] == "__result" ){
                        t = patchDef.resultType;
                    } else {
                        error = "cannot get implicit type of " + a[0];
                        return false;
                    }
                }
                argTypes.Add( byRef ? t.MakeByRefType() : t );
                argNames.Add( a[0] );
                a.RemoveAt(0);
            }
            if( a.Any() ){
                error = "failed to parse '" + s + "' as an argument";
                return false;
            }
        }
        return true;
    }

    void emitOpcodeWithArg(ILGenerator il, OpCode opcode, string s){
        if( opcode == OpCodes.Call || opcode == OpCodes.Callvirt ){
            MethodInfo mi = Utils.fqmnToMethodInfo(s, out error);
            if( mi == null ){
                error = opcode + ": " + error;
                return;
            }
            il.Emit(opcode, mi);
            return;
        }
        if( opcode == OpCodes.Isinst ){
            Type t = AccessTools.TypeByName(s);
            if( t == null ){
                error = opcode + ": cannot get type for " + s;
                return;
            }
            il.Emit(opcode, t);
            return;
        }
        error = opcode + ": don't know how to parse arg";
    }

    public bool ParseOpcodes(ILGenerator il){
        foreach( string s in anyfix.opcodes ){
            var a = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if( a.Count() < 1 || a.Count() > 2 ){
                error = "cannot parse opcode: " + s;
                return false;
            }

            var fi = typeof(OpCodes).GetField(a[0], BindingFlags.Public | BindingFlags.Static);
            if( fi == null ){
                error = "cannot parse opcode: " + s;
                return false;
            }
            
            OpCode opcode = (OpCode)fi.GetValue(null);

            if( a.Count() == 1 ){
                il.Emit(opcode);
            } else {
                emitOpcodeWithArg(il, opcode, a[1]);
                if( error != null ) return false;
            }
        }
        return error == null;
    }

//    public bool Parse(){
//        if( anyfix.arguments != null && !parseArguments() ) return false;
//        if( anyfix.opcodes == null ){
//            error = "no opcodes";
//            return false;
//        }
//        error = parseOpcodes();
//
//        return error == null;
//    }
}
