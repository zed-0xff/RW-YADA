using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Verse;

namespace YADA;

static class ExpCompiler {
    public delegate object DynInvoker(object root);

    static Dictionary<string, DynInvoker> invokerCache = new Dictionary<string, DynInvoker>();

    public static DynInvoker Compile(string exp, object root){
        DynInvoker invoker;
        if( !invokerCache.TryGetValue(exp, out invoker) ){
            invoker = invokerCache[exp] = compile(exp, root);
        }
        return invoker;
    }

    public class TokenList : List<Token> {
        public override string ToString(){
            return string.Join(".", this);
        }

        public string Original { get { return string.Join(".", this.Select((x) => x.Original)); }}
    }

    public abstract class Token {
        public abstract string Original{ get; }

        public Type typeHint = null;
    }

    public class ImmToken : Token {
        public int value;

        public ImmToken(int x) {
            value = x;
        }

        public override string Original => value.ToString();
        public override string ToString(){
            return "{" + value + "}";
        }
    }

    public class SimpleToken : Token {
        public string content;

        public override string Original { get { return content; } }

        public SimpleToken(string x) {
            content = x;
        }

        public override string ToString(){
            return "[" + content + "]";
        }
    }

    public class CallToken : Token {
        public string method;
        public TokenList[] args;
        public Type[] argTypes = null;

        public override string Original { get { return method + "(" + string.Join(",", args.Select((x) => x.Original)) + ")"; } }

        public CallToken(string m, TokenList[] a) {
            method = m;
            args = a == null ? new TokenList[]{} : a;
        }

        public override string ToString(){
            return "[" + method + "(" + string.Join(", ", (object[])args) + ")]";
        }
    }

    public static TokenList Tokenize(string fqmn){
        TokenList tokens = new TokenList();
        int start = 0;
        for( int i=0; i<fqmn.Length; i++ ){
            switch(fqmn[i]){
                case '.':
                    if( i != start ){
                        addToken(fqmn.Substring(start, i-start));
                    }
                    start = i+1;
                    break;
                case '(':
                case '[':
                    char closing = fqmn[i] == '(' ? ')' : ']';
                    int depth = 0;
                    for( int j=i+1; j<fqmn.Length; j++ ){
                        if( fqmn[j] == fqmn[i] ) // '('
                            depth++;
                        if( fqmn[j] == closing ){ // ')'
                            if( depth > 0 ){
                                depth--;
                            } else {
                                var args = fqmn.Substring(i+1, j-i-1)
                                    .Split(',')
                                    .Select(x => x.Trim())
                                    .Where(x  => !string.IsNullOrEmpty(x))
                                    .Select(x => Tokenize(x))
                                    .ToArray();
                                if( closing == ')' ){
                                    tokens.Add(new CallToken(fqmn.Substring(start, i-start), args));
                                } else {
                                    if( i != start )
                                        tokens.Add(new SimpleToken(fqmn.Substring(start, i-start)));
                                    tokens.Add(new CallToken("get_Item", args));
                                }
                                i = j;
                                start = i+1;
                                break;
                            }
                        }
                    }
                    break;
                case '|':
                    string typeName = fqmn.Substring(start, i-start);
                    Type t = AccessTools.TypeByName(typeName);
                    if( t == null ){
                        throw new ArgumentException("no type \""+typeName+"\"");
                    }
                    tokens.Last().typeHint = t;
                    start = i+1;
                    break;
                case ')':
                case ']':
                    throw new ArgumentException("unexpected '"+fqmn[i]+"'");
            }
        }

        if( start < fqmn.Length ){
            addToken(fqmn.Substring(start));
        }

        void addToken(string value){
            if( Int32.TryParse(value, out int immValue) ){
                tokens.Add(new ImmToken(immValue));
            } else {
                tokens.Add(new SimpleToken(value));
            }
        }

        return tokens;
    }

    static DynInvoker compile( string fqmn, object root ){
        return FastCallTokenized(Tokenize(fqmn), root);
    }

