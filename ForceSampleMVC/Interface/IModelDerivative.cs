using Autodesk.Forge.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ForceSampleMVC.Controllers.Force.ModelDerivativeController;

namespace ForceSampleMVC.Interface
{
    public interface IModelDerivative
    {
        Task<dynamic> TranslateObject([FromBody] TranslateObjectModel objModel);
        Task<dynamic> GetLoadObject(string urn);
    }
}
