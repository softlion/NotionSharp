namespace NotionSharp.ApiClient
{
    public class Bot
    {
        protected bool Equals(Bot other) => true;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Bot) obj);
        }

        public override int GetHashCode() => 0;
    }
}