    static DynInvoker FastCallTokenized( TokenList tokens, object root){
        DynamicMethod dynMethod = new DynamicMethod("", typeof(object), new Type[]{typeof(object)}, restrictedSkipVisibility: true);
        ILGenerator il = dynMethod.GetILGenerator(256);
        var lRet = il.DefineLabel();
        var localVar = il.DeclareLocal(typeof(object));

        // syntax sugar
        if( (tokens[0] as SimpleToken)?.content == "parent" ){
            tokens.Insert(0, new SimpleToken("this"));
        }

        if( tokens.Count() < 1 ){
            throw new ArgumentException("don't know how to parse " + tokens.Count() + " arg(s)");
        }

        Type lastValueType;
        object obj;
        Token token = tokens[0];
    
        if( (token as SimpleToken)?.content == "this" ){
            tokens.RemoveAt(0);
            obj = root;
            lastValueType = root.GetType();
            il.Emit(OpCodes.Ldarg_0);
        } else if( token is ImmToken imm) {
            obj = imm.value;
            lastValueType = obj.GetType();
        } else {
            List<string> typeParts = new List<string>();
            Type t = null;
            while( t == null && token is SimpleToken st ){
                typeParts.Add(st.content);
                tokens.RemoveAt(0);
                token = tokens[0];
                t = AccessTools.TypeByName(string.Join(".", typeParts));
            }
            if( t == null ){
                throw new ArgumentException("cannot resolve root type");
            }
            obj = t;
            lastValueType = t;
        }

        while( tokens.Any() ){
            if( token != null && token.typeHint != null ){
                lastValueType = token.typeHint;
            }

            token = tokens[0];
            tokens.RemoveAt(0);

            if( token is ImmToken it ){
                il.Emit(OpCodes.Ldc_I4, it.value);
                continue;
            }

            if( obj == null ){
                // ${Find.DesignatorManager.SelectedDesignator.GetType()}
                if( lastValueType != null && !lastValueType.IsAbstract ){
                    try{
                        obj = Activator.CreateInstance(lastValueType);
                    } catch( MissingMethodException ){
                        // Default constructor not found for type RimWorld.Designator_Build
                    }
                }
            }

            if( token is SimpleToken st ){
                // field
                FieldInfo fi;
                if( obj is null )
                    fi = AccessTools.Field(lastValueType, st.content);
                else
                    fi = AccessTools.Field((obj is Type) ? (Type)obj : obj.GetType(), st.content);

                if( fi != null ){
                    if( tokens.Count() > 0 ){
                        // ${Find.DesignatorManager.SelectedDesignator.isOrder}
                        // check only if it's not last token
                        if( !fi.IsStatic && (obj == null || obj is Type) )
                            throw new ArgumentException("non-static field " + fi + " on no object");
                        obj = fi.GetValue(obj);
                    }

                    lastValueType = fi.FieldType;
                    il.Emit(fi.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fi);
                    
                    continue;
                }
                // fi == null, try as getter
                // intentional fallback to CallToken check
                CallToken newToken = new CallToken("get_" + st.content, null);
                newToken.typeHint = token.typeHint;
                token = newToken;
            }

            if( token is CallToken ct ){
                MethodInfo mi = null;
                object subValue = null;

                DynInvoker invoker = null;
                if( ct.args.Any() ){
                    invoker = Compile( ct.args[0].Original, root ); // XXX only a single arg now
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, invoker.GetInvocationList()[0].Method); // Call or Callvirt ?

                    subValue = invoker.Invoke(root);
                    if( subValue != null ){
                        ct.argTypes = new Type[]{ subValue.GetType() }; // XXX may be a subtype
                    }

                    if( obj == null && lastValueType != null )
                        mi = getMethodInfo(lastValueType, ct);
                    else
                        mi = getMethodInfo((obj is Type) ? (Type)obj : obj.GetType(), ct);

                    if( mi == null )
                        throw new ArgumentException("no method " + ct.Original + " on " + (obj == null ? "Null" : obj.GetType()));
//                    if( !mi.IsStatic && (obj == null || obj is Type) )
//                        throw new ArgumentException("non-static method " + mi + " on no object");

                    // this.parent.Position.GetEdifice(Find.CurrentMap)
                    //                      ^^^^^^^^^^ extension method
                    int pidx = mi.IsDefined(typeof(ExtensionAttribute)) ? 1 : 0;
                    Type ptype = mi.GetParameters()[pidx].ParameterType;
                    if( ptype.IsValueType ){
                        il.Emit(OpCodes.Unbox_Any, ptype);
                    }
                } else {
                    if( obj == null && lastValueType != null )
                        mi = getMethodInfo(lastValueType, ct);
                    else
                        mi = getMethodInfo((obj is Type) ? (Type)obj : obj.GetType(), ct);
                }

                if( mi == null )
                    throw new ArgumentException("no method " + ct.Original + " on obj:" + obj + " lt:" + lastValueType);
//                if( !mi.IsStatic && (obj == null || obj is Type) )
//                    throw new ArgumentException("non-static method " + mi + " on no object");

                il.Emit(mi.IsStatic ? OpCodes.Call : OpCodes.Callvirt, mi);

                // check for null
                //il.Emit(OpCodes.Dup);
//                il.Emit(OpCodes.Ldnull);
//                il.Emit(OpCodes.Ceq);
//                il.Emit(OpCodes.Br, lRet);
//

                lastValueType = mi.ReturnType;

                if( tokens.Count() == 0 ){
                    // skip last real call, only emit as opcode
                    break;
                }

                if( invoker == null ){
                    obj = mi.Invoke(obj, null);
                } else if( !mi.IsDefined(typeof(ExtensionAttribute) )){
                    try{ 
                        obj = mi.Invoke(obj, new[]{ subValue } );
                    } catch(ArgumentOutOfRangeException ex){
                        if( Prefs.DevMode ){
                            Log.Warning("[d] " + ex);
                        }
                        // trying to get list[x] while compile-time list is smaller than at runtime
                        if( !(subValue is int i) ) throw;
                        if( !(obj is IEnumerable e)) throw;
                        if( i == 0 ) throw;

                        obj = e.GetEnumerator().Current;
                    }
                } else {
                    obj = mi.Invoke(obj, new[]{ obj, subValue } );
                }
            } else {
                throw new ArgumentException("unexpected token: " + token);
            }
        }

