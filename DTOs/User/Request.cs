using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.User.ValueObjects;

namespace DTOs.User
{
    public class Request
    {
        public Guid Id { get; set; }

        public FirstName FirstName { get; set; }

        public LastName LastName { get; set; }

        public UserName UserName { get; set; }

        public Password Password { get; set; }
    }
}
