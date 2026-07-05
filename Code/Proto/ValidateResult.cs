using System;

namespace Proto
{
    public readonly struct ValidateResult
    {
        public static ValidateResult Ok => new ValidateResult(true, string.Empty, null, 0);
        public static ValidateResult Fail(string reason) => new ValidateResult(false, reason, null, 0);
        internal static ValidateResult Fail(string reason, Type protoType, int rowIdx)
            => new ValidateResult(false, reason, protoType, rowIdx);

        private ValidateResult(bool isValid, string reason, Type? protoType, int rowIdx)
        {
            IsValid = isValid;
            Reason = reason;
            ProtoType = protoType;
            RowIdx = rowIdx;
        }

        public bool IsValid { get; }
        public string Reason { get; }
        public Type? ProtoType { get; }
        public int RowIdx { get; }
    }
}
