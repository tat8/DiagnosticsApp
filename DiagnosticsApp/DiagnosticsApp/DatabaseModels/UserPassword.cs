using System;
using System.Collections.Generic;

namespace DiagnosticsApp.DatabaseModels
{
    public partial class UserPassword
    {
        public long UserId { get; set; }
        public string Password { get; set; }

    }
}
