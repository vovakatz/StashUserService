using System;
using System.Collections.Generic;
using System.Text;

namespace StashUserService.Model
{
    public class Error
    {
        public Error(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
