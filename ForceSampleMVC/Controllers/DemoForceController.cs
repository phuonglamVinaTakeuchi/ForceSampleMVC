using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ForceSampleMVC.Controllers.Force;
using ForceSampleMVC.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ForceSampleMVC.Controllers
{
    public class DemoForceController : Controller
    {
        private IOss _iOssController;
        public DemoForceController(IOss oss)
        {
            _iOssController = oss;
        }
        //[Route("")]
        public IActionResult Index()
        {
            var dictionTree = new Dictionary<OSSController.TreeNode, List<OSSController.TreeNode>>();
            var data = _iOssController.GetOss();
            if (data!=null)
            {
                var dataList = data.Result;
                foreach (var treeNode in dataList)
                {
                    var childData = _iOssController.GetOss(treeNode.id).Result.ToList();
                    dictionTree.Add(treeNode,childData);
                }
            }
            return View(dictionTree);
        }

        public IActionResult Create()
        {
            return View(new OSSController.CreateBucketModel());
        }

        public IActionResult Upload(string id)
        {
            return View(new OSSController.UploadFile(){bucketKey = id});
        }
        [HttpPost]
        //[Route("Create")]
        public IActionResult Create(OSSController.CreateBucketModel bucket)
        {
            _iOssController.CreateBucket(bucket);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Upload(OSSController.UploadFile uploadFile,IFormFile file)
        {
            if (file==null)
            {
                return View(uploadFile);
            }
            uploadFile.fileToUpload = file;
            var dynamic = await _iOssController.UploadObject(uploadFile);
            //while (_iOssController.UploadObject(uploadFile).Status != TaskStatus.RanToCompletion)
            //{
                
            //}
            return RedirectToAction("Index");
            

            //return View(uploadFile);
        }
        public IActionResult DeleteFile( string objectName,string bucketId)
        {
            //var fileDelete = new FileData() { ObjectName = objectName};
            _iOssController.DeleteFile(objectName,bucketId);
            return RedirectToAction("Index");
        }

        public class FileData
        {
            public string BucketId { get; set; }
            public string ObjectName { get; set; }
        }

    }
}
