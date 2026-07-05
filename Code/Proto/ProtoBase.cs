namespace Proto
{
    public class ProtoBase
    {
        public int Idx { get; internal set; }
        protected internal virtual void OnLoaded() { }
        protected internal virtual ValidateResult OnValidate() => ValidateResult.Ok;
        protected internal virtual void SetField(string name, string value) { }
    }
}
