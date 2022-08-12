using EleCho.Json;
using EleCho.JsonModelGenerator.Utils;
using Humanizer.Inflections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Linq;

namespace EleCho.JsonModelGenerator.Core
{

    public class JsonModelWriter
    {
        readonly JsonModelWriterOptions _options;
        readonly TextWriter _writer;

        public List<Exception> Exceptions { get; set; } = new List<Exception>();

        public JsonModelWriter(JsonModelWriterOptions options, TextWriter target)
        {
            _options = options;
            _writer = target;
        }

        public void WriteModel(JsonObject jobj)
        {
            WriteJsonModelCore(0, "RootObject", jobj, Enumerable.Empty<string>(), out _);
        }

        public void WriteModel(JsonArray jarr)
        {
            //WriteJsonModelCore(0, );
        }

        /// <summary>
        /// 根据一个值为对象的属性的属性名极其父类名生成一个合适的类型名
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="classParents">父类名</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">属性名为空</exception>
        protected string GenerateClassNameFromPropertyName(string propertyName, IEnumerable<string> classParents)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Invalid property name");

            List<string> words = _options.ClassNestStyle == ClassNestStyle.NoNestWithBetterName ?
                StrUtils.SplitIdentifier(string.Join(null, classParents.Append(propertyName))) :
                StrUtils.SplitIdentifier(propertyName);

            return _options.NamingStyle switch
            {
                NamingStyle.Pascal => StrUtils.MakePascal(words),
                NamingStyle.Camel => StrUtils.MakeCamel(words),
                NamingStyle.Snake => StrUtils.MakeSnake(words),
                _ => string.Join(null, _options.ClassNestStyle == ClassNestStyle.NoNestWithBetterName ?
                    classParents.Append(propertyName) :
                    new string[] { propertyName })
            };
        }

        /// <summary>
        /// 根据一个值为数组的属性的属性名与其父类名生成适合该数组成员类型的类型名
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="classParents">父类名</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        protected string GenerateClassNameFromCollectionName(string propertyName, IEnumerable<string> classParents)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Invalid property name");

            List<string> words = _options.ClassNestStyle == ClassNestStyle.NoNestWithBetterName ?
                StrUtils.SplitIdentifier(string.Join(null, classParents.Append(propertyName))) :
                StrUtils.SplitIdentifier(propertyName);

            Func<IEnumerable<string>, string> stylizer = _options.NamingStyle switch
            {
                NamingStyle.Pascal => StrUtils.MakePascal,
                NamingStyle.Camel => StrUtils.MakeCamel,
                NamingStyle.Snake => StrUtils.MakeSnake,
                _ => (_) => string.Join(null, _options.ClassNestStyle == ClassNestStyle.NoNestWithBetterName ?
                    classParents.Append(propertyName) :
                    new string[] { propertyName }),
            };

