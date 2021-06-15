using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using ForceSampleMVC.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ForceSampleMVC.Controllers.Force
{
    [ApiController]
    public class ModelDerivativeController : ControllerBase,IModelDerivative
    {
        /// <summary>
        /// Start the translation job for a give bucketKey/objectName
        /// </summary>
        /// <param name="objModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/forge/modelderivative/jobs")]
        public async Task<dynamic> TranslateObject([FromBody] TranslateObjectModel objModel)
        {
            dynamic oauth = await OAuthController.GetInternalAsync();

            // prepare the payload
            List<JobPayloadItem> outputs = new List<JobPayloadItem>()
            {
                new JobPayloadItem(
                    JobPayloadItem.TypeEnum.Svf,
                    new List<JobPayloadItem.ViewsEnum>()
                    {
                        JobPayloadItem.ViewsEnum._2d,
                        JobPayloadItem.ViewsEnum._3d
                    })
            };
            JobPayload job;
            job = new JobPayload(new JobPayloadInput(objModel.objectName), new JobPayloadOutput(outputs));

            // start the translation
            DerivativesApi derivative = new DerivativesApi();
            derivative.Configuration.AccessToken = oauth.access_token;
            dynamic jobPosted = await derivative.TranslateAsync(job);
            return jobPosted;
        }
        [Route("api/forge/modelderivative/jobs")]
        public async Task<dynamic> GetLoadObject(string urn)
        {
            dynamic oauth = await OAuthController.GetInternalAsync();
            DerivativesApi derivative = new DerivativesApi();
            derivative.Configuration.AccessToken = oauth.access_token;
            
                dynamic result = derivative.GetManifest(urn);
                return result;
            
        }

        /// <summary>
        /// Model for TranslateObject method
        /// </summary>
        public class TranslateObjectModel
        {
            public string bucketKey { get; set; }
            public string objectName { get; set; }
        }
    }
}
