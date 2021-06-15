using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Forge.Model;
using ForceSampleMVC.Controllers.Force;
using ForceSampleMVC.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ForceSampleMVC.Controllers.Force.ModelDerivativeController;

namespace ForceSampleMVC.Controllers
{
    public class DemoForceController : Controller
    {
        private IOss _iOssController;
        private IModelDerivative _modelDerivative;
        public DemoForceController(IOss oss,IModelDerivative modelderivate)
        {
            _iOssController = oss;
            _modelDerivative = modelderivate;
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
            return RedirectToAction("Index");
            

            //return View(uploadFile);
        }
        public IActionResult DeleteFile( string objectName,string bucketId)
        {
            //var fileDelete = new FileData() { ObjectName = objectName};
            _iOssController.DeleteFile(objectName,bucketId);
            return RedirectToAction("Index");
        }
        public IActionResult TranslateFile(string objectName,string bucketId)
        {
            var translateObjectModel = new TranslateObjectModel();
            translateObjectModel.bucketKey = bucketId;
            translateObjectModel.objectName = objectName;
            return View(translateObjectModel);
        }
        [HttpPost]
        public async Task<IActionResult> TranslateFile(TranslateObjectModel translatfile)
        {
            if (translatfile == null)
            {
                return View(translatfile);
            }

            var dynamic = await _modelDerivative.TranslateObject(translatfile);
            return RedirectToAction("Index");
        }
        public object ViewIn3D(string objectId)
        {
            var result = _modelDerivative.GetLoadObject(objectId).Result;
           return result;
        }
        public class FileData
        {
            public string BucketId { get; set; }
            public string ObjectName { get; set; }
        }

    }
}
