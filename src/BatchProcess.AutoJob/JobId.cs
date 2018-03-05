using System;
using System.Collections.Generic;
using System.Text;

namespace BatchProcess.AutoJob
{
    /// <summary>
    /// Enables unique identification to IAutomatedJob
    /// </summary>
    public class JobId : IEquatable<JobId>
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }

        private JobId() { }

        public JobId(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("id, name");

            Id = id;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as JobId;
            if (other != null)
                return Equals(other);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (Id + Name).GetHashCode();
        }

        public bool Equals(JobId other)
        {
            return Id.Equals(other.Id, StringComparison.Ordinal) && Name.Equals(other.Name, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return string.Format("{0}[-]{1}", Id, Name);
        }
    }
}
