using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using ForceSampleMVC.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ForceSampleMVC.Controllers.Force
{
    [ApiController]
    public class OSSController : ControllerBase,IOss
    {
        private IWebHostEnvironment _env;
        public OSSController(IWebHostEnvironment env) => _env = env;
        public string ClientId => OAuthController.GetAppSetting("FORGE_LIENT_ID");

        /// <summary>
        /// Return list of buckets (id=#) or list of objects (id=bucketKey)
        /// </summary>
        /// API này giúp lấy danh sách các đối tượng bao gồm bucket và object đã được upload lên dựa vào ID
        [HttpGet]
        [Route("api/forge/oss/buckets/{id?}")]
        public async Task<IList<TreeNode>> GetOss(string id="#")
        {
            IList<TreeNode> nodes = new List<TreeNode>();
            // Lấy ra Token để truy cập vào Autodesk Force API
            dynamic oauth = await OAuthController.GetInternalAsync();
            if (id == "#") // root
            {
                // in this case, let's return all buckets
                BucketsApi appBckets = new BucketsApi();
                appBckets.Configuration.AccessToken = oauth.access_token;

                // to simplify, let's return only the first 100 buckets
                dynamic buckets = await appBckets.GetBucketsAsync("US", 100);
                var bucketss = new DynamicDictionaryItems(buckets.items);
                foreach (KeyValuePair<string, dynamic> bucket in bucketss)
                {
                    nodes.Add(new TreeNode(bucket.Value.bucketKey, bucket.Value.bucketKey.Replace(ClientId + "-", string.Empty), "bucket", true));
                }
            }
            else
            {
                // as we have the id (bucketKey), let's return all 
                ObjectsApi objects = new ObjectsApi();
                objects.Configuration.AccessToken = oauth.access_token;
                var objectsList = await objects.GetObjectsAsync(id, 100);
                foreach (KeyValuePair<string, dynamic> objInfo in new DynamicDictionaryItems(objectsList.items))
                {
                    nodes.Add(new TreeNode(Base64Encode((string)objInfo.Value.objectId),
                        objInfo.Value.objectKey, "object", false));
                }
            }
            return nodes;
        }

        // <summary>
        /// Model data for jsTree used on GetOSSAsync
        /// </summary>
        public class TreeNode
        {
            public TreeNode(string id, string text, string type, bool children)
            {
                this.id = id;
                this.text = text;
                this.type = type;
                this.children = children;
            }

            public string id { get; set; }
            public string text { get; set; }
            public string type { get; set; }
            public bool children { get; set; }
        }

        /// <summary>
        /// Create a new bucket 
        /// </summary>
        /// API giúp tạo mới một bucket, Bucket giống như một folder dùng để lưu trữ các file bản vẽ
        [HttpPost]
        [Route("api/forge/oss/buckets")]
        public async Task<dynamic> CreateBucket([FromBody] CreateBucketModel bucket)
        {
            BucketsApi buckets = new BucketsApi();
            dynamic token = await OAuthController.GetInternalAsync();
            buckets.Configuration.AccessToken = token.access_token;
            PostBucketsPayload bucketPayload = new PostBucketsPayload(string.Format("{0}-{1}", ClientId, bucket.bucketKey.ToLower()), null,
              PostBucketsPayload.PolicyKeyEnum.Transient);
            return await buckets.CreateBucketAsync(bucketPayload, "US");
        }

        /// <summary>
        /// Input model for CreateBucket method
        /// </summary>
        public class CreateBucketModel
        {
            public string bucketKey { get; set; }
        }

        /// <summary>
        /// Receive a file from the client and upload to the bucket
        /// </summary>
        /// <returns></returns>
        /// API giúp upload object len server
        [HttpPost]
        [Route("api/forge/oss/objects")]
        public async Task<dynamic> UploadObject([FromForm] UploadFile input)
        {
            // save the file on the server
            var fileSavePath = Path.Combine(_env.WebRootPath, Path.GetFileName(input.fileToUpload.FileName));
            using (var stream = new FileStream(fileSavePath, FileMode.Create))
                await input.fileToUpload.CopyToAsync(stream);


            // get the bucket...
            dynamic oauth = await OAuthController.GetInternalAsync();
            ObjectsApi objects = new ObjectsApi();
            objects.Configuration.AccessToken = oauth.access_token;

            // upload the file/object, which will create a new object
            dynamic uploadedObj;
            using (StreamReader streamReader = new StreamReader(fileSavePath))
            {
                uploadedObj = await objects.UploadObjectAsync(input.bucketKey,
                       Path.GetFileName(input.fileToUpload.FileName), (int)streamReader.BaseStream.Length, streamReader.BaseStream,
                       "application/octet-stream");
            }

            // cleanup
            System.IO.File.Delete(fileSavePath);

            return uploadedObj;
        }

        [HttpPost]
        [Route("api/forge/oss/delete/{objectID?}/{bucketId?}")]
        public async void DeleteFile(string objectID,string bucketId)
        {
            if (string.IsNullOrEmpty(objectID) || string.IsNullOrEmpty(bucketId))
            {
                return;
            }
            //var objectDelete = this.GetOss(objectID);
            var apiInstance = new ObjectsApi();
            dynamic token = await OAuthController.GetInternalAsync();
            apiInstance.Configuration.AccessToken = token.access_token;
            try
            {
                apiInstance.DeleteObject(bucketId, objectID);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ObjectApi.DeleteObject" + e.Message);
            }
           
        }

        public static  async Task<dynamic>  GetObject(string objectKey, string bucketKey)
        {
            var apiInstance = new ObjectsApi();
            dynamic token = await OAuthController.GetInternalAsync();
            apiInstance.Configuration.AccessToken = token;
            var result = apiInstance.GetObjectAsync(bucketKey, objectKey);
            return result;

        }
        public class UploadFile
        {
            public string bucketKey { get; set; }
            public IFormFile fileToUpload { get; set; }
        }

        /// <summary>
        /// Base64 enconde a string
        /// </summary>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}
