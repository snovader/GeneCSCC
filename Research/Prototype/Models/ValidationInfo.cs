namespace Prototype.Models
{
    internal sealed class ValidationInfo
    {
        public enum Result { Match, NotRelevant, Relevant }

        private readonly string _type;
        private readonly string _invocation;

        public ValidationInfo(string type, string invocation)
        {
            _type = type;
            _invocation = invocation;
        }

        public Result Validate(string invocation)
        {
            return _invocation.Equals(invocation) ? Result.Match : Result.NotRelevant;
        }

        public Result Validate(ValidationInfo validationInfo)
        {
            return _invocation.Equals(validationInfo._invocation) ? Result.Match
                   : _type.Equals(validationInfo._type) ? Result.Relevant
                   : Result.NotRelevant;
        }

        public override bool Equals(object obj)
        {
            var that = obj as ValidationInfo;
            if (that == null)
            {
                return false;
            }

            return _type.Equals(that._type) && _invocation.Equals(that._invocation);
        }

        public override int GetHashCode()
        {
            var hash = 17;

            unchecked
            {
                hash = hash * 31 + _type.GetHashCode();
                hash = hash * 31 + _invocation.GetHashCode();
            }

            return hash;
        }
    }
}
