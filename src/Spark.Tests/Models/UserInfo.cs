using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Tests.Models
{
    public class UserInfo
    {
        public string Name { get; set; }
        public UserType UserType { get; set; }
    }

    public enum UserType
    {
        Anonymous,
        Registered,
        Administrator,
    }
}
