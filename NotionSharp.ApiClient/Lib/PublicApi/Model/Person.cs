namespace NotionSharp.ApiClient
{
    public class Person
    {
        public string Email { get; set; }

        protected bool Equals(Person other) => Email == other.Email;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Person) obj);
        }

        public override int GetHashCode() => Email.GetHashCode();
    }
}