using System.Collections.Generic;
using System.Threading.Tasks;
using ForceSampleMVC.Controllers.Force;
using Microsoft.AspNetCore.Mvc;

namespace ForceSampleMVC.Interface
{
    public interface IOss
    {
       Task<IList<OSSController.TreeNode>> GetOss(string id = "#");
       Task<dynamic> CreateBucket([FromBody] OSSController.CreateBucketModel bucket);
       Task<dynamic> UploadObject([FromForm] OSSController.UploadFile input);
       void DeleteFile(string objectID,string bucketId);
    }
}