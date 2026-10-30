using hw0909.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Infrastructure
{
    public class NullAttachmentStorage : IAttachmentStorage
    {
        public string Save(string originalFileName, byte[] content) => string.Empty;
    }
}