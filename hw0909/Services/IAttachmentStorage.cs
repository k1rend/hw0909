using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hw0909.Services
{
    public interface IAttachmentStorage
    {
        string Save(string originalFileName, byte[] content);
    }
}