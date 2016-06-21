namespace Prototype.Models
{
    internal sealed class TrainingInfo
    {
        public readonly string Type;
        public readonly AbstractContextInfo ContextInfo;
        public int Index { get; }

        public TrainingInfo(string type, AbstractContextInfo contextInfo, int index = 0)
        {
            Type = type;
            ContextInfo = contextInfo;
            Index = index;
        }

        public TrainingInfo Copy(int index)
        {
            return new TrainingInfo(Type, ContextInfo, index);
        }

        public override bool Equals(object obj)
        {
            var that = obj as TrainingInfo;
            if (that == null)
            {
                return false;
            }

            return Index == that.Index
                   && Type.Equals(that.Type)
                   && ContextInfo.Equals(that.ContextInfo);
        }

        public override int GetHashCode()
        {
            var hash = 17;

            unchecked
            {
                hash = hash * 31 + Type.GetHashCode();
                hash = hash * 31 + ContextInfo.GetHashCode();
                hash = hash * 31 + Index;
            }

            return hash;
        }
    }
}
