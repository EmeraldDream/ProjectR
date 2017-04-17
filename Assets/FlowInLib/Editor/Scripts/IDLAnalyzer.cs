using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace FlowInLib
{
    #region 数据结构
    public class ParamNode
    {
        public string name;
        public string type;
        public string lenParam;
        public string lenMax;
        public bool bBaseType;
    }

    public class StructNode
    {
        public string name;
        public List<ParamNode> paramList = new List<ParamNode>();
    }

    public class FuncNode
    {
        public string name;
        public string structName;
    }

    public class ClassNode
    {
        public string name;
        public List<FuncNode> funcs = new List<FuncNode>();
    }
    #endregion

    public class IDLReader
    {
        public List<StructNode> _arrStructs = new List<StructNode>();
        public List<ClassNode> _arrClasses = new List<ClassNode>();
        private char[] _trimChars = new char[] { ' ', '\t', '\n' };
        private const int MAX_ARRAY_SIZE = 1000;

        bool ParseStruct(string name, string[] strList)
        {
            if (strList.Length <= 0)
                return false;

            StructNode structNode = new StructNode();
            structNode.name = name;
            structNode.paramList.Clear();
            
            string[] keyValue = null;
            string[] typeAndLen = null;
            string[] lenAndSize = null;

            for (int index = 0; index < strList.Length; ++index)
            {
                keyValue = strList[index].Split(new char[] { ' ', '\t', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length != 2)
                {
                    Debug.Log(string.Format("ParseStruct Error: {0}", strList[index]));
                    return false;
                }

                ParamNode paramNode = new ParamNode();
                paramNode.name = keyValue[1];
                if (keyValue[0].Contains("["))
                {
                    typeAndLen = keyValue[0].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    if (typeAndLen.Length != 2)
                    {
                        Debug.Log(string.Format("ParseStruct Error: {0}", strList[index]));
                        return false;
                    }

                    paramNode.type = ToCSType(typeAndLen[0]);
                    lenAndSize = typeAndLen[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lenAndSize.Length == 1)
                    {
                        paramNode.lenParam = typeAndLen[1];
                        paramNode.lenMax = MAX_ARRAY_SIZE.ToString();
                    }
                    else
                    {
                        paramNode.lenParam = lenAndSize[0];
                        paramNode.lenMax = lenAndSize[1];
                    }
                }
                else
                {
                    paramNode.type = ToCSType(keyValue[0]);
                    paramNode.lenParam = "";
                    paramNode.lenMax = "";
                }
                paramNode.bBaseType = IsBaseType(paramNode.type);
                structNode.paramList.Add(paramNode);
            }

            foreach (var param in structNode.paramList)
            {
                if (!param.bBaseType)
                {
                    bool validType = _arrStructs.Find(obj => obj.name == param.type) != null;
                    if (!validType)
                    {
                        Debug.Log(string.Format("ParseStruct Error: Invalid type [{0}]", param.type));
                        return false;
                    }
                }
            }

            _arrStructs.Add(structNode);
            return true;
        }

        bool ParseClass(string name, string[] strList)
        {
            if (strList.Length <= 0)
                return false;

            ClassNode classNode = new ClassNode();
            classNode.name = name;
            classNode.funcs.Clear();

            string[] keyValue = null;
            string[] descLines = null;
            for (int index = 0; index < strList.Length; ++index)
            {
                keyValue = strList[index].Split(new char[] { '(', ')', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length < 1)
                {
                    Debug.Log(string.Format("ParseClass Error: {0}", strList[index]));
                    continue;
                }

                keyValue[0] = keyValue[0].Trim(_trimChars);
                FuncNode funcNode = new FuncNode();
                funcNode.name = keyValue[0];

                if (keyValue.Length == 2)
                {
                    keyValue[1] = keyValue[1].Trim(_trimChars);
                    if (!string.IsNullOrEmpty(keyValue[1]))
                    {
                        funcNode.structName = "AutoGenStructFor_";
                        funcNode.structName += keyValue[0];

                        descLines = keyValue[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        ParseStruct(funcNode.structName, descLines);
                    }
                }
                classNode.funcs.Add(funcNode);
            }

            _arrClasses.Add(classNode);
            return true;
        }

        public void ReadIDLFiles(List<string> files)
        {
            for (int i = 0; i < files.Count; ++i)
            {
                StreamReader sr = File.OpenText(Path.Combine(Application.dataPath, "FlowInLib/Editor/AutoGen/" + files[i]));
                if (sr == null)
                    continue;

                int lineNum = 0;
                int nodeLineNum = 0;
                string line = null;
                string nodeName = null;
                string[] strList = null;
                List<string> descLines = new List<string>();
                bool bStartStruct = false;
                bool bStartClass = false;
                bool bNeedRightBrace = false;

                while ((line = sr.ReadLine()) != null)
                {
                    ++lineNum;
                    line = line.Trim(_trimChars);
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (line.Contains("{"))
                    {
                        if (bNeedRightBrace)
                        {
                            Debug.Log(string.Format("Error(line {0}, file {1}): Last '{' lacks'}'", lineNum, files[i]));
                            break;
                        }
                        bNeedRightBrace = true;
                        continue;
                    }

                    if (line.Contains("}"))
                    {
                        if (bStartStruct)
                        {
                            if (!ParseStruct(nodeName, descLines.ToArray()))
                            {
                                Debug.Log(string.Format("Error(line {0}, file {1}): Can't parse", nodeLineNum, files[i]));
                            }
                            descLines.Clear();
                            bStartStruct = false;
                        }

                        if (bStartClass)
                        {
                            if (!ParseClass(nodeName, descLines.ToArray()))
                            {
                                Debug.Log(string.Format("Error(line {0}, file {1}): Can't parse", nodeLineNum, files[i]));
                            }
                            descLines.Clear();
                            bStartClass = false;
                        }

                        bNeedRightBrace = false;
                        continue;
                    }

                    if (bStartStruct || bStartClass)
                    {
                        descLines.Add(line);
                        continue;
                    }

                    strList = line.Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strList.Length < 2)
                    {
                        Debug.Log(string.Format("Error(line {0}, file {1}): {2}", nodeLineNum, files[i], line));
                        continue;
                    }

                    strList[0] = strList[0].ToLower();
                    if (strList[0] == "struct")
                    {
                        nodeName = strList[1];
                        nodeLineNum = lineNum;
                        bStartStruct = true;
                        continue;
                    }

                    if (strList[0] == "class")
                    {
                        nodeName = strList[1];
                        nodeLineNum = lineNum;
                        bStartClass = true;
                        continue;
                    }
                }

                sr.Close();
            }
        }

        private List<string> _baseTypeSet = new List<string>() { "bool", "float", "double", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong" };
        bool IsBaseType(string type)
        {
            return _baseTypeSet.Contains(type);
        }

        string ToCSType(string type)
        {
            switch (type)
            {
                case "char": return "byte";
                case "int8": return "sbyte";
                case "uint8": return "byte";
                case "int16": return "short";
                case "uint16": return "ushort";
                case "int32": return "int";
                case "uint32": return "uint";
                case "int64": return "long";
                case "uint64": return "ulong";
            }
            return type;
        }
    }

    public class IDLWriter
    {
        private string _codeLines = "";
        private int _curTab = 0;
        private List<string> _endianlessTypeSet = new List<string>() { "bool", "byte", "sbyte" };

        public void WriteIDLCode(IDLReader reader)
        {
            GenerateStructs(reader._arrStructs);
            GenerateClasses(reader._arrStructs, reader._arrClasses);
        }

        void Write(string code)
        {
            for (int j = 0; j < _curTab; ++j)
                _codeLines += '\t';
            _codeLines += code;
            _codeLines += '\n';
        }

        void IncTab()
        {
            ++_curTab;
        }

        void DecTab()
        {
            --_curTab;
            if (_curTab < 0)
                _curTab = 0;
        }

        string GetBaseTypeInitValue(string type)
        {
            switch (type)
            {
                case "bool": return "false";
                case "float":
                case "double": return "0f";
            }
            return "0";
        }

        bool IsEndianLess(string type)
        {
            return _endianlessTypeSet.Contains(type);
        }

        void GenerateStructs(List<StructNode> structs)
        {
            Write("// This file is Auto Generated by IDLAnalyzer");
            Write("// Please don't edit manually");
            Write("using System;");
            Write("using System.Collections.Generic;");
            Write("");

            Write("namespace FlowInLib");
            Write("{");
            IncTab();
            {
                foreach (var stru in structs)
                {
                    Write(string.Format("public class {0} : ISerializableObj", stru.name));
                    Write("{");
                    IncTab();
                    {
                        foreach (var param in stru.paramList)
                        {
                            if (string.IsNullOrEmpty(param.lenParam))
                            {
                                if (param.bBaseType)
                                    Write(string.Format("public {0} {1} = {2};", param.type, param.name, GetBaseTypeInitValue(param.type)));
                                else
                                    Write(string.Format("public {0} {1} = null;", param.type, param.name));
                            }
                            else
                            {
                                Write(string.Format("public List<{0}> {1} = null;", param.type, param.name));
                            }
                        }

                        // 实例化函数
                        Write("");
                        Write("public void CreateInstance()");
                        Write("{");
                        IncTab();
                        {
                            foreach (var param in stru.paramList)
                            {
                                if (string.IsNullOrEmpty(param.lenParam))
                                {
                                    if (!param.bBaseType)
                                        Write(string.Format("{1} = new {0}();", param.type, param.name));
                                }
                                else
                                {
                                    Write(string.Format("{1} = new List<{0}>();", param.type, param.name));
                                }
                            }
                        }
                        DecTab();
                        Write("}");

                        // Clear函数
                        Write("");
                        Write("public void Clear()");
                        Write("{");
                        IncTab();
                        {
                            foreach (var param in stru.paramList)
                            {
                                if (string.IsNullOrEmpty(param.lenParam))
                                {
                                    if (param.bBaseType)
                                        Write(string.Format("{0} = {1};", param.name, GetBaseTypeInitValue(param.type)));
                                    else
                                        Write(string.Format("{0}.Clear();", param.name));
                                }
                                else
                                {
                                    if (!param.bBaseType)
                                        Write(string.Format("foreach (var item in {0}) item.Clear();", param.name));
                                    Write(string.Format("{0}.Clear();", param.name));
                                }
                            }
                        }
                        DecTab();
                        Write("}");

                        // 序列化函数
                        Write("");
                        Write("public uint Serialize (byte[] buff, uint offset, uint buffLen)");
                        Write("{");
                        IncTab();
                        {
                            Write("uint buffPos = offset;");
                            Write("uint copyLen = 0;");

                            foreach (var param in stru.paramList)
                            {
                                Write("");
                                if (string.IsNullOrEmpty(param.lenParam))
                                {
                                    if (param.bBaseType)
                                    {
                                        if (IsEndianLess(param.type))
                                        {
                                            Write(string.Format("buff[buffPos] = {0};", param.name));
                                            Write("buffPos += 1;");
                                        }
                                        else
                                        {
                                            Write(string.Format("copyLen = sizeof({0});", param.type));
                                            Write(string.Format("SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes({0}), 0, copyLen);", param.name));
                                            Write("buffPos += copyLen;");
                                        }
                                    }
                                    else
                                    {
                                        Write(string.Format("copyLen = {0}.Serialize(buff, buffPos, buffLen - (buffPos - offset));", param.name));
                                        Write("buffPos += copyLen;");
                                    }
                                }
                                else
                                {
                                    Write("copyLen = 2;");
                                    Write(string.Format("SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes((ushort){0}.Count), 0, copyLen);", param.name));
                                    Write("buffPos += copyLen;");
                                    Write("");

                                    Write(string.Format("if ({0}.Count > {1}) return 0;", param.name, param.lenMax));
                                    Write(string.Format("if ({0}.Count > 0)", param.name));
                                    Write("{");
                                    IncTab();
                                    {
                                        if (IsEndianLess(param.type))
                                        {
                                            Write(string.Format("for (int i = 0; i < {0}.Count; ++i)", param.name));
                                            IncTab();
                                            Write(string.Format("buff[buffPos + i] = {0}[i];", param.name));
                                            DecTab();
                                            Write(string.Format("buffPos += (uint){0}.Count;", param.name));
                                        }
                                        else
                                        {
                                            if (param.bBaseType)
                                                Write(string.Format("copyLen = sizeof({0});", param.type));

                                            Write(string.Format("for (int i = 0; i < {0}.Count; ++i)", param.name));
                                            Write("{");
                                            IncTab();
                                            {
                                                if (param.bBaseType)
                                                    Write(string.Format("SerializeUtil.LEMemcpy(buff, buffPos, BitConverter.GetBytes({0}[i]), 0, copyLen);", param.name));
                                                else
                                                    Write(string.Format("copyLen = {0}[i].Serialize(buff, buffPos, buffLen - (buffPos - offset));", param.name));
                                                Write("buffPos += copyLen;");
                                            }
                                            DecTab();
                                            Write("}");
                                        }
                                    }
                                    DecTab();
                                    Write("}");
                                }
                            }
                            Write("");
                            Write("return buffPos - offset;");
                        }
                        DecTab();
                        Write("}");

                        // 反序列化函数
                        Write("");
                        Write("public uint Unserialize(byte[] buff, uint offset, uint buffLen)");
                        Write("{");
                        IncTab();
                        {
                            Write("CreateInstance();");
                            Write("uint buffPos = offset;");
                            Write("uint copyLen = 0;");
                            bool bHasTempBytes = false;

                            foreach (var param in stru.paramList)
                            {
                                Write("");
                                if (string.IsNullOrEmpty(param.lenParam))
                                {
                                    if (param.bBaseType)
                                    {
                                        if (!bHasTempBytes)
                                        {
                                            bHasTempBytes = true;
                                            Write("byte[] tempBytes = new byte[8];");
                                        }
                                        Write(string.Format("copyLen = sizeof({0});", param.type));
                                        Write("SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);");
                                        Write(string.Format("{0} = {1}(tempBytes, 0);", param.name, GetConvertString(param.type)));
                                        Write("buffPos += copyLen;");
                                    }
                                    else
                                    {
                                        Write(string.Format("copyLen = {0}.Unserialize(buff, buffPos, buffLen - (buffPos - offset));", param.name));
                                        Write("buffPos += copyLen;");
                                    }
                                }
                                else
                                {
                                    Write("copyLen = 2;");
                                    Write("if (copyLen > buffLen - buffPos) return 0;");
                                    if (!bHasTempBytes)
                                    {
                                        bHasTempBytes = true;
                                        Write("byte[] tempBytes = new byte[8];");
                                    }
                                    Write("SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);");
                                    Write(string.Format("uint {0}Len = (uint)BitConverter.ToUInt16(tempBytes, 0);", param.name));
                                    Write("buffPos += copyLen;");
                                    Write("");

                                    Write(string.Format("if ({0}Len > {1}) return 0;", param.name, param.lenMax));
                                    Write(string.Format("if ({0}Len > 0)", param.name));
                                    Write("{");
                                    IncTab();
                                    {
                                        if (param.bBaseType)
                                        {
                                            if (IsEndianLess(param.type))
                                            {
                                                Write(string.Format("if ({0}Len > buffLen - buffPos) return 0;", param.name));
                                                Write(string.Format("for (int i = 0; i < {0}Len; ++i)", param.name));
                                                IncTab();
                                                Write(string.Format("{0}.Add(buff[buffPos + i]);", param.name));
                                                DecTab();
                                                Write(string.Format("buffPos += {0}Len;", param.name));
                                            }
                                            else
                                            {
                                                Write(string.Format("copyLen = sizeof({0});", param.type));
                                                Write(string.Format("if ({0}Len * copyLen > buffLen - buffPos) return 0;", param.name));
                                                Write(string.Format("for (int i = 0; i < {0}Len; ++i)", param.name));
                                                Write("{");
                                                IncTab();
                                                {
                                                    if (!bHasTempBytes)
                                                    {
                                                        bHasTempBytes = true;
                                                        Write("byte[] tempBytes = new byte[8];");
                                                    }
                                                    Write("SerializeUtil.LEMemcpy(tempBytes, 0, buff, buffPos, copyLen);");
                                                    Write(string.Format("{0} item = {1}(tempBytes, 0);", param.type, GetConvertString(param.type)));
                                                    Write(string.Format("{0}.Add(item);", param.name));
                                                    Write("buffPos += copyLen;");
                                                }
                                                DecTab();
                                                Write("}");
                                            }
                                        }
                                        else
                                        {
                                            Write(string.Format("for (int i = 0; i < {0}Len; ++i)", param.name));
                                            Write("{");
                                            IncTab();
                                            {
                                                Write(string.Format("{0} item = new {0}();", param.type));
                                                Write("copyLen = item.Unserialize(buff, buffPos, buffLen - (buffPos - offset));");
                                                Write(string.Format("{0}.Add(item);", param.name));
                                                Write("buffPos += copyLen;");
                                            }
                                            DecTab();
                                            Write("}");
                                        }
                                    }
                                    DecTab();
                                    Write("}");
                                }
                            }
                            Write("");
                            Write("return buffPos;");
                        }
                        DecTab();
                        Write("}");

                        // 计算字节数函数
                        Write("");
                        Write("public uint CalcDataLen()");
                        Write("{");
                        IncTab();
                        {
                            Write("uint dataLen = 0;");
                            foreach (var param in stru.paramList)
                            {
                                if (string.IsNullOrEmpty(param.lenParam))
                                {
                                    if (param.bBaseType)
                                        Write(string.Format("dataLen += sizeof({0});", param.type));
                                    else
                                        Write(string.Format("dataLen += {0}.CalcDataLen();", param.name));
                                }
                                else
                                {
                                    Write("dataLen += 2;");
                                    if (param.bBaseType)
                                    {
                                        Write(string.Format("dataLen += (uint)(sizeof({0}) * {1}.Count);", param.type, param.name));
                                    }
                                    else
                                    {
                                        Write(string.Format("for (int i = 0; i < {0}.Count; ++i)", param.name));
                                        IncTab();
                                        Write(string.Format("dataLen += {0}[i].CalcDataLen();", param.name));
                                        DecTab();
                                    }
                                }
                            }
                            Write("return dataLen;");
                        }
                        DecTab();
                        Write("}");
                    }
                    DecTab();
                    Write("}");
                }
            }
            DecTab();
            Write("}");
            OutputToFile("MsgDef.cs");
        }

        void GenerateClasses(List<StructNode> structs, List<ClassNode> classes)
        {
            foreach (var cla in classes)
            {
                Write("// This file is Auto Generated by IDLAnalyzer");
                Write("// Please don't edit manually");
                Write("using System;");
                Write("using System.Collections.Generic;");
                Write("");

                Write("namespace FlowInLib");
                Write("{");
                IncTab();
                {
                    Write(string.Format("public class {0} : TcpSession", cla.name));
                    Write("{");
                    IncTab();
                    {
                        Write("public enum EMsgType");
                        Write("{");
                        IncTab();
                        {
                            foreach (var func in cla.funcs) Write(func.name + ",");
                            Write("MaxNum");
                        }
                        DecTab();
                        Write("}");

                        Write("");
                        Write(string.Format("public {0}()", cla.name));
                        Write("{");
                        IncTab();
                        {
                            Write("_rpcCallbackNum = (int)EMsgType.MaxNum;");
                            Write("_rpcCallbackArray = new RpcCallback[_rpcCallbackNum];");
                            foreach (var func in cla.funcs)
                                Write(string.Format("_rpcCallbackArray[(int)EMsgType.{0}] = {0}Stub;", func.name));
                        }
                        DecTab();
                        Write("}");

                        foreach (var func in cla.funcs)
                        {
                            Write("");
                            Write(string.Format("public virtual void {0} (TcpSession session{1}{2}) {{}}", func.name, string.IsNullOrEmpty(func.structName) ? "" : ", ", GetParamString(structs, func.structName)));
                            Write(string.Format("public void {0} ({1})", func.name, GetParamString(structs, func.structName)));
                            Write("{");
                            IncTab();
                            {
                                if (string.IsNullOrEmpty(func.structName))
                                {
                                    Write(string.Format("PushSendData((ushort)EMsgType.{0}, null);", func.name));
                                }
                                else
                                {
                                    StructNode sn = structs.Find(obj => obj.name == func.structName);
                                    if (sn == null)
                                        continue;

                                    Write(string.Format("{0} msgData = new {0}();", func.structName));
                                    foreach (var param in sn.paramList)
                                    {
                                        if (string.IsNullOrEmpty(param.lenParam))
                                            Write(string.Format("msgData.{0} = {0};", param.name));
                                        else
                                            Write(string.Format("msgData.{0}.AddRange({0});", param.name));
                                    }
                                    Write(string.Format("PushSendData((ushort)EMsgType.{0}, msgData);", func.name));
                                }
                                Write("SendBytes();");
                            }
                            DecTab();
                            Write("}");
                        }

                        Write("");
                        foreach (var func in cla.funcs)
                        {
                            Write(string.Format("protected bool {0}Stub(byte[] buff, uint offset, uint buffLen)", func.name));
                            Write("{");
                            IncTab();
                            {
                                if (string.IsNullOrEmpty(func.structName))
                                {
                                    Write(string.Format("{0}();", func.name));
                                    Write("return true;");
                                }
                                else
                                {
                                    Write(string.Format("{0} msgData = new {0}();", func.structName));
                                    Write("uint readLen = msgData.Unserialize(buff, offset, buffLen);");
                                    Write("if (readLen <= 0) { msgData.Clear(); return false; }");
                                    Write(string.Format("{0} (this, {1});", func.name, GetParamString(structs, func.structName, true, "msgData.")));
                                    Write("msgData.Clear();");
                                    Write("return true;");
                                }
                            }
                            DecTab();
                            Write("}");
                        }
                    }
                    DecTab();
                    Write("}");
                }
                DecTab();
                Write("}");
                OutputToFile(string.Format("{0}.cs", cla.name));
            }
        }

        string GetConvertString(string type)
        {
            switch (type)
            {
                case "bool": return "BitConverter.ToBoolean";
                case "float": return "BitConverter.ToSingle";
                case "double": return "BitConverter.ToDouble";
                case "short": return "BitConverter.ToInt16";
                case "ushort": return "BitConverter.ToUInt16";
                case "int": return "BitConverter.ToInt32";
                case "uint": return "BitConverter.ToUInt32";
                case "long": return "BitConverter.ToInt64";
                case "ulong": return "BitConverter.ToUInt64";
            }
            return null;
        }

        string GetParamString(List<StructNode> structs, string structName, bool bNoType = false, string prefix = "")
        {
            StructNode sn = structs.Find(obj => obj.name == structName);
            if (sn == null)
                return "";

            string code = "";
            for (int i = 0; i < sn.paramList.Count; ++i)
            {
                ParamNode param = sn.paramList[i];
                if (i > 0)
                    code += ", ";

                if (bNoType)
                {
                    code += string.Format("{0}{1}", prefix, param.name);
                    continue;
                }

                string type = param.bBaseType ? param.type : param.type;
                if (string.IsNullOrEmpty(param.lenParam))
                    code += string.Format("{0} {1}", type, param.name);
                else
                    code += string.Format("List<{0}> {1}", type, param.name);
            }
            return code;
        }

        void OutputToFile(string file)
        {
            string path = Path.Combine(Application.dataPath, "FlowInLib/Editor/AutoGen/" + file);
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, _codeLines);
            _codeLines = "";
        }
    }

    public class IDLAnalyzer
    {
        [MenuItem("FlowInLib/IDL Gen")]
        static void Main()
        {
            List<string> files = new List<string>();
            files.Add("IDL.txt");

            IDLReader reader = new IDLReader();
            reader.ReadIDLFiles(files);
            IDLWriter writer = new IDLWriter();
            writer.WriteIDLCode(reader);
        }
    }
}