            return _options.CollectionItemNamingStyle switch
            {
                CollectionItemNamingStyle.PropertyName => stylizer(words),
                CollectionItemNamingStyle.PropertyNameAndItem => stylizer(words.Append("Item")),
                _ => stylizer(words.Take(words.Count - 1).Append(Vocabularies.Default.Singularize(words.TakeLast(1).First())))
            };
        }

        /// <summary>
        /// 根据成员类型获取集合类型 T[], T&lt;&gt;, IEnumerable&lt;&gt;
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        protected string GetTypeNameOfCollection(string itemType)
        {
            return _options.CollectionTypeStyle switch
            {
                CollectionTypeStyle.List => $"List<{itemType}>",
                CollectionTypeStyle.IEnumerable => $"IEnumerable<{itemType}>",

                _ => $"{itemType}[]"
            };
        }

        private Func<string, string> GetPropertyNameBuilder()
        {
            return _options.NamingStyle switch
            {
                NamingStyle.Pascal => (name) => StrUtils.MakePascal(StrUtils.SplitIdentifier(name)),
                NamingStyle.Camel => (name) => StrUtils.MakeCamel(StrUtils.SplitIdentifier(name)),
                NamingStyle.Snake => (name) => StrUtils.MakeSnake(StrUtils.SplitIdentifier(name)),

                _ => (name) => name,
            };
        }

        private Action<string> GetAttributeWriter(int level)
        {
            return _options.AttributeStyle switch
            {
                AttributeStyle.SystemTextJson => (name) =>
                {
                    WriteAttributeCore(level, "JsonPropertyName", name);
                }
                ,
                AttributeStyle.NewtonsoftJson => (name) =>
                {
                    WriteAttributeCore(level, "JsonProperty", name);
                }
                ,
                _ => (_) => { }
            };
        }

        protected bool WriteJsonModelCore(int level, IEnumerable<KeyValuePair<string, IJsonData>> toWrite, IEnumerable<string> classParents, out Dictionary<string, string> name2type)
        {
            name2type = new Dictionary<string, string>();
            foreach (var kv in toWrite)
            {
                if (kv.Value is JsonObject jobj)
                {
                    if (WriteJsonModelCore(level, kv.Key, jobj, classParents, out string _className))
                    {
                        name2type[kv.Key] = _className;
                    }
                }
                else if (kv.Value is JsonArray jarr)
                {
                    bool
                        hasstr = false,
                            hasnum = false,
                            hasboo = false,
                            hasobj = false,
                            hasarr = false;

                    foreach (var item in jarr)
                    {
                        hasstr |= item is JsonString;
                        hasnum |= item is JsonNumber;
                        hasboo |= item is JsonBoolean;
                        hasobj |= item is JsonObject;
                        hasarr |= item is JsonArray;


                        if (hasstr && hasnum && hasboo && hasobj && hasarr)
                            break;
                    }

                    bool complexType = false;

                    // 检查类型有没有混合 (一个数组装多个类型的数据)
                    bool flag = false;
                    foreach (var cur in new bool[] { hasstr, hasnum, hasboo, hasobj, hasarr })
                    {
                        if (cur && flag)
                        {
                            complexType = true;
                            break;
                        }

                        flag = cur;
                    }

                    if (complexType)
                    {
                        if (hasarr && hasobj)
                        {
                            name2type[kv.Key] = GetTypeNameOfCollection("object");
                        }
                        else
                        {
                            if (WriteJsonModelCore(level, kv.Key, jarr, classParents, out string _arrClassName))
                            {
                                name2type[kv.Key] = _arrClassName;
                            }
                            else
                            {
                                name2type[kv.Key] = GetTypeNameOfCollection("object");
                            }
                        }
                    }
                    else
                    {
                        if (hasstr)
                            name2type[kv.Key] = GetTypeNameOfCollection("string");
                        else if (hasboo)
                            name2type[kv.Key] = GetTypeNameOfCollection("bool");
                        else if (hasnum)
                            name2type[kv.Key] = GetTypeNameOfCollection(NumUtils.GetNumType(jarr.OfType<JsonNumber>()));
                        else if (hasobj || hasarr)
                        {
                            if (WriteJsonModelCore(level, kv.Key, jarr, classParents, out string _arrClassName))
                            {
                                name2type[kv.Key] = _arrClassName;
                            }
                            else
                            {
                                name2type[kv.Key] = GetTypeNameOfCollection("object");
                            }
                        }
                        else
                        {
                            name2type[kv.Key] = "object";
                        }

                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 在当前位置写下一个 class 声明, 通过传入的 JsonObject
        /// </summary>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <param name="classParents">类型父类树</param>
        /// <param name="className">新生成的类型名</param>
        /// <returns></returns>
        protected bool WriteJsonModelCore(int level, string name, JsonObject data, IEnumerable<string> classParents, out string className)
        {
            Dictionary<string, string> name2type;
            className = GenerateClassNameFromPropertyName(name, classParents);

            if (_options.ClassNestStyle == ClassNestStyle.Nest)
            {
                _writer.Write(new string(' ', _options.Indent * level));
                _writer.WriteLine($"public class {className}");
                _writer.Write(new string(' ', _options.Indent * level));
                _writer.WriteLine("{");

                WriteJsonModelCore(level + 1, data, classParents.Append(className), out name2type);
            }
            else
            {
                WriteJsonModelCore(level, data, classParents.Append(className), out name2type);

                _writer.Write(new string(' ', _options.Indent * level));
                _writer.WriteLine($"public class {className}");
                _writer.Write(new string(' ', _options.Indent * level));
                _writer.WriteLine("{");
            }

            Func<string, string> propNameBuilder = GetPropertyNameBuilder();
            Action<string> attributeWriter = GetAttributeWriter(level + 1);

            foreach (var kv in data)
            {
                attributeWriter(kv.Key);


                if (kv.Value is JsonObject or JsonArray)
                {
                    string typename = name2type[kv.Key];
                    WriteValueCore(level + 1, typename, kv.Key);
                }
                else
                {
                    _ = kv.Value switch
                    {
                        JsonString => WriteValueCore(level + 1, "string", kv.Key),
                        JsonNumber jnum => WriteValueCore(level + 1, NumUtils.GetNumType(jnum), kv.Key),
                        JsonBoolean => WriteValueCore(level + 1, "bool", kv.Key),
                        JsonNull => WriteValueCore(level + 1, "object", kv.Key),

                        _ => false
                    };
                }
            }

            _writer.Write(new string(' ', _options.Indent * level));
            _writer.WriteLine("}");

            return true;
        }

        /// <summary>
        /// 在当前位置写下多个 class 声明, 通过传入的 JsonArray (遍历检查)
        /// JsonArray 的成员必须全部为 Object 或者 Array
        /// </summary>
        /// <param name="level"></param>
        /// <param name="propName"></param>
        /// <param name="data"></param>
        /// <param name="classParents"></param>
        /// <param name="typeName">集合的类型 (包含数组, 列表或可迭代外壳)</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected bool WriteJsonModelCore(int level, string propName, JsonArray data, IEnumerable<string> classParents, out string typeName)
        {
            if (data.Count == 0)
            {
                typeName = null!;
                return false;
            }

            JsonObject unionObj = new JsonObject();
            foreach (var item in data.OfType<JsonObject>())
            {
                foreach (var kv in item)
                {
                    if (!unionObj.ContainsKey(kv.Key))
                    {
                        unionObj[kv.Key] = kv.Value;
                    }
                }
            }

            JsonArray unionArr = new JsonArray();
            foreach (var item in data.OfType<JsonArray>())
            {
                unionArr.AddRange(item);
            }

            if (unionObj.Count > 0)
            {

                Dictionary<string, string> name2type;
                var className = GenerateClassNameFromCollectionName(propName, classParents);

                if (_options.ClassNestStyle == ClassNestStyle.Nest)
                {
                    _writer.Write(new string(' ', _options.Indent * level));
                    _writer.WriteLine($"public class {className}");
                    _writer.Write(new string(' ', _options.Indent * level));
                    _writer.WriteLine("{");

                    WriteJsonModelCore(level + 1, unionObj, classParents.Append(className), out name2type);
                }
                else
                {
                    WriteJsonModelCore(level, unionObj, classParents.Append(className), out name2type);

                    _writer.Write(new string(' ', _options.Indent * level));
                    _writer.WriteLine($"public class {className}");
                    _writer.Write(new string(' ', _options.Indent * level));
                    _writer.WriteLine("{");
                }

                Func<string, string> propNameBuilder = GetPropertyNameBuilder();
                Action<string> attributeWriter = GetAttributeWriter(level + 1);

                foreach (var kv in unionObj)
                {
                    attributeWriter(kv.Key);


                    if (kv.Value is JsonObject or JsonArray)
                    {
                        string typename = name2type[kv.Key];
                        WriteValueCore(level + 1, typename, kv.Key);
                    }
                    else
                    {
                        _ = kv.Value switch
                        {
                            JsonString => WriteValueCore(level + 1, "string", kv.Key),
                            JsonNumber jnum => WriteValueCore(level + 1, NumUtils.GetNumType(data
                                .OfType<JsonObject>()
                                .Where(obj => obj.ContainsKey(kv.Key))
                                .Select(obj => obj[kv.Key])
                                .OfType<JsonNumber>()), kv.Key),
                            JsonBoolean => WriteValueCore(level + 1, "bool", kv.Key),
                            JsonNull => WriteValueCore(level + 1, "object", kv.Key),

                            _ => false
                        };
                    }
                }

                _writer.Write(new string(' ', _options.Indent * level));
                _writer.WriteLine("}");

                typeName = GetTypeNameOfCollection(className);
                return true;
            }
            else if (unionArr.Count > 0)
            {
                if (WriteJsonModelCore(level, propName, unionArr, classParents, out string childTypeName))
                {
                    typeName = GetTypeNameOfCollection(childTypeName);
                    return true;
                }

                typeName = null!;
                return false;
            }

            typeName = null!;
            return false;
        }

        /// <summary>
        /// 在当前位置写下一个特性标记 (仅支持基础类型)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="attributeName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        protected bool WriteAttributeCore(int level, string attributeName, params object[] parameters)
        {
            IEnumerable<string> attributeParameterInsStrs = parameters.Select(v =>
                v switch
                {
                    null => "null",
                    bool b => b.ToString(),

                    char c => c.ToString(),
                    byte n => n.ToString(),
                    sbyte n => n.ToString(),
                    short n => n.ToString(),
                    ushort n => n.ToString(),
                    int n => n.ToString(),
                    uint n => n.ToString(),
                    long n => n.ToString(),
                    ulong n => n.ToString(),
                    float n => n.ToString(),
                    double n => n.ToString(),
                    decimal n => n.ToString(),
                    string s => JsonSerializer.Serialize(s),

                    _ => throw new ArgumentException("Invalid parameter found")
                });

            string paramstr = parameters.Any() ? $"({string.Join(", ", attributeParameterInsStrs)})" : string.Empty;

            _writer.Write(new string(' ', _options.Indent * level));
            _writer.Write($"[{attributeName}{paramstr}]");
            _writer.WriteLine();

            return true;
        }

        /// <summary>
        /// 在当前位置写下一个 class 属性成员声明
        /// </summary>
        /// <param name="level"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected bool WriteValueCore(int level, string type, string name)
        {
            List<string> words = StrUtils.SplitIdentifier(name);
            string stylizedName = _options.NamingStyle switch
            {
                NamingStyle.Pascal => StrUtils.MakePascal(words),
                NamingStyle.Camel => StrUtils.MakeCamel(words),
                NamingStyle.Snake => StrUtils.MakeSnake(words),
                _ => name,
            };

            _writer.Write(new string(' ', _options.Indent * level));
            _writer.Write($"public {type} {stylizedName} {{ get; set; }}");
            _writer.WriteLine();

            return true;
        }
    }
}