        if (lastValueType.IsValueType){
            il.Emit(OpCodes.Box, lastValueType); // convert int/float/... into an object
        }

        il.MarkLabel(lRet);
        il.Emit(OpCodes.Ret);

        return (DynInvoker)dynMethod.CreateDelegate(typeof(DynInvoker));
    }

    public static MethodInfo getMethodInfo(Type t, CallToken ct){
        MethodInfo mi = null;

        try {
            mi = AccessTools.Method(t, ct.method); // does not return extension methods!
        } catch( AmbiguousMatchException ){
            if( ct.argTypes == null )
                throw;

            mi = AccessTools.Method(t, ct.method, ct.argTypes);
        }

        if( mi != null ) return mi;

        // try to find extension method
        foreach( MethodInfo mi2 in extensionMethodsForType(t) ){
            if( mi2.Name == ct.method && mi2.GetParameters().Length == ct.args.Length + 1 ){
                // XXX possible ambiguations like this:
                // IntVec3 RotatedBy(IntVec3, RotationDirection)
                // IntVec3 RotatedBy(IntVec3, Rot4)
                return mi2;
            }
        }

        return null;
    }

    public static MethodInfo[] allExtensionMethods = null;

    public static MethodInfo[] extensionMethodsForType(Type t){
        if( allExtensionMethods == null ){
            allExtensionMethods = GenTypes.AllTypes
                .Where(t => t.IsDefined(typeof(ExtensionAttribute)))
                .SelectMany(e => e.GetMethods().Where(m => m.IsDefined(typeof(ExtensionAttribute)))).ToArray();
        }

        return allExtensionMethods.Where(m => m.GetParameters()[0].ParameterType == t).ToArray();
    }
}
