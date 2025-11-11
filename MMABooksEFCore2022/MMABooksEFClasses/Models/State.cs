using System;
using System.Collections.Generic;

namespace MMABooksEFClasses.Models
{
    public partial class State
    {
        public State()
        {
            Customers = new HashSet<Customer>();
        }

        public string StateCode { get; set; } = null!;
        public string StateName { get; set; } = null!;

        public virtual ICollection<Customer> Customers { get; set; }
    }
}
