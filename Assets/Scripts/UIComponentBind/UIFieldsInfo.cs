using System.Reflection;
using System.Collections.Generic;
using System;

namespace KH.UIBinding
{
    public class UIFieldsInfo
    {
        public Type UIType;
        public List<FieldInfo> BindedComponents = new List<FieldInfo>(8);
    }
}