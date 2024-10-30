using System;
using System.Collections.Generic;

namespace ClassDocumentGenerator.Lib.DocumentWriter
{
    public class FunctionDetails : IEquatable<FunctionDetails>
    {
        public string FunctionName { get; set; } = string.Empty;

        public string ParentFunction { get; set; } = string.Empty;

        public string FunctionDescription { get; set; } = string.Empty;

        public string SourceFileName { get; set; } = string.Empty;

        /// <summary>
        /// 동일 함수인지 비교를 위한 파라미터 비교 문자열 
        /// </summary>
        public string ParameterFullString { get; set; } = string.Empty;
        
        public List<ParameterInfo> Inputs { get; set; } = new();
        public string InputsString => string.Join(Environment.NewLine, Inputs);
        public ReturnInfo Output { get; set; } = new();
        public string OutputString => Output.ToString();

        public string Process { get; set; } = string.Empty;

        public bool Equals(FunctionDetails other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return FunctionName == other.FunctionName &&
                   SourceFileName == other.SourceFileName &&
                   ParameterFullString == other.ParameterFullString;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FunctionDetails)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FunctionName, ParentFunction, FunctionDescription, SourceFileName, Inputs, Output, Process);
        }


        public static bool operator ==(FunctionDetails left, FunctionDetails right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FunctionDetails left, FunctionDetails right)
        {
            return !Equals(left, right);
        }

        public void Merge(FunctionDetails other)
        {
            if (FunctionName != other.FunctionName) FunctionName = string.Concat(FunctionName, other.FunctionName);
            if (ParentFunction != other.ParentFunction) ParentFunction = string.Concat(ParentFunction, other.ParentFunction);
            if (FunctionDescription != other.FunctionDescription) FunctionDescription = string.Concat(FunctionDescription, other.FunctionDescription);
            if (SourceFileName != other.SourceFileName) SourceFileName = string.Concat(SourceFileName, other.SourceFileName);
            Inputs.AddRange(other.Inputs);
            if (!other.Output.IsEmpty) Output = other.Output;
            if (Process != other.Process) Process = string.Concat(Process, other.Process);
        }
    }
}