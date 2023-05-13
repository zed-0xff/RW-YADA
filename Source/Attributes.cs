using System;
using System.Collections.Generic;

namespace zed_0xff.YADA;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class FieldLabel : Attribute {
    public string value;

    public FieldLabel(string v) {
        value = v;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class FieldCategory : Attribute {
    public string value;

    public FieldCategory(string v) {
        value = v;
    }
}
